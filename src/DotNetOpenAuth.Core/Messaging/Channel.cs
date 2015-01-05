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
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Net.Sockets;
	using System.Runtime.Serialization.Json;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging.Reflection;
	using Validation;

	/// <summary>
	/// Manages sending direct messages to a remote party and receiving responses.
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Unavoidable.")]
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
		/// Backing field for the <see cref="MaximumIndirectMessageUrlLength"/> property.
		/// </summary>
		private int maximumIndirectMessageUrlLength = Configuration.DotNetOpenAuthSection.Messaging.MaximumIndirectMessageUrlLength;

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel" /> class.
		/// </summary>
		/// <param name="messageTypeProvider">A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.</param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.
		/// The order they are provided is used for outgoing messgaes, and reversed for incoming messages.</param>
		/// <param name="hostFactories">The host factories.</param>
		protected Channel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements, IHostFactories hostFactories) {
			Requires.NotNull(messageTypeProvider, "messageTypeProvider");
			Requires.NotNull(bindingElements, "bindingElements");
			Requires.NotNull(hostFactories, "hostFactories");

			this.messageTypeProvider = messageTypeProvider;
			this.HostFactories = hostFactories;
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
		/// Gets the host factories instance to use.
		/// </summary>
		public IHostFactories HostFactories { get; private set; }

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
				Requires.Range(value >= 500 && value <= 4096, "value");
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
		/// Gets or sets the outgoing message filter.
		/// </summary>
		/// <value>
		/// The outgoing message filter.
		/// </value>
		internal Action<IProtocolMessage> OutgoingMessageFilter { get; set; }

		/// <summary>
		/// Gets or sets the incoming message filter.
		/// </summary>
		/// <value>
		/// The incoming message filter.
		/// </value>
		internal Action<IProtocolMessage> IncomingMessageFilter { get; set; }

		/// <summary>
		/// Gets the binding elements used by this channel, in no particular guaranteed order.
		/// </summary>
		protected internal ReadOnlyCollection<IChannelBindingElement> BindingElements {
			get {
				var result = this.outgoingBindingElements.AsReadOnly();
				Assumes.True(result != null);  // should be an implicit BCL contract
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
		/// Gets or sets the XML dictionary reader quotas.
		/// </summary>
		/// <value>The XML dictionary reader quotas.</value>
		protected virtual XmlDictionaryReaderQuotas XmlDictionaryReaderQuotas { get; set; }

		/// <summary>
		/// Prepares an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		public async Task<HttpResponseMessage> PrepareResponseAsync(IProtocolMessage message, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(message, "message");

			await this.ProcessOutgoingMessageAsync(message, cancellationToken);
			Logger.Channel.DebugFormat("Sending message: {0}", message.GetType().Name);

			HttpResponseMessage result;
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
			result.Headers.CacheControl = new CacheControlHeaderValue {
				NoCache = true,
				NoStore = true,
				MaxAge = TimeSpan.Zero,
				MustRevalidate = true,
			};
			result.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));

			return result;
		}

		/// <summary>
		/// Gets the protocol message embedded in the given HTTP request, if present.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// True if the expected message was recognized and deserialized.  False otherwise.
		/// </returns>
		/// <exception cref="InvalidOperationException">Thrown when <see cref="HttpContext.Current" /> is null.</exception>
		/// <exception cref="ProtocolException">Thrown when a request message of an unexpected type is received.</exception>
		public async Task<TRequest> TryReadFromRequestAsync<TRequest>(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
			where TRequest : class, IProtocolMessage {
			Requires.NotNull(httpRequest, "httpRequest");

			IProtocolMessage untypedRequest = await this.ReadFromRequestAsync(httpRequest, cancellationToken);
			if (untypedRequest == null) {
				return null;
			}

			var request = untypedRequest as TRequest;
			ErrorUtilities.VerifyProtocol(request != null, MessagingStrings.UnexpectedMessageReceived, typeof(TRequest), untypedRequest.GetType());
			return request;
		}

		/// <summary>
		/// Gets the protocol message embedded in the given HTTP request.
		/// </summary>
		/// <typeparam name="TRequest">The expected type of the message to be received.</typeparam>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message.  Never null.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the expected message was not recognized in the response.</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This returns and verifies the appropriate message type.")]
		public async Task<TRequest> ReadFromRequestAsync<TRequest>(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
			where TRequest : class, IProtocolMessage {
			Requires.NotNull(httpRequest, "httpRequest");

			TRequest request = await this.TryReadFromRequestAsync<TRequest>(httpRequest, cancellationToken);
			ErrorUtilities.VerifyProtocol(request != null, MessagingStrings.ExpectedMessageNotReceived, typeof(TRequest));
			return request;
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message, if one is found.  Null otherwise.
		/// </returns>
		public async Task<IDirectedProtocolMessage> ReadFromRequestAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken) {
			Requires.NotNull(httpRequest, "httpRequest");

			if (Logger.Channel.IsInfoEnabled() && httpRequest.RequestUri != null) {
				Logger.Channel.InfoFormat("Scanning incoming request for messages: {0}", httpRequest.RequestUri.AbsoluteUri);
			}
			IDirectedProtocolMessage requestMessage = await this.ReadFromRequestCoreAsync(httpRequest, cancellationToken);
			if (requestMessage != null) {
				Logger.Channel.DebugFormat("Incoming request received: {0}", requestMessage.GetType().Name);

				var directRequest = requestMessage as IHttpDirectRequest;
				if (directRequest != null) {
					foreach (var header in httpRequest.Headers) {
						directRequest.Headers.Add(header.Key, header.Value);
					}
				}

				await this.ProcessIncomingMessageAsync(requestMessage, cancellationToken);
			}

			return requestMessage;
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <typeparam name="TResponse">The expected type of the message to be received.</typeparam>
		/// <param name="requestMessage">The message to send.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The remote party's response.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no message is recognized in the response
		/// or an unexpected type of message is received.</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This returns and verifies the appropriate message type.")]
		public async Task<TResponse> RequestAsync<TResponse>(IDirectedProtocolMessage requestMessage, CancellationToken cancellationToken)
			where TResponse : class, IProtocolMessage {
			Requires.NotNull(requestMessage, "requestMessage");

			IProtocolMessage response = await this.RequestAsync(requestMessage, cancellationToken);
			ErrorUtilities.VerifyProtocol(response != null, MessagingStrings.ExpectedMessageNotReceived, typeof(TResponse));

			var expectedResponse = response as TResponse;
			ErrorUtilities.VerifyProtocol(expectedResponse != null, MessagingStrings.UnexpectedMessageReceived, typeof(TResponse), response.GetType());

			return expectedResponse;
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="requestMessage">The message to send.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The remote party's response.  Guaranteed to never be null.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the response does not include a protocol message.</exception>
		public async Task<IProtocolMessage> RequestAsync(IDirectedProtocolMessage requestMessage, CancellationToken cancellationToken) {
			Requires.NotNull(requestMessage, "requestMessage");

			await this.ProcessOutgoingMessageAsync(requestMessage, cancellationToken);
			Logger.Channel.DebugFormat("Sending {0} request.", requestMessage.GetType().Name);
			var responseMessage = await this.RequestCoreAsync(requestMessage, cancellationToken);
			ErrorUtilities.VerifyProtocol(responseMessage != null, MessagingStrings.ExpectedMessageNotReceived, typeof(IProtocolMessage).Name);

			Logger.Channel.DebugFormat("Received {0} response.", responseMessage.GetType().Name);
			await this.ProcessIncomingMessageAsync(responseMessage, cancellationToken);

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when the message is somehow invalid.
		/// This can be due to tampering, replay attack or expiration, among other things.</exception>
		internal Task ProcessIncomingMessageTestHookAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			return this.ProcessIncomingMessageAsync(message, cancellationToken);
		}

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The <see cref="HttpWebRequest"/> prepared to send the request.</returns>
		/// <remarks>
		/// This method must be overridden by a derived class, unless the <see cref="RequestCoreAsync"/> method
		/// is overridden and does not require this method.
		/// </remarks>
		internal HttpRequestMessage CreateHttpRequestTestHook(IDirectedProtocolMessage request) {
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
		internal HttpResponseMessage PrepareDirectResponseTestHook(IProtocolMessage response) {
			return this.PrepareDirectResponse(response);
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		internal Task<IDictionary<string, string>> ReadFromResponseCoreAsyncTestHook(HttpResponseMessage response, CancellationToken cancellationToken) {
			return this.ReadFromResponseCoreAsync(response, cancellationToken);
		}

		/// <summary>
		/// Prepares a message for transmit by applying signatures, nonces, etc.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		/// <remarks>
		/// This method should NOT be called by derived types
		/// except when sending ONE WAY request messages.
		/// </remarks>
		internal Task ProcessOutgoingMessageTestHookAsync(IProtocolMessage message, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.ProcessOutgoingMessageAsync(message, cancellationToken);
		}

		/// <summary>
		/// Parses the URL encoded form content.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A sequence of key=value pairs found in the request's entity; or an empty sequence if none are found.</returns>
		protected internal static async Task<IEnumerable<KeyValuePair<string, string>>> ParseUrlEncodedFormContentAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			if (request.Content != null && request.Content.Headers.ContentType != null
				&& request.Content.Headers.ContentType.MediaType.Equals(HttpFormUrlEncoded)) {
				return HttpUtility.ParseQueryString(await request.Content.ReadAsStringAsync()).AsKeyValuePairs();
			}

			return Enumerable.Empty<KeyValuePair<string, string>>();
		}

		/// <summary>
		/// Gets the HTTP context for the current HTTP request.
		/// </summary>
		/// <returns>An HttpContextBase instance.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Allocates memory")]
		protected internal virtual HttpContextBase GetHttpContext() {
			RequiresEx.ValidState(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
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
			RequiresEx.ValidState(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);

			Assumes.True(HttpContext.Current.Request.Url != null);
			Assumes.True(HttpContext.Current.Request.RawUrl != null);
			return new HttpRequestWrapper(HttpContext.Current.Request);
		}

		/// <summary>
		/// Adds just the binary data part of a message to a multipart form content object.
		/// </summary>
		/// <param name="requestMessageWithBinaryData">The request message with binary data.</param>
		/// <returns>The initialized HttpContent.</returns>
		protected static MultipartFormDataContent InitializeMultipartFormDataContent(IMessageWithBinaryData requestMessageWithBinaryData) {
			Requires.NotNull(requestMessageWithBinaryData, "requestMessageWithBinaryData");

			var content = new MultipartFormDataContent();
			foreach (var part in requestMessageWithBinaryData.BinaryData) {
				if (string.IsNullOrEmpty(part.Name)) {
					content.Add(part.Content);
				} else if (string.IsNullOrEmpty(part.FileName)) {
					content.Add(part.Content, part.Name);
				} else {
					content.Add(part.Content, part.Name, part.FileName);
				}
			}

			return content;
		}

		/// <summary>
		/// Checks whether a given HTTP method is expected to include an entity body in its request.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <returns><c>true</c> if the HTTP method is supposed to have an entity; <c>false</c> otherwise.</returns>
		protected static bool HttpMethodHasEntity(HttpMethod httpMethod) {
			Requires.NotNull(httpMethod, "httpMethod");

			if (httpMethod == HttpMethod.Get ||
				httpMethod == HttpMethod.Head ||
				httpMethod == HttpMethod.Delete ||
				httpMethod == HttpMethod.Options) {
				return false;
			} else if (httpMethod == HttpMethod.Post ||
				httpMethod == HttpMethod.Put ||
				string.Equals(httpMethod.Method, "PATCH", StringComparison.Ordinal)) {
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
		protected static void ApplyMessageTemplate(IMessage message, HttpResponseMessage response) {
			Requires.NotNull(message, "message");
			var httpMessage = message as IHttpDirectResponse;
			if (httpMessage != null) {
				response.StatusCode = httpMessage.HttpStatusCode;
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
		/// Submits a direct request message to some remote party and blocks waiting for an immediately reply.
		/// </summary>
		/// <param name="request">The request message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response message, or null if the response did not carry a message.</returns>
		/// <remarks>
		/// Typically a deriving channel will override <see cref="CreateHttpRequest"/> to customize this method's
		/// behavior.  However in non-HTTP frameworks, such as unit test mocks, it may be appropriate to override 
		/// this method to eliminate all use of an HTTP transport.
		/// </remarks>
		protected virtual async Task<IProtocolMessage> RequestCoreAsync(IDirectedProtocolMessage request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");
			Requires.That(request.Recipient != null, "request", MessagingStrings.DirectedMessageMissingRecipient);

			if (this.OutgoingMessageFilter != null) {
				this.OutgoingMessageFilter(request);
			}

			var webRequest = this.CreateHttpRequest(request);
			var directRequest = request as IHttpDirectRequest;
			if (directRequest != null) {
				foreach (var header in directRequest.Headers) {
					webRequest.Headers.Add(header.Key, header.Value);
				}
			}

			try {
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.SendAsync(webRequest, cancellationToken)) {
						if (response.Content != null) {
							var responseFields = await this.ReadFromResponseCoreAsync(response, cancellationToken);
							if (responseFields != null) {
								var responseMessage = this.MessageFactory.GetNewResponseMessage(request, responseFields);
								if (responseMessage != null) {
									this.OnReceivingDirectResponse(response, responseMessage);

									var messageAccessor = this.MessageDescriptions.GetAccessor(responseMessage);
									messageAccessor.Deserialize(responseFields);

									return responseMessage;
								}
							}
						}

						if (!response.IsSuccessStatusCode) {
							var errorContent = (response.Content != null) ? await response.Content.ReadAsStringAsync() : null;
							Logger.Http.ErrorFormat(
								"Error received in HTTP response: {0} {1}\n{2}", (int)response.StatusCode, response.ReasonPhrase, errorContent);
							response.EnsureSuccessStatusCode(); // throw so we can wrap it in our catch block.
						}

						return null;
					}
				}
			} catch (HttpRequestException requestException) {
				throw ErrorUtilities.Wrap(requestException, "Error sending HTTP request or receiving response.");
			}
		}

		/// <summary>
		/// Called when receiving a direct response message, before deserialization begins.
		/// </summary>
		/// <param name="response">The HTTP direct response.</param>
		/// <param name="message">The newly instantiated message, prior to deserialization.</param>
		protected virtual void OnReceivingDirectResponse(HttpResponseMessage response, IDirectResponseProtocolMessage message) {
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected virtual async Task<IDirectedProtocolMessage> ReadFromRequestCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			Logger.Channel.DebugFormat("Incoming HTTP request: {0} {1}", request.Method, request.RequestUri.AbsoluteUri);

			var fields = new Dictionary<string, string>();

			// Search Form data first, and if nothing is there search the QueryString
			fields.AddRange(await ParseUrlEncodedFormContentAsync(request, cancellationToken));

			if (fields.Count == 0 && request.Method.Method != "POST") { // OpenID 2.0 section 4.1.2
				fields.AddRange(HttpUtility.ParseQueryString(request.RequestUri.Query).AsKeyValuePairs());
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
		protected virtual HttpResponseMessage PrepareIndirectResponse(IDirectedProtocolMessage message) {
			Requires.NotNull(message, "message");
			Requires.That(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			Requires.That((message.HttpMethods & (HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest)) != 0, "message", "GET or POST expected.");

			Assumes.True(message != null && message.Recipient != null);
			var messageAccessor = this.MessageDescriptions.GetAccessor(message);
			Assumes.True(message != null && message.Recipient != null);
			var fields = messageAccessor.Serialize();

			HttpResponseMessage response = null;
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
				tooLargeForGet = response.Headers.Location.PathAndQuery.Length > this.MaximumIndirectMessageUrlLength;
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
		protected virtual HttpResponseMessage Create301RedirectResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields, bool payloadInFragment = false) {
			Requires.NotNull(message, "message");
			Requires.That(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			Requires.NotNull(fields, "fields");

			// As part of this redirect, we include an HTML body in order to get passed some proxy filters
			// such as WebSense.
			UriBuilder builder = new UriBuilder(message.Recipient);
			if (payloadInFragment) {
				builder.AppendFragmentArgs(fields);
			} else {
				builder.AppendQueryArgs(fields);
			}

			Logger.Http.DebugFormat("Redirecting to {0}", builder.Uri.AbsoluteUri);
			HttpResponseMessage response = new HttpResponseMessageWithOriginal(message) {
				StatusCode = HttpStatusCode.Redirect,
				Content = new StringContent(string.Format(CultureInfo.InvariantCulture, RedirectResponseBodyFormat, builder.Uri.AbsoluteUri)),
			};

			response.Headers.Location = builder.Uri;
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html") { CharSet = "utf-8" };
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
		protected virtual HttpResponseMessage CreateFormPostResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			Requires.NotNull(message, "message");
			Requires.That(message.Recipient != null, "message", MessagingStrings.DirectedMessageMissingRecipient);
			Requires.NotNull(fields, "fields");

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
				HttpResponseMessage response = new HttpResponseMessageWithOriginal(message) {
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(bodyWriter.ToString()),
				};
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

				return response;
			}
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected abstract Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken);

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The <see cref="HttpWebRequest"/> prepared to send the request.</returns>
		/// <remarks>
		/// This method must be overridden by a derived class, unless the <see cref="Channel.RequestCoreAsync"/> method
		/// is overridden and does not require this method.
		/// </remarks>
		protected virtual HttpRequestMessage CreateHttpRequest(IDirectedProtocolMessage request) {
			Requires.NotNull(request, "request");
			Requires.That(request.Recipient != null, "request", MessagingStrings.DirectedMessageMissingRecipient);
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
		protected abstract HttpResponseMessage PrepareDirectResponse(IProtocolMessage response);

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		/// <exception cref="UnprotectedMessageException">Thrown if the message does not have the minimal required protections applied.</exception>
		/// <remarks>
		/// This method should NOT be called by derived types
		/// except when sending ONE WAY request messages.
		/// </remarks>
		protected async Task ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
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
				Assumes.True(bindingElement.Channel != null);
				MessageProtections? elementProtection = await bindingElement.ProcessOutgoingMessageAsync(message, cancellationToken);
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

			if (this.OutgoingMessageFilter != null) {
				this.OutgoingMessageFilter(message);
			}

			if (Logger.Channel.IsInfoEnabled()) {
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
		protected virtual HttpRequestMessage InitializeRequestAsGet(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");
			Requires.That(requestMessage.Recipient != null, "requestMessage", MessagingStrings.DirectedMessageMissingRecipient);

			var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
			var fields = messageAccessor.Serialize();

			UriBuilder builder = new UriBuilder(requestMessage.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			var httpRequest = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
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
		protected virtual HttpRequestMessage InitializeRequestAsHead(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");
			Requires.That(requestMessage.Recipient != null, "requestMessage", MessagingStrings.DirectedMessageMissingRecipient);

			var request = this.InitializeRequestAsGet(requestMessage);
			request.Method = HttpMethod.Head;
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
		protected virtual HttpRequestMessage InitializeRequestAsPost(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");

			var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
			var fields = messageAccessor.Serialize();

			var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestMessage.Recipient);
			this.PrepareHttpWebRequest(httpRequest);

			var requestMessageWithBinaryData = requestMessage as IMessageWithBinaryData;
			if (requestMessageWithBinaryData != null && requestMessageWithBinaryData.SendAsMultipart) {
				var content = InitializeMultipartFormDataContent(requestMessageWithBinaryData);

				// When sending multi-part, all data gets send as multi-part -- even the non-binary data.
				foreach (var field in fields) {
					content.Add(new StringContent(field.Value), field.Key);
				}

				httpRequest.Content = content;
			} else {
				ErrorUtilities.VerifyProtocol(requestMessageWithBinaryData == null || requestMessageWithBinaryData.BinaryData.Count == 0, MessagingStrings.BinaryDataRequiresMultipart);
				httpRequest.Content = new FormUrlEncodedContent(fields);
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
		protected virtual HttpRequestMessage InitializeRequestAsPut(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");

			var request = this.InitializeRequestAsGet(requestMessage);
			request.Method = HttpMethod.Put;
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
		protected virtual HttpRequestMessage InitializeRequestAsDelete(IDirectedProtocolMessage requestMessage) {
			Requires.NotNull(requestMessage, "requestMessage");

			var request = this.InitializeRequestAsGet(requestMessage);
			request.Method = HttpMethod.Delete;
			return request;
		}

		/// <summary>
		/// Verifies the integrity and applicability of an incoming message.
		/// </summary>
		/// <param name="message">The message just received.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		/// <exception cref="UnprotectedMessageException">Thrown if the message does not have the minimal required protections applied.</exception>
		/// <exception cref="ProtocolException">Thrown when the message is somehow invalid.
		/// This can be due to tampering, replay attack or expiration, among other things.</exception>
		protected virtual async Task ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			Requires.NotNull(message, "message");

			if (Logger.Channel.IsInfoEnabled()) {
				var messageAccessor = this.MessageDescriptions.GetAccessor(message, true);
				Logger.Channel.InfoFormat(
					"Processing incoming {0} ({1}) message:{2}{3}",
					message.GetType().Name,
					message.Version,
					Environment.NewLine,
					messageAccessor.ToStringDeferred());
			}

			if (this.IncomingMessageFilter != null) {
				this.IncomingMessageFilter(message);
			}

			MessageProtections appliedProtection = MessageProtections.None;
			foreach (IChannelBindingElement bindingElement in this.IncomingBindingElements) {
				Assumes.True(bindingElement.Channel != null); // CC bug: this.IncomingBindingElements ensures this... why must we assume it here?
				MessageProtections? elementProtection = await bindingElement.ProcessIncomingMessageAsync(message, cancellationToken);
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

			if (Logger.Channel.IsDebugEnabled()) {
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
		protected virtual void PrepareHttpWebRequest(HttpRequestMessage request) {
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
			Requires.NullOrNotNullElements(elements, "elements");
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
