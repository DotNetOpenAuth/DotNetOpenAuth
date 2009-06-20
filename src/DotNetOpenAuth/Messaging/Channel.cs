//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Cache;
	using System.Text;
	using System.Threading;
	using System.Web;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Manages sending direct messages to a remote party and receiving responses.
	/// </summary>
	[ContractVerification(true)]
	[ContractClass(typeof(ChannelContract))]
	public abstract class Channel : IDisposable {
		/// <summary>
		/// The content-type used on HTTP POST requests where the POST entity is a
		/// URL-encoded series of key=value pairs.
		/// </summary>
		protected internal const string HttpFormUrlEncoded = "application/x-www-form-urlencoded";

		/// <summary>
		/// The encoding to use when writing out POST entity strings.
		/// </summary>
		private static readonly Encoding PostEntityEncoding = new UTF8Encoding(false);

		/// <summary>
		/// The maximum allowable size for a 301 Redirect response before we send
		/// a 200 OK response with a scripted form POST with the parameters instead
		/// in order to ensure successfully sending a large payload to another server
		/// that might have a maximum allowable size restriction on its GET request.
		/// </summary>
		private const int IndirectMessageGetToPostThreshold = 2 * 1024; // 2KB, recommended by OpenID group

		/// <summary>
		/// A list of binding elements in the order they must be applied to outgoing messages.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly List<IChannelBindingElement> outgoingBindingElements = new List<IChannelBindingElement>();

		/// <summary>
		/// A list of binding elements in the order they must be applied to incoming messages.
		/// </summary>
		private readonly List<IChannelBindingElement> incomingBindingElements = new List<IChannelBindingElement>();

		/// <summary>
		/// The template for indirect messages that require form POST to forward through the user agent.
		/// </summary>
		/// <remarks>
		/// We are intentionally using " instead of the html single quote ' below because
		/// the HtmlEncode'd values that we inject will only escape the double quote, so
		/// only the double-quote used around these values is safe.
		/// </remarks>
		private const string IndirectMessageFormPostFormat = @"
<html>
<body onload=""var btn = document.getElementById('submit_button'); btn.disabled = true; btn.value = 'Login in progress'; document.getElementById('openid_message').submit()"">
<form id=""openid_message"" action=""{0}"" method=""post"" accept-charset=""UTF-8"" enctype=""application/x-www-form-urlencoded"" onSubmit=""var btn = document.getElementById('submit_button'); btn.disabled = true; btn.value = 'Login in progress'; return true;"">
{1}
	<input id=""submit_button"" type=""submit"" value=""Continue"" />
</form>
</body>
</html>
";

		/// <summary>
		/// The default cache of message descriptions to use unless they are customized.
		/// </summary>
		/// <remarks>
		/// This is a perf optimization, so that we don't reflect over every message type
		/// every time a channel is constructed.
		/// </remarks>
		private static MessageDescriptionCollection defaultMessageDescriptions = new MessageDescriptionCollection();

		/// <summary>
		/// A cache of reflected message types that may be sent or received on this channel.
		/// </summary>
		private MessageDescriptionCollection messageDescriptions = defaultMessageDescriptions;

		/// <summary>
		/// A tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		private IMessageFactory messageTypeProvider;

		/// <summary>
		/// Backing store for the <see cref="CachePolicy"/> property.
		/// </summary>
		private RequestCachePolicy cachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.
		/// </param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.</param>
		protected Channel(IMessageFactory messageTypeProvider, params IChannelBindingElement[] bindingElements) {
			Contract.Requires(messageTypeProvider != null);
			ErrorUtilities.VerifyArgumentNotNull(messageTypeProvider, "messageTypeProvider");

			this.messageTypeProvider = messageTypeProvider;
			this.WebRequestHandler = new StandardWebRequestHandler();
			this.outgoingBindingElements = new List<IChannelBindingElement>(ValidateAndPrepareBindingElements(bindingElements));
			this.incomingBindingElements = new List<IChannelBindingElement>(this.outgoingBindingElements);
			this.incomingBindingElements.Reverse();

			foreach (var element in this.outgoingBindingElements) {
				element.Channel = this;
			}
		}

		/// <summary>
		/// An event fired whenever a message is about to be encoded and sent.
		/// </summary>
		internal event EventHandler<ChannelEventArgs> Sending;

		/// <summary>
		/// Gets or sets an instance to a <see cref="IDirectWebRequestHandler"/> that will be used when 
		/// submitting HTTP requests and waiting for responses.
		/// </summary>
		/// <remarks>
		/// This defaults to a straightforward implementation, but can be set
		/// to a mock object for testing purposes.
		/// </remarks>
		public IDirectWebRequestHandler WebRequestHandler { get; set; }

		/// <summary>
		/// Gets or sets the message descriptions.
		/// </summary>
		internal MessageDescriptionCollection MessageDescriptions {
			get {
				Contract.Ensures(Contract.Result<MessageDescriptionCollection>() != null);
				return this.messageDescriptions;
			}

			set {
				Contract.Requires(value != null);
				ErrorUtilities.VerifyArgumentNotNull(value, "value");
				this.messageDescriptions = value;
			}
		}

		/// <summary>
		/// Gets the binding elements used by this channel, in no particular guaranteed order.
		/// </summary>
		protected internal ReadOnlyCollection<IChannelBindingElement> BindingElements {
			get {
				Contract.Ensures(Contract.Result<ReadOnlyCollection<IChannelBindingElement>>() != null);
				var result = this.outgoingBindingElements.AsReadOnly();
				Contract.Assume(result != null);  // should be an implicit BCL contract
				return result;
			}
		}

		/// <summary>
		/// Gets the binding elements used by this channel, in the order applied to outgoing messages.
		/// </summary>
		protected internal ReadOnlyCollection<IChannelBindingElement> OutgoingBindingElements {
			get { return this.outgoingBindingElements.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the binding elements used by this channel, in the order applied to incoming messages.
		/// </summary>
		protected internal ReadOnlyCollection<IChannelBindingElement> IncomingBindingElements {
			get { return this.incomingBindingElements.AsReadOnly(); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is disposed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
		/// </value>
		protected internal bool IsDisposed { get; set; }

		/// <summary>
		/// Gets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		protected IMessageFactory MessageFactory {
			get { return this.messageTypeProvider; }
		}

		/// <summary>
		/// Gets or sets the cache policy to use for direct message requests.
		/// </summary>
		/// <value>Default is <see cref="HttpRequestCacheLevel.NoCacheNoStore"/>.</value>
		protected RequestCachePolicy CachePolicy {
			get {
				return this.cachePolicy;
			}

			set {
				Contract.Requires(value != null);
				ErrorUtilities.VerifyArgumentNotNull(value, "value");
				this.cachePolicy = value;
			}
		}

		/// <summary>
		/// Sends an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party
		/// and ends execution on the current page or handler.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		/// <exception cref="ThreadAbortException">Thrown by ASP.NET in order to prevent additional data from the page being sent to the client and corrupting the response.</exception>
		/// <remarks>
		/// Requires an HttpContext.Current context.
		/// </remarks>
		public void Send(IProtocolMessage message) {
			Contract.Requires(HttpContext.Current != null);
			Contract.Requires(message != null);
			this.PrepareResponse(message).Send();
		}

		/// <summary>
		/// Prepares an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		public OutgoingWebResponse PrepareResponse(IProtocolMessage message) {
			Contract.Requires(message != null);
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			this.ProcessOutgoingMessage(message);
			Logger.Channel.DebugFormat("Sending message: {0}", message.GetType().Name);

			switch (message.Transport) {
				case MessageTransport.Direct:
					// This is a response to a direct message.
					return this.PrepareDirectResponse(message);
				case MessageTransport.Indirect:
					var directedMessage = message as IDirectedProtocolMessage;
					ErrorUtilities.VerifyArgumentNamed(
						directedMessage != null,
						"message",
						MessagingStrings.IndirectMessagesMustImplementIDirectedProtocolMessage,
						typeof(IDirectedProtocolMessage).FullName);
					ErrorUtilities.VerifyArgumentNamed(
						directedMessage.Recipient != null,
						"message",
						MessagingStrings.DirectedMessageMissingRecipient);
					return this.PrepareIndirectResponse(directedMessage);
				default:
					throw ErrorUtilities.ThrowArgumentNamed(
						"message",
						MessagingStrings.UnrecognizedEnumValue,
						"Transport",
						message.Transport);
			}
		}

		/// <summary>
		/// Gets the protocol message embedded in the given HTTP request, if present.
		/// </summary>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		/// <remarks>
		/// Requires an HttpContext.Current context.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown when <see cref="HttpContext.Current"/> is null.</exception>
		public IDirectedProtocolMessage ReadFromRequest() {
			return this.ReadFromRequest(this.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the protocol message embedded in the given HTTP request, if present.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <param name="request">The deserialized message, if one is found.  Null otherwise.</param>
		/// <returns>True if the expected message was recognized and deserialized.  False otherwise.</returns>
		/// <remarks>
		/// Requires an HttpContext.Current context.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown when <see cref="HttpContext.Current"/> is null.</exception>
		/// <exception cref="ProtocolException">Thrown when a request message of an unexpected type is received.</exception>
		public bool TryReadFromRequest<TRequest>(out TRequest request)
			where TRequest : class, IProtocolMessage {
			return TryReadFromRequest<TRequest>(this.GetRequestFromContext(), out request);
		}

		/// <summary>
		/// Gets the protocol message embedded in the given HTTP request, if present.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <param name="request">The deserialized message, if one is found.  Null otherwise.</param>
		/// <returns>True if the expected message was recognized and deserialized.  False otherwise.</returns>
		/// <exception cref="InvalidOperationException">Thrown when <see cref="HttpContext.Current"/> is null.</exception>
		/// <exception cref="ProtocolException">Thrown when a request message of an unexpected type is received.</exception>
		public bool TryReadFromRequest<TRequest>(HttpRequestInfo httpRequest, out TRequest request)
			where TRequest : class, IProtocolMessage {
			Contract.Requires(httpRequest != null);
			Contract.Ensures(Contract.Result<bool>() == (Contract.ValueAtReturn<TRequest>(out request) != null));

			IProtocolMessage untypedRequest = this.ReadFromRequest(httpRequest);
			if (untypedRequest == null) {
				request = null;
				return false;
			}

			request = untypedRequest as TRequest;
			ErrorUtilities.VerifyProtocol(request != null, MessagingStrings.UnexpectedMessageReceived, typeof(TRequest), untypedRequest.GetType());

			return true;
		}

		/// <summary>
		/// Gets the protocol message embedded in the current HTTP request.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <returns>The deserialized message.  Never null.</returns>
		/// <remarks>
		/// Requires an HttpContext.Current context.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown when <see cref="HttpContext.Current"/> is null.</exception>
		/// <exception cref="ProtocolException">Thrown if the expected message was not recognized in the response.</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This returns and verifies the appropriate message type.")]
		public TRequest ReadFromRequest<TRequest>()
			where TRequest : class, IProtocolMessage {
			return this.ReadFromRequest<TRequest>(this.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the protocol message embedded in the given HTTP request.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <returns>The deserialized message.  Never null.</returns>
		/// <exception cref="ProtocolException">Thrown if the expected message was not recognized in the response.</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This returns and verifies the appropriate message type.")]
		public TRequest ReadFromRequest<TRequest>(HttpRequestInfo httpRequest)
			where TRequest : class, IProtocolMessage {
			Contract.Requires(httpRequest != null);
			TRequest request;
			if (this.TryReadFromRequest<TRequest>(httpRequest, out request)) {
				return request;
			} else {
				throw ErrorUtilities.ThrowProtocol(MessagingStrings.ExpectedMessageNotReceived, typeof(TRequest));
			}
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		public IDirectedProtocolMessage ReadFromRequest(HttpRequestInfo httpRequest) {
			Contract.Requires(httpRequest != null);
			ErrorUtilities.VerifyArgumentNotNull(httpRequest, "httpRequest");

			if (Logger.Channel.IsInfoEnabled && httpRequest.UrlBeforeRewriting != null) {
				Logger.Channel.InfoFormat("Scanning incoming request for messages: {0}", httpRequest.UrlBeforeRewriting.AbsoluteUri);
			}
			IDirectedProtocolMessage requestMessage = this.ReadFromRequestCore(httpRequest);
			if (requestMessage != null) {
				Logger.Channel.DebugFormat("Incoming request received: {0}", requestMessage.GetType().Name);
				this.ProcessIncomingMessage(requestMessage);
			}

			return requestMessage;
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <typeparam name="TResponse">The expected type of the message to be received.</typeparam>
		/// <param name="requestMessage">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		/// <exception cref="ProtocolException">
		/// Thrown if no message is recognized in the response
		/// or an unexpected type of message is received.
		/// </exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This returns and verifies the appropriate message type.")]
		public TResponse Request<TResponse>(IDirectedProtocolMessage requestMessage)
			where TResponse : class, IProtocolMessage {
			Contract.Requires(requestMessage != null);
			Contract.Ensures(Contract.Result<TResponse>() != null);

			IProtocolMessage response = this.Request(requestMessage);
			ErrorUtilities.VerifyProtocol(response != null, MessagingStrings.ExpectedMessageNotReceived, typeof(TResponse));

			var expectedResponse = response as TResponse;
			ErrorUtilities.VerifyProtocol(expectedResponse != null, MessagingStrings.UnexpectedMessageReceived, typeof(TResponse), response.GetType());

			return expectedResponse;
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="requestMessage">The message to send.</param>
		/// <returns>The remote party's response.  Guaranteed to never be null.</returns>
		/// <exception cref="ProtocolException">Thrown if the response does not include a protocol message.</exception>
		public IProtocolMessage Request(IDirectedProtocolMessage requestMessage) {
			Contract.Requires(requestMessage != null);
			ErrorUtilities.VerifyArgumentNotNull(requestMessage, "requestMessage");

			this.ProcessOutgoingMessage(requestMessage);
			Logger.Channel.DebugFormat("Sending {0} request.", requestMessage.GetType().Name);
			var responseMessage = this.RequestCore(requestMessage);
			ErrorUtilities.VerifyProtocol(responseMessage != null, MessagingStrings.ExpectedMessageNotReceived, typeof(IProtocolMessage).Name);

			Logger.Channel.DebugFormat("Received {0} response.", responseMessage.GetType().Name);
			this.ProcessIncomingMessage(responseMessage);

			return responseMessage;
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Gets the current HTTP request being processed.
		/// </summary>
		/// <returns>The HttpRequestInfo for the current request.</returns>
		/// <remarks>
		/// Requires an <see cref="HttpContext.Current"/> context.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Costly call should not be a property.")]
		protected internal virtual HttpRequestInfo GetRequestFromContext() {
			Contract.Ensures(Contract.Result<HttpRequestInfo>() != null);
			ErrorUtilities.VerifyHttpContext();

			return new HttpRequestInfo(HttpContext.Current.Request);
		}

		/// <summary>
		/// Checks whether a given HTTP method is expected to include an entity body in its request.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <returns><c>true</c> if the HTTP method is supposed to have an entity; <c>false</c> otherwise.</returns>
		protected static bool HttpMethodHasEntity(string httpMethod) {
			if (string.Equals(httpMethod, "GET", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "HEAD", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "DELETE", StringComparison.Ordinal)) {
				return false;
			} else if (string.Equals(httpMethod, "POST", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "PUT", StringComparison.Ordinal)) {
				return true;
			} else {
				throw ErrorUtilities.ThrowArgumentNamed("httpMethod", MessagingStrings.UnsupportedHttpVerb, httpMethod);
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				// Call dispose on any binding elements that need it.
				foreach (IDisposable bindingElement in this.BindingElements.OfType<IDisposable>()) {
					bindingElement.Dispose();
				}

				this.IsDisposed = true;
			}
		}

		/// <summary>
		/// Fires the <see cref="Sending"/> event.
		/// </summary>
		/// <param name="message">The message about to be encoded and sent.</param>
		protected virtual void OnSending(IProtocolMessage message) {
			Contract.Requires(message != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			var sending = this.Sending;
			if (sending != null) {
				sending(this, new ChannelEventArgs(message));
			}
		}

		/// <summary>
		/// Gets the direct response of a direct HTTP request.
		/// </summary>
		/// <param name="webRequest">The web request.</param>
		/// <returns>The response to the web request.</returns>
		/// <exception cref="ProtocolException">Thrown on network or protocol errors.</exception>
		protected virtual IncomingWebResponse GetDirectResponse(HttpWebRequest webRequest) {
			Contract.Requires(webRequest != null);
			ErrorUtilities.VerifyArgumentNotNull(webRequest, "webRequest");
			return this.WebRequestHandler.GetResponse(webRequest);
		}

		/// <summary>
		/// Submits a direct request message to some remote party and blocks waiting for an immediately reply.
		/// </summary>
		/// <param name="request">The request message.</param>
		/// <returns>The response message, or null if the response did not carry a message.</returns>
		/// <remarks>
		/// Typically a deriving channel will override <see cref="CreateHttpRequest"/> to customize this method's
		/// behavior.  However in non-HTTP frameworks, such as unit test mocks, it may be appropriate to override 
		/// this method to eliminate all use of an HTTP transport.
		/// </remarks>
		protected virtual IProtocolMessage RequestCore(IDirectedProtocolMessage request) {
			Contract.Requires(request != null);
			HttpWebRequest webRequest = this.CreateHttpRequest(request);
			IDictionary<string, string> responseFields;
			IDirectResponseProtocolMessage responseMessage;

			using (IncomingWebResponse response = this.GetDirectResponse(webRequest)) {
				if (response.ResponseStream == null) {
					return null;
				}

				responseFields = this.ReadFromResponseCore(response);

				responseMessage = this.MessageFactory.GetNewResponseMessage(request, responseFields);
				if (responseMessage == null) {
					return null;
				}

				this.OnReceivingDirectResponse(response, responseMessage);
			}

			var messageAccessor = this.MessageDescriptions.GetAccessor(responseMessage);
			messageAccessor.Deserialize(responseFields);

			return responseMessage;
		}

		/// <summary>
		/// Called when receiving a direct response message, before deserialization begins.
		/// </summary>
		/// <param name="response">The HTTP direct response.</param>
		/// <param name="message">The newly instantiated message, prior to deserialization.</param>
		protected virtual void OnReceivingDirectResponse(IncomingWebResponse response, IDirectResponseProtocolMessage message) {
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected virtual IDirectedProtocolMessage ReadFromRequestCore(HttpRequestInfo request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			Logger.Channel.DebugFormat("Incoming HTTP request: {0} {1}", request.HttpMethod, request.UrlBeforeRewriting.AbsoluteUri);

			// Search Form data first, and if nothing is there search the QueryString
			Contract.Assume(request.Form != null && request.QueryStringBeforeRewriting != null);
			var fields = request.Form.ToDictionary();
			if (fields.Count == 0 && request.HttpMethod != "POST") { // OpenID 2.0 section 4.1.2
				fields = request.QueryStringBeforeRewriting.ToDictionary();
			}

			return (IDirectedProtocolMessage)this.Receive(fields, request.GetRecipient());
		}

		/// <summary>
		/// Deserializes a dictionary of values into a message.
		/// </summary>
		/// <param name="fields">The dictionary of values that were read from an HTTP request or response.</param>
		/// <param name="recipient">Information about where the message was directed.  Null for direct response messages.</param>
		/// <returns>The deserialized message, or null if no message could be recognized in the provided data.</returns>
		protected virtual IProtocolMessage Receive(Dictionary<string, string> fields, MessageReceivingEndpoint recipient) {
			Contract.Requires(fields != null);
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			IProtocolMessage message = this.MessageFactory.GetNewRequestMessage(recipient, fields);

			// If there was no data, or we couldn't recognize it as a message, abort.
			if (message == null) {
				return null;
			}

			// Ensure that the message came in using an allowed HTTP verb for this message type.
			var directedMessage = message as IDirectedProtocolMessage;
			ErrorUtilities.VerifyProtocol(recipient == null || (directedMessage != null && (recipient.AllowedMethods & directedMessage.HttpMethods) != 0), MessagingStrings.UnsupportedHttpVerbForMessageType, message.GetType().Name, recipient.AllowedMethods);

			// We have a message!  Assemble it.
			var messageAccessor = this.MessageDescriptions.GetAccessor(message);
			messageAccessor.Deserialize(fields);

			return message;
		}

		/// <summary>
		/// Queues an indirect message for transmittal via the user agent.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		protected virtual OutgoingWebResponse PrepareIndirectResponse(IDirectedProtocolMessage message) {
			Contract.Requires(message != null && message.Recipient != null);
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			var messageAccessor = this.MessageDescriptions.GetAccessor(message);
			var fields = messageAccessor.Serialize();

			// First try creating a 301 redirect, and fallback to a form POST
			// if the message is too big.
			OutgoingWebResponse response = this.Create301RedirectResponse(message, fields);
			if (response.Headers[HttpResponseHeader.Location].Length > IndirectMessageGetToPostThreshold) {
				response = this.CreateFormPostResponse(message, fields);
			}

			return response;
		}

		/// <summary>
		/// Encodes an HTTP response that will instruct the user agent to forward a message to
		/// some remote third party using a 301 Redirect GET method.
		/// </summary>
		/// <param name="message">The message to forward.</param>
		/// <param name="fields">The pre-serialized fields from the message.</param>
		/// <returns>The encoded HTTP response.</returns>
		protected virtual OutgoingWebResponse Create301RedirectResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			Contract.Requires(message != null && message.Recipient != null);
			Contract.Requires(fields != null);
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			ErrorUtilities.VerifyArgumentNamed(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			WebHeaderCollection headers = new WebHeaderCollection();
			UriBuilder builder = new UriBuilder(message.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			headers.Add(HttpResponseHeader.Location, builder.Uri.AbsoluteUri);
			Logger.Http.DebugFormat("Redirecting to {0}", builder.Uri.AbsoluteUri);
			OutgoingWebResponse response = new OutgoingWebResponse {
				Status = HttpStatusCode.Redirect,
				Headers = headers,
				Body = null,
				OriginalMessage = message
			};

			return response;
		}

		/// <summary>
		/// Encodes an HTTP response that will instruct the user agent to forward a message to
		/// some remote third party using a form POST method.
		/// </summary>
		/// <param name="message">The message to forward.</param>
		/// <param name="fields">The pre-serialized fields from the message.</param>
		/// <returns>The encoded HTTP response.</returns>
		protected virtual OutgoingWebResponse CreateFormPostResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			Contract.Requires(message != null && message.Recipient != null);
			Contract.Requires(fields != null);
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			ErrorUtilities.VerifyArgumentNamed(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			WebHeaderCollection headers = new WebHeaderCollection();
			StringWriter bodyWriter = new StringWriter(CultureInfo.InvariantCulture);
			StringBuilder hiddenFields = new StringBuilder();
			foreach (var field in fields) {
				hiddenFields.AppendFormat(
					"\t<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />\r\n",
					HttpUtility.HtmlEncode(field.Key),
					HttpUtility.HtmlEncode(field.Value));
			}
			bodyWriter.WriteLine(
				IndirectMessageFormPostFormat,
				HttpUtility.HtmlEncode(message.Recipient.AbsoluteUri),
				hiddenFields);
			bodyWriter.Flush();
			OutgoingWebResponse response = new OutgoingWebResponse {
				Status = HttpStatusCode.OK,
				Headers = headers,
				Body = bodyWriter.ToString(),
				OriginalMessage = message
			};

			return response;
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>The deserialized message parts, if found.  Null otherwise.</returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected abstract IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response);

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The <see cref="HttpWebRequest"/> prepared to send the request.</returns>
		/// <remarks>
		/// This method must be overridden by a derived class, unless the <see cref="RequestCore"/> method
		/// is overridden and does not require this method.
		/// </remarks>
		protected virtual HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			Contract.Requires(request != null);
			Contract.Ensures(Contract.Result<HttpWebRequest>() != null);
			throw new NotImplementedException();
		}

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		/// <remarks>
		/// This method implements spec OAuth V1.0 section 5.3.
		/// </remarks>
		protected abstract OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response);

		/// <summary>
		/// Prepares a message for transmit by applying signatures, nonces, etc.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <remarks>
		/// This method should NOT be called by derived types
		/// except when sending ONE WAY request messages.
		/// </remarks>
		protected void ProcessOutgoingMessage(IProtocolMessage message) {
			Contract.Requires(message != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			Logger.Channel.DebugFormat("Preparing to send {0} ({1}) message.", message.GetType().Name, message.Version);
			this.OnSending(message);

			// Give the message a chance to do custom serialization.
			IMessageWithEvents eventedMessage = message as IMessageWithEvents;
			if (eventedMessage != null) {
				eventedMessage.OnSending();
			}

			MessageProtections appliedProtection = MessageProtections.None;
			foreach (IChannelBindingElement bindingElement in this.outgoingBindingElements) {
				MessageProtections? elementProtection = bindingElement.ProcessOutgoingMessage(message);
				if (elementProtection.HasValue) {
					Logger.Bindings.DebugFormat("Binding element {0} applied to message.", bindingElement.GetType().FullName);

					// Ensure that only one protection binding element applies to this message
					// for each protection type.
					ErrorUtilities.VerifyProtocol((appliedProtection & elementProtection.Value) == 0, MessagingStrings.TooManyBindingsOfferingSameProtection, elementProtection.Value);
					appliedProtection |= elementProtection.Value;
				} else {
					Logger.Bindings.DebugFormat("Binding element {0} did not apply to message.", bindingElement.GetType().FullName);
				}
			}

			// Ensure that the message's protection requirements have been satisfied.
			if ((message.RequiredProtection & appliedProtection) != message.RequiredProtection) {
				throw new UnprotectedMessageException(message, appliedProtection);
			}

			this.EnsureValidMessageParts(message);
			message.EnsureValidMessage();

			if (Logger.Channel.IsInfoEnabled) {
				var directedMessage = message as IDirectedProtocolMessage;
				string recipient = (directedMessage != null && directedMessage.Recipient != null) ? directedMessage.Recipient.AbsoluteUri : "<response>";
				var messageAccessor = this.MessageDescriptions.GetAccessor(message);
				Logger.Channel.InfoFormat(
					"Prepared outgoing {0} ({1}) message for {2}: {3}{4}",
					message.GetType().Name,
					message.Version,
					recipient,
					Environment.NewLine,
					messageAccessor.ToStringDeferred());
			}
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the query string in a GET request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method is simply a standard HTTP Get request with the message parts serialized to the query string.
		/// This method satisfies OAuth 1.0 section 5.2, item #3.
		/// </remarks>
		protected virtual HttpWebRequest InitializeRequestAsGet(IDirectedProtocolMessage requestMessage) {
			Contract.Requires(requestMessage != null);
			ErrorUtilities.VerifyArgumentNotNull(requestMessage, "requestMessage");

			var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
			var fields = messageAccessor.Serialize();

			UriBuilder builder = new UriBuilder(requestMessage.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(builder.Uri);

			return httpRequest;
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the payload of a POST request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method is simply a standard HTTP POST request with the message parts serialized to the POST entity
		/// with the application/x-www-form-urlencoded content type
		/// This method satisfies OAuth 1.0 section 5.2, item #2 and OpenID 2.0 section 4.1.2.
		/// </remarks>
		protected virtual HttpWebRequest InitializeRequestAsPost(IDirectedProtocolMessage requestMessage) {
			Contract.Requires(requestMessage != null);
			Contract.Ensures(Contract.Result<HttpWebRequest>() != null);
			ErrorUtilities.VerifyArgumentNotNull(requestMessage, "requestMessage");

			var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
			var fields = messageAccessor.Serialize();

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(requestMessage.Recipient);
			httpRequest.CachePolicy = this.CachePolicy;
			httpRequest.Method = "POST";
			this.SendParametersInEntity(httpRequest, fields);

			return httpRequest;
		}

		/// <summary>
		/// Sends the given parameters in the entity stream of an HTTP request.
		/// </summary>
		/// <param name="httpRequest">The HTTP request.</param>
		/// <param name="fields">The parameters to send.</param>
		/// <remarks>
		/// This method calls <see cref="HttpWebRequest.GetRequestStream()"/> and closes
		/// the request stream, but does not call <see cref="HttpWebRequest.GetResponse"/>.
		/// </remarks>
		protected void SendParametersInEntity(HttpWebRequest httpRequest, IDictionary<string, string> fields) {
			Contract.Requires(httpRequest != null);
			Contract.Requires(fields != null);
			ErrorUtilities.VerifyArgumentNotNull(httpRequest, "httpRequest");
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			httpRequest.ContentType = HttpFormUrlEncoded;

			// Setting the content-encoding to "utf-8" causes Google to reply
			// with a 415 UnsupportedMediaType. But adding it doesn't buy us
			// anything specific, so we disable it until we know how to get it right.
			////httpRequest.Headers[HttpRequestHeader.ContentEncoding] = PostEntityEncoding.WebName;

			string requestBody = MessagingUtilities.CreateQueryString(fields);
			byte[] requestBytes = PostEntityEncoding.GetBytes(requestBody);
			httpRequest.ContentLength = requestBytes.Length;
			Stream requestStream = this.WebRequestHandler.GetRequestStream(httpRequest);
			try {
				requestStream.Write(requestBytes, 0, requestBytes.Length);
			} finally {
				// We need to be sure to close the request stream...
				// unless it is a MemoryStream, which is a clue that we're in
				// a mock stream situation and closing it would preclude reading it later.
				if (!(requestStream is MemoryStream)) {
					requestStream.Dispose();
				}
			}
		}

		/// <summary>
		/// Verifies the integrity and applicability of an incoming message.
		/// </summary>
		/// <param name="message">The message just received.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when the message is somehow invalid.
		/// This can be due to tampering, replay attack or expiration, among other things.
		/// </exception>
		protected virtual void ProcessIncomingMessage(IProtocolMessage message) {
			Contract.Requires(message != null);

			if (Logger.Channel.IsInfoEnabled) {
				var messageAccessor = this.MessageDescriptions.GetAccessor(message);
				Logger.Channel.InfoFormat(
					"Processing incoming {0} ({1}) message:{2}{3}",
					message.GetType().Name,
					message.Version,
					Environment.NewLine,
					messageAccessor.ToStringDeferred());
			}

			MessageProtections appliedProtection = MessageProtections.None;
			foreach (IChannelBindingElement bindingElement in this.incomingBindingElements) {
				MessageProtections? elementProtection = bindingElement.ProcessIncomingMessage(message);
				if (elementProtection.HasValue) {
					Logger.Bindings.DebugFormat("Binding element {0} applied to message.", bindingElement.GetType().FullName);

					// Ensure that only one protection binding element applies to this message
					// for each protection type.
					if ((appliedProtection & elementProtection.Value) != 0) {
						// It turns out that this MAY not be a fatal error condition.  
						// But it may indicate a problem.
						// Specifically, when this RP uses OpenID 1.x to talk to an OP, and both invent
						// their own replay protection for OpenID 1.x, and the OP happens to reuse
						// openid.response_nonce, then this RP may consider both the RP's own nonce and
						// the OP's nonce and "apply" replay protection twice.  This actually isn't a problem.
						Logger.Bindings.WarnFormat(MessagingStrings.TooManyBindingsOfferingSameProtection, elementProtection.Value);
					}

					appliedProtection |= elementProtection.Value;
				} else {
					Logger.Bindings.DebugFormat("Binding element {0} did not apply to message.", bindingElement.GetType().FullName);
				}
			}

			// Ensure that the message's protection requirements have been satisfied.
			if ((message.RequiredProtection & appliedProtection) != message.RequiredProtection) {
				throw new UnprotectedMessageException(message, appliedProtection);
			}

			// Give the message a chance to do custom serialization.
			IMessageWithEvents eventedMessage = message as IMessageWithEvents;
			if (eventedMessage != null) {
				eventedMessage.OnReceiving();
			}

			if (Logger.Channel.IsDebugEnabled) {
				var messageAccessor = this.MessageDescriptions.GetAccessor(message);
				Logger.Channel.DebugFormat(
					"After binding element processing, the received {0} ({1}) message is: {2}{3}",
					message.GetType().Name,
					message.Version,
					Environment.NewLine,
					messageAccessor.ToStringDeferred());
			}

			// We do NOT verify that all required message parts are present here... the 
			// message deserializer did for us.  It would be too late to do it here since
			// they might look initialized by the time we have an IProtocolMessage instance.
			message.EnsureValidMessage();
		}

		/// <summary>
		/// Customizes the binding element order for outgoing and incoming messages.
		/// </summary>
		/// <param name="outgoingOrder">The outgoing order.</param>
		/// <param name="incomingOrder">The incoming order.</param>
		/// <remarks>
		/// No binding elements can be added or removed from the channel using this method.
		/// Only a customized order is allowed.
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown if a binding element is new or missing in one of the ordered lists.</exception>
		protected void CustomizeBindingElementOrder(IEnumerable<IChannelBindingElement> outgoingOrder, IEnumerable<IChannelBindingElement> incomingOrder) {
			Contract.Requires(outgoingOrder != null);
			Contract.Requires(incomingOrder != null);
			ErrorUtilities.VerifyArgumentNotNull(outgoingOrder, "outgoingOrder");
			ErrorUtilities.VerifyArgumentNotNull(incomingOrder, "incomingOrder");
			ErrorUtilities.VerifyArgument(this.IsBindingElementOrderValid(outgoingOrder), MessagingStrings.InvalidCustomBindingElementOrder);
			ErrorUtilities.VerifyArgument(this.IsBindingElementOrderValid(incomingOrder), MessagingStrings.InvalidCustomBindingElementOrder);

			this.outgoingBindingElements.Clear();
			this.outgoingBindingElements.AddRange(outgoingOrder);
			this.incomingBindingElements.Clear();
			this.incomingBindingElements.AddRange(incomingOrder);
		}

		/// <summary>
		/// Ensures a consistent and secure set of binding elements and 
		/// sorts them as necessary for a valid sequence of operations.
		/// </summary>
		/// <param name="elements">The binding elements provided to the channel.</param>
		/// <returns>The properly ordered list of elements.</returns>
		/// <exception cref="ProtocolException">Thrown when the binding elements are incomplete or inconsistent with each other.</exception>
		private static IEnumerable<IChannelBindingElement> ValidateAndPrepareBindingElements(IEnumerable<IChannelBindingElement> elements) {
			Contract.Requires(elements == null || elements.All(e => e != null));
			Contract.Ensures(Contract.Result<IEnumerable<IChannelBindingElement>>() != null);
			if (elements == null) {
				return new IChannelBindingElement[0];
			}

			ErrorUtilities.VerifyArgumentNamed(!elements.Contains(null), "elements", MessagingStrings.SequenceContainsNullElement);

			// Filter the elements between the mere transforming ones and the protection ones.
			var transformationElements = new List<IChannelBindingElement>(
				elements.Where(element => element.Protection == MessageProtections.None));
			var protectionElements = new List<IChannelBindingElement>(
				elements.Where(element => element.Protection != MessageProtections.None));

			bool wasLastProtectionPresent = true;
			foreach (MessageProtections protectionKind in Enum.GetValues(typeof(MessageProtections))) {
				if (protectionKind == MessageProtections.None) {
					continue;
				}

				int countProtectionsOfThisKind = protectionElements.Count(element => (element.Protection & protectionKind) == protectionKind);

				// Each protection binding element is backed by the presence of its dependent protection(s).
				ErrorUtilities.VerifyProtocol(!(countProtectionsOfThisKind > 0 && !wasLastProtectionPresent), MessagingStrings.RequiredProtectionMissing, protectionKind);

				wasLastProtectionPresent = countProtectionsOfThisKind > 0;
			}

			// Put the binding elements in order so they are correctly applied to outgoing messages.
			// Start with the transforming (non-protecting) binding elements first and preserve their original order.
			var orderedList = new List<IChannelBindingElement>(transformationElements);

			// Now sort the protection binding elements among themselves and add them to the list.
			orderedList.AddRange(protectionElements.OrderBy(element => element.Protection, BindingElementOutgoingMessageApplicationOrder));
			return orderedList;
		}

		/// <summary>
		/// Puts binding elements in their correct outgoing message processing order.
		/// </summary>
		/// <param name="protection1">The first protection type to compare.</param>
		/// <param name="protection2">The second protection type to compare.</param>
		/// <returns>
		/// -1 if <paramref name="element1"/> should be applied to an outgoing message before <paramref name="element2"/>.
		/// 1 if <paramref name="element2"/> should be applied to an outgoing message before <paramref name="element1"/>.
		/// 0 if it doesn't matter.
		/// </returns>
		private static int BindingElementOutgoingMessageApplicationOrder(MessageProtections protection1, MessageProtections protection2) {
			ErrorUtilities.VerifyInternal(protection1 != MessageProtections.None || protection2 != MessageProtections.None, "This comparison function should only be used to compare protection binding elements.  Otherwise we change the order of user-defined message transformations.");

			// Now put the protection ones in the right order.
			return -((int)protection1).CompareTo((int)protection2); // descending flag ordinal order
		}

		/// <summary>
		/// Verifies that all required message parts are initialized to values
		/// prior to sending the message to a remote party.
		/// </summary>
		/// <param name="message">The message to verify.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when any required message part does not have a value.
		/// </exception>
		private void EnsureValidMessageParts(IProtocolMessage message) {
			Contract.Requires(message != null);
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(message);
			MessageDescription description = this.MessageDescriptions.Get(message);
			description.EnsureMessagePartsPassBasicValidation(dictionary);
		}

		/// <summary>
		/// Determines whether a given ordered list of binding elements includes every
		/// binding element in this channel exactly once.
		/// </summary>
		/// <param name="order">The list of binding elements to test.</param>
		/// <returns>
		/// 	<c>true</c> if the given list is a valid description of a binding element ordering; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		private bool IsBindingElementOrderValid(IEnumerable<IChannelBindingElement> order) {
			Contract.Requires(order != null);
			ErrorUtilities.VerifyArgumentNotNull(order, "order");

			// Check that the same number of binding elements are defined.
			if (order.Count() != this.OutgoingBindingElements.Count) {
				return false;
			}

			// Check that every binding element appears exactly once.
			if (order.Any(el => !this.OutgoingBindingElements.Contains(el))) {
				return false;
			}

			return true;
		}
	}
}
