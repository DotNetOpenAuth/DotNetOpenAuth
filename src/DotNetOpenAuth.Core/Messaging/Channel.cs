//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Cache;
	using System.Net.Mime;
	using System.Runtime.Serialization.Json;
	using System.Text;
	using System.Threading;
	using System.Web;
	using System.Xml;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Manages sending direct messages to a remote party and receiving responses.
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Unavoidable.")]
	[ContractVerification(true)]
	[ContractClass(typeof(ChannelContract))]
	public abstract class Channel : IDisposable {
		/// <summary>
		/// The encoding to use when writing out POST entity strings.
		/// </summary>
		internal static readonly Encoding PostEntityEncoding = new UTF8Encoding(false);

		/// <summary>
		/// A default set of XML dictionary reader quotas that are relatively safe from causing unbounded memory consumption.
		/// </summary>
		internal static readonly XmlDictionaryReaderQuotas DefaultUntrustedXmlDictionaryReaderQuotas = new XmlDictionaryReaderQuotas {
			MaxArrayLength = 1,
			MaxDepth = 2,
			MaxBytesPerRead = 8 * 1024,
			MaxStringContentLength = 16 * 1024,
		};

		/// <summary>
		/// The content-type used on HTTP POST requests where the POST entity is a
		/// URL-encoded series of key=value pairs.
		/// </summary>
		protected internal const string HttpFormUrlEncoded = "application/x-www-form-urlencoded";

		/// <summary>
		/// The content-type used for JSON serialized objects.
		/// </summary>
		protected internal const string JsonEncoded = "application/json";

		/// <summary>
		/// The "text/javascript" content-type that some servers return instead of the standard <see cref="JsonEncoded"/> one.
		/// </summary>
		protected internal const string JsonTextEncoded = "text/javascript";

		/// <summary>
		/// The content-type for plain text.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PlainText", Justification = "Not 'Plaintext' in the crypographic sense.")]
		protected internal const string PlainTextEncoded = "text/plain";

		/// <summary>
		/// The content-type used on HTTP POST requests where the POST entity is a
		/// URL-encoded series of key=value pairs.
		/// This includes the <see cref="PostEntityEncoding"/> character encoding.
		/// </summary>
		protected internal static readonly ContentType HttpFormUrlEncodedContentType = new ContentType(HttpFormUrlEncoded) { CharSet = PostEntityEncoding.WebName };

		/// <summary>
		/// The HTML that should be returned to the user agent as part of a 301 Redirect.
		/// </summary>
		/// <value>A string that should be used as the first argument to string.Format, where the {0} should be replaced with the URL to redirect to.</value>
		private const string RedirectResponseBodyFormat = @"<html><head><title>Object moved</title></head><body>
<h2>Object moved to <a href=""{0}"">here</a>.</h2>
</body></html>";

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
<head>
</head>
<body onload=""document.body.style.display = 'none'; var btn = document.getElementById('submit_button'); btn.disabled = true; btn.value = 'Login in progress'; document.getElementById('openid_message').submit()"">
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
		/// Backing field for the <see cref="MaximumIndirectMessageUrlLength"/> property.
		/// </summary>
		private int maximumIndirectMessageUrlLength = Configuration.DotNetOpenAuthSection.Messaging.MaximumIndirectMessageUrlLength;

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.
		/// </param>
		/// <param name="bindingElements">
		/// The binding elements to use in sending and receiving messages.
		/// The order they are provided is used for outgoing messgaes, and reversed for incoming messages.
		/// </param>
		protected Channel(IMessageFactory messageTypeProvider, params IChannelBindingElement[] bindingElements) {
			Requires.NotNull(messageTypeProvider, "messageTypeProvider");

			this.messageTypeProvider = messageTypeProvider;
			this.WebRequestHandler = new StandardWebRequestHandler();
			this.XmlDictionaryReaderQuotas = DefaultUntrustedXmlDictionaryReaderQuotas;

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
		/// Gets or sets the maximum allowable size for a 301 Redirect response before we send
		/// a 200 OK response with a scripted form POST with the parameters instead
		/// in order to ensure successfully sending a large payload to another server
		/// that might have a maximum allowable size restriction on its GET request.
		/// </summary>
		/// <value>The default value is 2048.</value>
		public int MaximumIndirectMessageUrlLength {
			get {
				return this.maximumIndirectMessageUrlLength;
			}

			set {
				Requires.InRange(value >= 500 && value <= 4096, "value");
				this.maximumIndirectMessageUrlLength = value;
			}
		}

		/// <summary>
		/// Gets or sets the message descriptions.
		/// </summary>
		internal virtual MessageDescriptionCollection MessageDescriptions {
			get {
				return this.messageDescriptions;
			}

			set {
				Requires.NotNull(value, "value");
				this.messageDescriptions = value;
			}
		}

		/// <summary>
		/// Gets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		internal IMessageFactory MessageFactoryTestHook {
			get { return this.MessageFactory; }
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
			get {
				Contract.Ensures(Contract.Result<ReadOnlyCollection<IChannelBindingElement>>().All(be => be.Channel != null));
				Contract.Ensures(Contract.Result<ReadOnlyCollection<IChannelBindingElement>>().All(be => be != null));
				return this.incomingBindingElements.AsReadOnly();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is disposed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
		/// </value>
		protected internal bool IsDisposed { get; set; }

		/// <summary>
		/// Gets or sets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		protected virtual IMessageFactory MessageFactory {
			get { return this.messageTypeProvider; }
			set { this.messageTypeProvider = value; }
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
				Requires.NotNull(value, "value");
				this.cachePolicy = value;
			}
		}

		/// <summary>
		/// Gets or sets the XML dictionary reader quotas.
		/// </summary>
		/// <value>The XML dictionary reader quotas.</value>
		protected virtual XmlDictionaryReaderQuotas XmlDictionaryReaderQuotas { get; set; }

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
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Send(IProtocolMessage message) {
			Requires.ValidState(HttpContext.Current != null, MessagingStrings.CurrentHttpContextRequired);
			Requires.NotNull(message, "message");
			this.PrepareResponse(message).Respond(HttpContext.Current, true);
		}

		/// <summary>
		/// Sends an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party
		/// and skips most of the remaining ASP.NET request handling pipeline.
		/// Not safe to call from ASP.NET web forms.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		/// <remarks>
		/// Requires an HttpContext.Current context.
		/// This call is not safe to make from an ASP.NET web form (.aspx file or code-behind) because
		/// ASP.NET will render HTML after the protocol message has been sent, which will corrupt the response.
		/// Use the <see cref="Send"/> method instead for web forms.
		/// </remarks>
		public void Respond(IProtocolMessage message) {
			Requires.ValidState(HttpContext.Current != null, MessagingStrings.CurrentHttpContextRequired);
			Requires.NotNull(message, "message");
			this.PrepareResponse(message).Respond();
		}

		/// <summary>
		/// Prepares an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		public OutgoingWebResponse PrepareResponse(IProtocolMessage message) {
			Requires.NotNull(message, "message");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			this.ProcessOutgoingMessage(message);
			Logger.Channel.DebugFormat("Sending message: {0}", message.GetType().Name);

			OutgoingWebResponse result;
			switch (message.Transport) {
				case MessageTransport.Direct:
					// This is a response to a direct message.
					result = this.PrepareDirectResponse(message);
					break;
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
					result = this.PrepareIndirectResponse(directedMessage);
					break;
				default:
					throw ErrorUtilities.ThrowArgumentNamed(
						"message",
						MessagingStrings.UnrecognizedEnumValue,
						"Transport",
						message.Transport);
			}

			// Apply caching policy to any response.  We want to disable all caching because in auth* protocols,
			// caching can be utilized in identity spoofing attacks.
			result.Headers[HttpResponseHeader.CacheControl] = "no-cache, no-store, max-age=0, must-revalidate";
			result.Headers[HttpResponseHeader.Pragma] = "no-cache";

			return result;
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
		public bool TryReadFromRequest<TRequest>(HttpRequestBase httpRequest, out TRequest request)
			where TRequest : class, IProtocolMessage {
			Requires.NotNull(httpRequest, "httpRequest");
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
		public TRequest ReadFromRequest<TRequest>(HttpRequestBase httpRequest)
			where TRequest : class, IProtocolMessage {
			Requires.NotNull(httpRequest, "httpRequest");
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
		public IDirectedProtocolMessage ReadFromRequest(HttpRequestBase httpRequest) {
			Requires.NotNull(httpRequest, "httpRequest");

			if (Logger.Channel.IsInfoEnabled && httpRequest.GetPublicFacingUrl() != null) {
				Logger.Channel.InfoFormat("Scanning incoming request for messages: {0}", httpRequest.GetPublicFacingUrl().AbsoluteUri);
			}
			IDirectedProtocolMessage requestMessage = this.ReadFromRequestCore(httpRequest);
			if (requestMessage != null) {
				Logger.Channel.DebugFormat("Incoming request received: {0}", requestMessage.GetType().Name);

				var directRequest = requestMessage as IHttpDirectRequest;
				if (directRequest != null) {
					foreach (string header in httpRequest.Headers) {
						directRequest.Headers[header] = httpRequest.Headers[header];
					}
				}

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
			Requires.NotNull(requestMessage, "requestMessage");
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
			Requires.NotNull(requestMessage, "requestMessage");

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
		/// Verifies the integrity and applicability of an incoming message.
		/// </summary>
		/// <param name="message">The message just received.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when the message is somehow invalid.
		/// This can be due to tampering, replay attack or expiration, among other things.
		/// </exception>
		internal void ProcessIncomingMessageTestHook(IProtocolMessage message) {
			this.ProcessIncomingMessage(message);
		}

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The <see cref="HttpWebRequest"/> prepared to send the request.</returns>
		/// <remarks>
		/// This method must be overridden by a derived class, unless the <see cref="RequestCore"/> method
		/// is overridden and does not require this method.
		/// </remarks>
		internal HttpWebRequest CreateHttpRequestTestHook(IDirectedProtocolMessage request) {
			return this.CreateHttpRequest(request);
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
		internal OutgoingWebResponse PrepareDirectResponseTestHook(IProtocolMessage response) {
			return this.PrepareDirectResponse(response);
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>The deserialized message parts, if found.  Null otherwise.</returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		internal IDictionary<string, string> ReadFromResponseCoreTestHook(IncomingWebResponse response) {
			return this.ReadFromResponseCore(response);
		}

		/// <remarks>
		/// 	This method should NOT be called by derived types
		/// 	except when sending ONE WAY request messages.
		/// </remarks>
		/// <summary>
		/// Prepares a message for transmit by applying signatures, nonces, etc.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		internal void ProcessOutgoingMessageTestHook(IProtocolMessage message) {
			this.ProcessOutgoingMessage(message);
		}

		/// <summary>
		/// Gets the HTTP context for the current HTTP request.
		/// </summary>
		/// <returns>An HttpContextBase instance.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Allocates memory")]
		protected internal virtual HttpContextBase GetHttpContext() {
			Requires.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
			return new HttpContextWrapper(HttpContext.Current);
		}

		/// <summary>
		/// Gets the current HTTP request being processed.
		/// </summary>
		/// <returns>The HttpRequestInfo for the current request.</returns>
		/// <remarks>
		/// Requires an <see cref="HttpContext.Current"/> context.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Costly call should not be a property.")]
		protected internal virtual HttpRequestBase GetRequestFromContext() {
			Requires.ValidState(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			Contract.Ensures(Contract.Result<HttpRequestBase>() != null);

			Contract.Assume(HttpContext.Current.Request.Url != null);
			Contract.Assume(HttpContext.Current.Request.RawUrl != null);
			return new HttpRequestWrapper(HttpContext.Current.Request);
		}

		/// <summary>
		/// Checks whether a given HTTP method is expected to include an entity body in its request.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <returns><c>true</c> if the HTTP method is supposed to have an entity; <c>false</c> otherwise.</returns>
		protected static bool HttpMethodHasEntity(string httpMethod) {
			if (string.Equals(httpMethod, "GET", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "HEAD", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "DELETE", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "OPTIONS", StringComparison.Ordinal)) {
				return false;
			} else if (string.Equals(httpMethod, "POST", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "PUT", StringComparison.Ordinal) ||
				string.Equals(httpMethod, "PATCH", StringComparison.Ordinal)) {
				return true;
			} else {
				throw ErrorUtilities.ThrowArgumentNamed("httpMethod", MessagingStrings.UnsupportedHttpVerb, httpMethod);
			}
		}

		/// <summary>
		/// Applies message prescribed HTTP response headers to an outgoing web response.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="response">The HTTP response.</param>
		protected static void ApplyMessageTemplate(IMessage message, OutgoingWebResponse response) {
			Requires.NotNull(message, "message");
			var httpMessage = message as IHttpDirectResponse;
			if (httpMessage != null) {
				response.Status = httpMessage.HttpStatusCode;
				foreach (string headerName in httpMessage.Headers) {
					response.Headers.Add(headerName, httpMessage.Headers[headerName]);
				}
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
			Requires.NotNull(message, "message");

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
			Requires.NotNull(webRequest, "webRequest");
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
			Requires.NotNull(request, "request");
			Requires.True(request.Recipient != null, "request", MessagingStrings.DirectedMessageMissingRecipient);

			HttpWebRequest webRequest = this.CreateHttpRequest(request);
			var directRequest = request as IHttpDirectRequest;
			if (directRequest != null) {
				foreach (string header in directRequest.Headers) {
					webRequest.Headers[header] = directRequest.Headers[header];
				}
			}

			IDictionary<string, string> responseFields;
			IDirectResponseProtocolMessage responseMessage;

			using (IncomingWebResponse response = this.GetDirectResponse(webRequest)) {
				if (response.ResponseStream == null) {
					return null;
				}

				responseFields = this.ReadFromResponseCore(response);
				if (responseFields == null) {
					return null;
				}

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
		protected virtual IDirectedProtocolMessage ReadFromRequestCore(HttpRequestBase request) {
			Requires.NotNull(request, "request");

			Logger.Channel.DebugFormat("Incoming HTTP request: {0} {1}", request.HttpMethod, request.GetPublicFacingUrl().AbsoluteUri);

			// Search Form data first, and if nothing is there search the QueryString
			Contract.Assume(request.Form != null && request.GetQueryStringBeforeRewriting() != null);
			var fields = request.Form.ToDictionary();
			if (fields.Count == 0 && request.HttpMethod != "POST") { // OpenID 2.0 section 4.1.2
				fields = request.GetQueryStringBeforeRewriting().ToDictionary();
			}

			MessageReceivingEndpoint recipient;
			try {
				recipient = request.GetRecipient();
			} catch (ArgumentException ex) {
				Logger.Messaging.WarnFormat("Unrecognized HTTP request: {0}", ex);
				return null;
			}

			return (IDirectedProtocolMessage)this.Receive(fields, recipient);
		}

		/// <summary>
		/// Deserializes a dictionary of values into a message.
		/// </summary>
		/// <param name="fields">The dictionary of values that were read from an HTTP request or response.</param>
		/// <param name="recipient">Information about where the message was directed.  Null for direct response messages.</param>
		/// <returns>The deserialized message, or null if no message could be recognized in the provided data.</returns>
		protected virtual IProtocolMessage Receive(Dictionary<string, string> fields, MessageReceivingEndpoint recipient) {
			Requires.NotNull(fields, "fields");

			this.FilterReceivedFields(fields);
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
			Requires.NotNull(message, "message");
			Requires.True(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			Requires.True((message.HttpMethods & (HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest)) != 0, "message");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			Contract.Assert(message != null && message.Recipient != null);
			var messageAccessor = this.MessageDescriptions.GetAccessor(message);
			Contract.Assert(message != null && message.Recipient != null);
			var fields = messageAccessor.Serialize();

			OutgoingWebResponse response = null;
			bool tooLargeForGet = false;
			if ((message.HttpMethods & HttpDeliveryMethods.GetRequest) == HttpDeliveryMethods.GetRequest) {
				bool payloadInFragment = false;
				var httpIndirect = message as IHttpIndirectResponse;
				if (httpIndirect != null) {
					payloadInFragment = httpIndirect.Include301RedirectPayloadInFragment;
				}

				// First try creating a 301 redirect, and fallback to a form POST
				// if the message is too big.
				response = this.Create301RedirectResponse(message, fields, payloadInFragment);
				tooLargeForGet = response.Headers[HttpResponseHeader.Location].Length > this.MaximumIndirectMessageUrlLength;
			}

			// Make sure that if the message is too large for GET that POST is allowed.
			if (tooLargeForGet) {
				ErrorUtilities.VerifyProtocol(
					(message.HttpMethods & HttpDeliveryMethods.PostRequest) == HttpDeliveryMethods.PostRequest,
					MessagingStrings.MessageExceedsGetSizePostNotAllowed);
			}

			// If GET didn't work out, for whatever reason...
			if (response == null || tooLargeForGet) {
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
		/// <param name="payloadInFragment">if set to <c>true</c> the redirect will contain the message payload in the #fragment portion of the URL rather than the ?querystring.</param>
		/// <returns>The encoded HTTP response.</returns>
		[Pure]
		protected virtual OutgoingWebResponse Create301RedirectResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields, bool payloadInFragment = false) {
			Requires.NotNull(message, "message");
			Requires.True(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			Requires.NotNull(fields, "fields");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			// As part of this redirect, we include an HTML body in order to get passed some proxy filters
			// such as WebSense.
			WebHeaderCollection headers = new WebHeaderCollection();
			UriBuilder builder = new UriBuilder(message.Recipient);
			if (payloadInFragment) {
				builder.AppendFragmentArgs(fields);
			} else {
				builder.AppendQueryArgs(fields);
			}

			headers.Add(HttpResponseHeader.Location, builder.Uri.AbsoluteUri);
			headers.Add(HttpResponseHeader.ContentType, "text/html; charset=utf-8");
			Logger.Http.DebugFormat("Redirecting to {0}", builder.Uri.AbsoluteUri);
			OutgoingWebResponse response = new OutgoingWebResponse {
				Status = HttpStatusCode.Redirect,
				Headers = headers,
				Body = string.Format(CultureInfo.InvariantCulture, RedirectResponseBodyFormat, builder.Uri.AbsoluteUri),
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
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		protected virtual OutgoingWebResponse CreateFormPostResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			Requires.NotNull(message, "message");
			Requires.True(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			Requires.NotNull(fields, "fields");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add(HttpResponseHeader.ContentType, "text/html");
			using (StringWriter bodyWriter = new StringWriter(CultureInfo.InvariantCulture)) {
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
		/// This method must be overridden by a derived class, unless the <see cref="Channel.RequestCore"/> method
		/// is overridden and does not require this method.
		/// </remarks>
		protected virtual HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			Requires.NotNull(request, "request");
			Requires.True(request.Recipient != null, "request", MessagingStrings.DirectedMessageMissingRecipient);
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
		/// Serializes the given message as a JSON string.
		/// </summary>
		/// <param name="message">The message to serialize.</param>
		/// <returns>A JSON string.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		protected virtual string SerializeAsJson(IMessage message) {
			Requires.NotNull(message, "message");
			return MessagingUtilities.SerializeAsJson(message, this.MessageDescriptions);
		}

		/// <summary>
		/// Deserializes from flat data from a JSON object.
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <returns>The simple "key":"value" pairs from a JSON-encoded object.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		protected virtual IDictionary<string, string> DeserializeFromJson(string json) {
			Requires.NotNullOrEmpty(json, "json");

			var dictionary = new Dictionary<string, string>();
			using (var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), this.XmlDictionaryReaderQuotas)) {
				MessageSerializer.DeserializeJsonAsFlatDictionary(dictionary, jsonReader);
			}
			return dictionary;
		}

		/// <summary>
		/// Prepares a message for transmit by applying signatures, nonces, etc.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <remarks>
		/// This method should NOT be called by derived types
		/// except when sending ONE WAY request messages.
		/// </remarks>
		protected void ProcessOutgoingMessage(IProtocolMessage message) {
			Requires.NotNull(message, "message");

			Logger.Channel.DebugFormat("Preparing to send {0} ({1}) message.", message.GetType().Name, message.Version);
			this.OnSending(message);

			// Give the message a chance to do custom serialization.
			IMessageWithEvents eventedMessage = message as IMessageWithEvents;
			if (eventedMessage != null) {
				eventedMessage.OnSending();
			}

			MessageProtections appliedProtection = MessageProtections.None;
			foreach (IChannelBindingElement bindingElement in this.outgoingBindingElements) {
				Contract.Assume(bindingElement.Channel != null);
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
			Requires.NotNull(requestMessage, "requestMessage");
			Requires.True(requestMessage.Recipient != null, "requestMessage", MessagingStrings.DirectedMessageMissingRecipient);

			var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
			var fields = messageAccessor.Serialize();

			UriBuilder builder = new UriBuilder(requestMessage.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(builder.Uri);
			this.PrepareHttpWebRequest(httpRequest);

			return httpRequest;
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the query string in a HEAD request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method is simply a standard HTTP HEAD request with the message parts serialized to the query string.
		/// This method satisfies OAuth 1.0 section 5.2, item #3.
		/// </remarks>
		protected virtual HttpWebRequest InitializeRequestAsHead(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");
			Requires.True(requestMessage.Recipient != null, "requestMessage", MessagingStrings.DirectedMessageMissingRecipient);

			HttpWebRequest request = this.InitializeRequestAsGet(requestMessage);
			request.Method = "HEAD";
			return request;
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
			Requires.NotNull(requestMessage, "requestMessage");
			Contract.Ensures(Contract.Result<HttpWebRequest>() != null);

			var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
			var fields = messageAccessor.Serialize();

			var httpRequest = (HttpWebRequest)WebRequest.Create(requestMessage.Recipient);
			this.PrepareHttpWebRequest(httpRequest);
			httpRequest.CachePolicy = this.CachePolicy;
			httpRequest.Method = "POST";

			var requestMessageWithBinaryData = requestMessage as IMessageWithBinaryData;
			if (requestMessageWithBinaryData != null && requestMessageWithBinaryData.SendAsMultipart) {
				var multiPartFields = new List<MultipartPostPart>(requestMessageWithBinaryData.BinaryData);

				// When sending multi-part, all data gets send as multi-part -- even the non-binary data.
				multiPartFields.AddRange(fields.Select(field => MultipartPostPart.CreateFormPart(field.Key, field.Value)));
				this.SendParametersInEntityAsMultipart(httpRequest, multiPartFields);
			} else {
				ErrorUtilities.VerifyProtocol(requestMessageWithBinaryData == null || requestMessageWithBinaryData.BinaryData.Count == 0, MessagingStrings.BinaryDataRequiresMultipart);
				this.SendParametersInEntity(httpRequest, fields);
			}

			return httpRequest;
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the query string in a PUT request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method is simply a standard HTTP PUT request with the message parts serialized to the query string.
		/// </remarks>
		protected virtual HttpWebRequest InitializeRequestAsPut(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");
			Contract.Ensures(Contract.Result<HttpWebRequest>() != null);

			HttpWebRequest request = this.InitializeRequestAsGet(requestMessage);
			request.Method = "PUT";
			return request;
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the query string in a DELETE request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method is simply a standard HTTP DELETE request with the message parts serialized to the query string.
		/// </remarks>
		protected virtual HttpWebRequest InitializeRequestAsDelete(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");
			Contract.Ensures(Contract.Result<HttpWebRequest>() != null);

			HttpWebRequest request = this.InitializeRequestAsGet(requestMessage);
			request.Method = "DELETE";
			return request;
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
			Requires.NotNull(httpRequest, "httpRequest");
			Requires.NotNull(fields, "fields");

			string requestBody = MessagingUtilities.CreateQueryString(fields);
			byte[] requestBytes = PostEntityEncoding.GetBytes(requestBody);
			httpRequest.ContentType = HttpFormUrlEncodedContentType.ToString();
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
		/// Sends the given parameters in the entity stream of an HTTP request in multi-part format.
		/// </summary>
		/// <param name="httpRequest">The HTTP request.</param>
		/// <param name="fields">The parameters to send.</param>
		/// <remarks>
		/// This method calls <see cref="HttpWebRequest.GetRequestStream()"/> and closes
		/// the request stream, but does not call <see cref="HttpWebRequest.GetResponse"/>.
		/// </remarks>
		protected void SendParametersInEntityAsMultipart(HttpWebRequest httpRequest, IEnumerable<MultipartPostPart> fields) {
			httpRequest.PostMultipartNoGetResponse(this.WebRequestHandler, fields);
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
			Requires.NotNull(message, "message");

			if (Logger.Channel.IsInfoEnabled) {
				var messageAccessor = this.MessageDescriptions.GetAccessor(message, true);
				Logger.Channel.InfoFormat(
					"Processing incoming {0} ({1}) message:{2}{3}",
					message.GetType().Name,
					message.Version,
					Environment.NewLine,
					messageAccessor.ToStringDeferred());
			}

			MessageProtections appliedProtection = MessageProtections.None;
			foreach (IChannelBindingElement bindingElement in this.IncomingBindingElements) {
				Contract.Assume(bindingElement.Channel != null); // CC bug: this.IncomingBindingElements ensures this... why must we assume it here?
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
		/// Allows preprocessing and validation of message data before an appropriate message type is
		/// selected or deserialized.
		/// </summary>
		/// <param name="fields">The received message data.</param>
		protected virtual void FilterReceivedFields(IDictionary<string, string> fields) {
		}

		/// <summary>
		/// Performs additional processing on an outgoing web request before it is sent to the remote server.
		/// </summary>
		/// <param name="request">The request.</param>
		protected virtual void PrepareHttpWebRequest(HttpWebRequest request) {
			Requires.NotNull(request, "request");
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
			Requires.NotNull(outgoingOrder, "outgoingOrder");
			Requires.NotNull(incomingOrder, "incomingOrder");
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
			Requires.NullOrWithNoNullElements(elements, "elements");
			Contract.Ensures(Contract.Result<IEnumerable<IChannelBindingElement>>() != null);
			if (elements == null) {
				return new IChannelBindingElement[0];
			}

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
			return orderedList.AsEnumerable();
		}

		/// <summary>
		/// Puts binding elements in their correct outgoing message processing order.
		/// </summary>
		/// <param name="protection1">The first protection type to compare.</param>
		/// <param name="protection2">The second protection type to compare.</param>
		/// <returns>
		/// -1 if <paramref name="protection1"/> should be applied to an outgoing message before <paramref name="protection2"/>.
		/// 1 if <paramref name="protection2"/> should be applied to an outgoing message before <paramref name="protection1"/>.
		/// 0 if it doesn't matter.
		/// </returns>
		private static int BindingElementOutgoingMessageApplicationOrder(MessageProtections protection1, MessageProtections protection2) {
			ErrorUtilities.VerifyInternal(protection1 != MessageProtections.None || protection2 != MessageProtections.None, "This comparison function should only be used to compare protection binding elements.  Otherwise we change the order of user-defined message transformations.");

			// Now put the protection ones in the right order.
			return -((int)protection1).CompareTo((int)protection2); // descending flag ordinal order
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.MessageDescriptions != null);
		}
#endif

		/// <summary>
		/// Verifies that all required message parts are initialized to values
		/// prior to sending the message to a remote party.
		/// </summary>
		/// <param name="message">The message to verify.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when any required message part does not have a value.
		/// </exception>
		private void EnsureValidMessageParts(IProtocolMessage message) {
			Requires.NotNull(message, "message");
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
			Requires.NotNull(order, "order");

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
