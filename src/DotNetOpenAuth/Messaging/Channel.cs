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
	public abstract class Channel : IDisposable {
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
		private static int indirectMessageGetToPostThreshold = 2 * 1024; // 2KB, recommended by OpenID group

		/// <summary>
		/// The template for indirect messages that require form POST to forward through the user agent.
		/// </summary>
		/// <remarks>
		/// We are intentionally using " instead of the html single quote ' below because
		/// the HtmlEncode'd values that we inject will only escape the double quote, so
		/// only the double-quote used around these values is safe.
		/// </remarks>
		private static string indirectMessageFormPostFormat = @"
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
		/// A tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		private IMessageFactory messageTypeProvider;

		/// <summary>
		/// A list of binding elements in the order they must be applied to outgoing messages.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<IChannelBindingElement> outgoingBindingElements = new List<IChannelBindingElement>();

		/// <summary>
		/// A list of binding elements in the order they must be applied to incoming messages.
		/// </summary>
		private List<IChannelBindingElement> incomingBindingElements = new List<IChannelBindingElement>();

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
		/// Gets the binding elements used by this channel, in no particular guaranteed order.
		/// </summary>
		protected internal ReadOnlyCollection<IChannelBindingElement> BindingElements {
			get { return this.outgoingBindingElements.AsReadOnly(); }
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
			this.PrepareResponse(message).Send();
		}

		/// <summary>
		/// Prepares an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		public UserAgentResponse PrepareResponse(IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			this.PrepareMessageForSending(message);
			Logger.DebugFormat("Sending message: {0}", message);

			switch (message.Transport) {
				case MessageTransport.Direct:
					// This is a response to a direct message.
					return this.SendDirectMessageResponse(message);
				case MessageTransport.Indirect:
					var directedMessage = message as IDirectedProtocolMessage;
					if (directedMessage == null) {
						throw new ArgumentException(
							string.Format(
								CultureInfo.CurrentCulture,
								MessagingStrings.IndirectMessagesMustImplementIDirectedProtocolMessage,
								typeof(IDirectedProtocolMessage).FullName),
							"message");
					}
					if (directedMessage.Recipient == null) {
						throw new ArgumentException(MessagingStrings.DirectedMessageMissingRecipient, "message");
					}
					return this.SendIndirectMessage(directedMessage);
				default:
					throw new ArgumentException(
						string.Format(
							CultureInfo.CurrentCulture,
							MessagingStrings.UnrecognizedEnumValue,
							"Transport",
							message.Transport),
						"message");
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
		/// Gets the protocol message embedded in the given HTTP request, if present.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <returns>The deserialized message.</returns>
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
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		/// <exception cref="ProtocolException">Thrown if the expected message was not recognized in the response.</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This returns and verifies the appropriate message type.")]
		public TRequest ReadFromRequest<TRequest>(HttpRequestInfo httpRequest)
			where TRequest : class, IProtocolMessage {
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
			IDirectedProtocolMessage requestMessage = this.ReadFromRequestInternal(httpRequest);
			if (requestMessage != null) {
				Logger.DebugFormat("Incoming request received: {0}", requestMessage);
				this.VerifyMessageAfterReceiving(requestMessage);
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
			ErrorUtilities.VerifyArgumentNotNull(requestMessage, "requestMessage");

			this.PrepareMessageForSending(requestMessage);
			Logger.DebugFormat("Sending request: {0}", requestMessage);
			var responseMessage = this.RequestInternal(requestMessage);
			ErrorUtilities.VerifyProtocol(responseMessage != null, MessagingStrings.ExpectedMessageNotReceived, typeof(IProtocolMessage).Name);

			Logger.DebugFormat("Received message response: {0}", responseMessage);
			this.VerifyMessageAfterReceiving(responseMessage);

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
			ErrorUtilities.VerifyHttpContext();

			return new HttpRequestInfo(HttpContext.Current.Request);
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
		protected virtual DirectWebResponse GetDirectResponse(HttpWebRequest webRequest) {
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
		protected virtual IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			HttpWebRequest webRequest = this.CreateHttpRequest(request);
			IDictionary<string, string> responseFields;

			using (DirectWebResponse response = this.GetDirectResponse(webRequest)) {
				if (response.ResponseStream == null) {
					return null;
				}

				responseFields = this.ReadFromResponseInternal(response);
			}

			IDirectResponseProtocolMessage responseMessage = this.MessageFactory.GetNewResponseMessage(request, responseFields);
			if (responseMessage == null) {
				return null;
			}

			var responseSerializer = MessageSerializer.Get(responseMessage.GetType());
			responseSerializer.Deserialize(responseFields, responseMessage);

			return responseMessage;
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected virtual IDirectedProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			Logger.DebugFormat("Incoming HTTP request: {0}", request.Url.AbsoluteUri);

			// Search Form data first, and if nothing is there search the QueryString
			var fields = request.Form.ToDictionary();
			if (fields.Count == 0) {
				fields = request.QueryString.ToDictionary();
			}

			return (IDirectedProtocolMessage)this.Receive(fields, request.GetRecipient());
		}

		/// <summary>
		/// Deserializes a dictionary of values into a message.
		/// </summary>
		/// <param name="fields">The dictionary of values that were read from an HTTP request or response.</param>
		/// <param name="recipient">Information about where the message was been directed.  Null for direct response messages.</param>
		/// <returns>The deserialized message, or null if no message could be recognized in the provided data.</returns>
		protected virtual IProtocolMessage Receive(Dictionary<string, string> fields, MessageReceivingEndpoint recipient) {
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			IProtocolMessage message = this.MessageFactory.GetNewRequestMessage(recipient, fields);

			// If there was no data, or we couldn't recognize it as a message, abort.
			if (message == null) {
				return null;
			}

			// We have a message!  Assemble it.
			var serializer = MessageSerializer.Get(message.GetType());
			serializer.Deserialize(fields, message);

			return message;
		}

		/// <summary>
		/// Queues an indirect message for transmittal via the user agent.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		protected virtual UserAgentResponse SendIndirectMessage(IDirectedProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			var serializer = MessageSerializer.Get(message.GetType());
			var fields = serializer.Serialize(message);

			// First try creating a 301 redirect, and fallback to a form POST
			// if the message is too big.
			UserAgentResponse response = this.Create301RedirectResponse(message, fields);
			if (response.Headers[HttpResponseHeader.Location].Length > indirectMessageGetToPostThreshold) {
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
		protected virtual UserAgentResponse Create301RedirectResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			ErrorUtilities.VerifyArgumentNamed(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			WebHeaderCollection headers = new WebHeaderCollection();
			UriBuilder builder = new UriBuilder(message.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			headers.Add(HttpResponseHeader.Location, builder.Uri.AbsoluteUri);
			Logger.DebugFormat("Redirecting to {0}", builder.Uri.AbsoluteUri);
			UserAgentResponse response = new UserAgentResponse {
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
		protected virtual UserAgentResponse CreateFormPostResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
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
				indirectMessageFormPostFormat,
				HttpUtility.HtmlEncode(message.Recipient.AbsoluteUri),
				hiddenFields);
			bodyWriter.Flush();
			UserAgentResponse response = new UserAgentResponse {
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
		protected abstract IDictionary<string, string> ReadFromResponseInternal(DirectWebResponse response);

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The <see cref="HttpWebRequest"/> prepared to send the request.</returns>
		/// <remarks>
		/// This method must be overridden by a derived class, unless the <see cref="RequestInternal"/> method
		/// is overridden and does not require this method.
		/// </remarks>
		protected virtual HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		/// <remarks>
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		protected abstract UserAgentResponse SendDirectMessageResponse(IProtocolMessage response);

		/// <summary>
		/// Prepares a message for transmit by applying signatures, nonces, etc.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <remarks>
		/// This method should NOT be called by derived types
		/// except when sending ONE WAY request messages.
		/// </remarks>
		protected void PrepareMessageForSending(IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			Logger.DebugFormat("Preparing to send {0} ({1}) message.", message.GetType().Name, message.Version);
			this.OnSending(message);

			// Give the message a chance to do custom serialization.
			IMessageWithEvents eventedMessage = message as IMessageWithEvents;
			if (eventedMessage != null) {
				eventedMessage.OnSending();
			}

			MessageProtections appliedProtection = MessageProtections.None;
			foreach (IChannelBindingElement bindingElement in this.outgoingBindingElements) {
				if (bindingElement.PrepareMessageForSending(message)) {
					Logger.DebugFormat("Binding element {0} applied to message.", bindingElement.GetType().FullName);

					// Ensure that only one protection binding element applies to this message
					// for each protection type.
					ErrorUtilities.VerifyProtocol((appliedProtection & bindingElement.Protection) == 0, MessagingStrings.TooManyBindingsOfferingSameProtection, bindingElement.Protection);
					appliedProtection |= bindingElement.Protection;
				} else {
					Logger.DebugFormat("Binding element {0} did not apply to message.", bindingElement.GetType().FullName);
				}
			}

			// Ensure that the message's protection requirements have been satisfied.
			if ((message.RequiredProtection & appliedProtection) != message.RequiredProtection) {
				throw new UnprotectedMessageException(message, appliedProtection);
			}

			EnsureValidMessageParts(message);
			message.EnsureValidMessage();

			if (Logger.IsDebugEnabled) {
				Logger.DebugFormat(
					"Sending {0} ({1}) message: {2}{3}",
					message.GetType().Name,
					message.Version,
					Environment.NewLine,
					new MessageDictionary(message).ToStringDeferred());
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
			ErrorUtilities.VerifyArgumentNotNull(requestMessage, "requestMessage");

			var serializer = MessageSerializer.Get(requestMessage.GetType());
			var fields = serializer.Serialize(requestMessage);

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
			ErrorUtilities.VerifyArgumentNotNull(requestMessage, "requestMessage");

			var serializer = MessageSerializer.Get(requestMessage.GetType());
			var fields = serializer.Serialize(requestMessage);

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(requestMessage.Recipient);
			httpRequest.CachePolicy = this.CachePolicy;
			httpRequest.Method = "POST";
			httpRequest.ContentType = "application/x-www-form-urlencoded";
			httpRequest.Headers[HttpRequestHeader.ContentEncoding] = PostEntityEncoding.WebName;
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

			return httpRequest;
		}

		/// <summary>
		/// Verifies the integrity and applicability of an incoming message.
		/// </summary>
		/// <param name="message">The message just received.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when the message is somehow invalid.
		/// This can be due to tampering, replay attack or expiration, among other things.
		/// </exception>
		protected virtual void VerifyMessageAfterReceiving(IProtocolMessage message) {
			Debug.Assert(message != null, "message == null");

			if (Logger.IsDebugEnabled) {
				Logger.DebugFormat(
					"Preparing to receive {0} ({1}) message:{2}{3}",
					message.GetType().Name,
					message.Version,
					Environment.NewLine,
					new MessageDictionary(message).ToStringDeferred());
			}

			MessageProtections appliedProtection = MessageProtections.None;
			foreach (IChannelBindingElement bindingElement in this.incomingBindingElements) {
				if (bindingElement.PrepareMessageForReceiving(message)) {
					Logger.DebugFormat("Binding element {0} applied to message.", bindingElement.GetType().FullName);

					// Ensure that only one protection binding element applies to this message
					// for each protection type.
					ErrorUtilities.VerifyInternal((appliedProtection & bindingElement.Protection) == 0, MessagingStrings.TooManyBindingsOfferingSameProtection, bindingElement.Protection);
					appliedProtection |= bindingElement.Protection;
				} else {
					Logger.DebugFormat("Binding element {0} did not apply to message.", bindingElement.GetType().FullName);
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
		/// Verifies that all required message parts are initialized to values
		/// prior to sending the message to a remote party.
		/// </summary>
		/// <param name="message">The message to verify.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when any required message part does not have a value.
		/// </exception>
		private static void EnsureValidMessageParts(IProtocolMessage message) {
			Debug.Assert(message != null, "message == null");

			MessageDictionary dictionary = new MessageDictionary(message);
			MessageDescription description = MessageDescription.Get(message.GetType(), message.Version);
			description.EnsureMessagePartsPassBasicValidation(dictionary);
		}

		/// <summary>
		/// Ensures a consistent and secure set of binding elements and 
		/// sorts them as necessary for a valid sequence of operations.
		/// </summary>
		/// <param name="elements">The binding elements provided to the channel.</param>
		/// <returns>The properly ordered list of elements.</returns>
		/// <exception cref="ProtocolException">Thrown when the binding elements are incomplete or inconsistent with each other.</exception>
		private static IEnumerable<IChannelBindingElement> ValidateAndPrepareBindingElements(IEnumerable<IChannelBindingElement> elements) {
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
		/// Determines whether a given ordered list of binding elements includes every
		/// binding element in this channel exactly once.
		/// </summary>
		/// <param name="order">The list of binding elements to test.</param>
		/// <returns>
		/// 	<c>true</c> if the given list is a valid description of a binding element ordering; otherwise, <c>false</c>.
		/// </returns>
		private bool IsBindingElementOrderValid(IEnumerable<IChannelBindingElement> order) {
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
