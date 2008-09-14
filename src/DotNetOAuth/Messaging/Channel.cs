//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;

	/// <summary>
	/// Manages sending direct messages to a remote party and receiving responses.
	/// </summary>
	internal abstract class Channel {
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
		private IMessageTypeProvider messageTypeProvider;

		/// <summary>
		/// Gets or sets the HTTP response to send as a reply to the current incoming HTTP request.
		/// </summary>
		private Response queuedIndirectOrResponseMessage;

		/// <summary>
		/// A list of binding elements in the order they must be applied to outgoing messages.
		/// </summary>
		/// <remarks>
		/// Incoming messages should have the binding elements applied in reverse order.
		/// </remarks>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<IChannelBindingElement> bindingElements = new List<IChannelBindingElement>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.
		/// </param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.</param>
		protected Channel(IMessageTypeProvider messageTypeProvider, params IChannelBindingElement[] bindingElements) {
			if (messageTypeProvider == null) {
				throw new ArgumentNullException("messageTypeProvider");
			}

			this.messageTypeProvider = messageTypeProvider;
			this.bindingElements = new List<IChannelBindingElement>(ValidateAndPrepareBindingElements(bindingElements));
		}

		/// <summary>
		/// Gets the binding elements used by this channel, in the order they are applied to outgoing messages.
		/// </summary>
		/// <remarks>
		/// Incoming messages are processed by this binding elements in the reverse order.
		/// </remarks>
		protected internal ReadOnlyCollection<IChannelBindingElement> BindingElements {
			get {
				return this.bindingElements.AsReadOnly();
			}
		}

		/// <summary>
		/// Gets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		protected IMessageTypeProvider MessageTypeProvider {
			get { return this.messageTypeProvider; }
		}

		/// <summary>
		/// Retrieves the stored response for sending and clears it from the channel.
		/// </summary>
		/// <returns>The response to send as the HTTP response.</returns>
		internal Response DequeueIndirectOrResponseMessage() {
			Response response = this.queuedIndirectOrResponseMessage;
			this.queuedIndirectOrResponseMessage = null;
			return response;
		}

		/// <summary>
		/// Queues an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		internal void Send(IProtocolMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}
			this.PrepareMessageForSending(message);

			switch (message.Transport) {
				case MessageTransport.Direct:
					// This is a response to a direct message.
					this.SendDirectMessageResponse(message);
					break;
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
					this.SendIndirectMessage(directedMessage);
					break;
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
		internal IProtocolMessage ReadFromRequest() {
			if (HttpContext.Current == null) {
				throw new InvalidOperationException(MessagingStrings.HttpContextRequired);
			}

			return this.ReadFromRequest(new HttpRequestInfo(HttpContext.Current.Request));
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="httpRequest">The request to search for an embedded message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected internal IProtocolMessage ReadFromRequest(HttpRequestInfo httpRequest) {
			IProtocolMessage requestMessage = this.ReadFromRequestInternal(httpRequest);
			this.VerifyMessageAfterReceiving(requestMessage);
			return requestMessage;
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		protected internal IProtocolMessage Request(IDirectedProtocolMessage request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			this.PrepareMessageForSending(request);
			IProtocolMessage response = this.RequestInternal(request);
			if (response != null) {
				this.VerifyMessageAfterReceiving(response);
			}

			return response;
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response stream.
		/// </summary>
		/// <param name="responseStream">The response that is anticipated to contain an OAuth message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected internal IProtocolMessage ReadFromResponse(Stream responseStream) {
			IProtocolMessage message = this.ReadFromResponseInternal(responseStream);
			this.VerifyMessageAfterReceiving(message);
			return message;
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected virtual IProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			// Search Form data first, and if nothing is there search the QueryString
			var fields = request.Form.ToDictionary();
			if (fields.Count == 0) {
				fields = request.QueryString.ToDictionary();
			}

			return this.Receive(fields);
		}

		/// <summary>
		/// Deserializes a dictionary of values into a message.
		/// </summary>
		/// <param name="fields">The dictionary of values that were read from an HTTP request or response.</param>
		/// <returns>The deserialized message, or null if no message could be recognized in the provided data.</returns>
		protected virtual IProtocolMessage Receive(Dictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			Type messageType = this.MessageTypeProvider.GetRequestMessageType(fields);

			// If there was no data, or we couldn't recognize it as a message, abort.
			if (messageType == null) {
				return null;
			}

			// We have a message!  Assemble it.
			var serializer = MessageSerializer.Get(messageType);
			IProtocolMessage message = serializer.Deserialize(fields);

			return message;
		}

		/// <summary>
		/// Queues an indirect message for transmittal via the user agent.
		/// </summary>
		/// <param name="message">The message to send.</param>
		protected virtual void SendIndirectMessage(IDirectedProtocolMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			var serializer = MessageSerializer.Get(message.GetType());
			var fields = serializer.Serialize(message);
			Response response;
			if (CalculateSizeOfPayload(fields) > indirectMessageGetToPostThreshold) {
				response = this.CreateFormPostResponse(message, fields);
			} else {
				response = this.Create301RedirectResponse(message, fields);
			}

			this.QueueIndirectOrResponseMessage(response);
		}

		/// <summary>
		/// Encodes an HTTP response that will instruct the user agent to forward a message to
		/// some remote third party using a 301 Redirect GET method.
		/// </summary>
		/// <param name="message">The message to forward.</param>
		/// <param name="fields">The pre-serialized fields from the message.</param>
		/// <returns>The encoded HTTP response.</returns>
		protected virtual Response Create301RedirectResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}
			if (message.Recipient == null) {
				throw new ArgumentException(MessagingStrings.DirectedMessageMissingRecipient, "message");
			}
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			WebHeaderCollection headers = new WebHeaderCollection();
			UriBuilder builder = new UriBuilder(message.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			headers.Add(HttpResponseHeader.Location, builder.Uri.AbsoluteUri);
			Logger.DebugFormat("Redirecting to {0}", builder.Uri.AbsoluteUri);
			Response response = new Response {
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
		protected virtual Response CreateFormPostResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}
			if (message.Recipient == null) {
				throw new ArgumentException(MessagingStrings.DirectedMessageMissingRecipient, "message");
			}
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			WebHeaderCollection headers = new WebHeaderCollection();
			StringWriter bodyWriter = new StringWriter();
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
			Response response = new Response {
				Status = HttpStatusCode.OK,
				Headers = headers,
				Body = bodyWriter.ToString(),
				OriginalMessage = message
			};

			return response;
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response stream.
		/// </summary>
		/// <param name="responseStream">The response that is anticipated to contain an OAuth message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected abstract IProtocolMessage ReadFromResponseInternal(Stream responseStream);

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		protected abstract IProtocolMessage RequestInternal(IDirectedProtocolMessage request);

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <remarks>
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		protected abstract void SendDirectMessageResponse(IProtocolMessage response);

		/// <summary>
		/// Takes a message and temporarily stores it for sending as the hosting site's
		/// HTTP response to the current request.
		/// </summary>
		/// <param name="response">The message to store for sending.</param>
		protected void QueueIndirectOrResponseMessage(Response response) {
			if (response == null) {
				throw new ArgumentNullException("response");
			}
			if (this.queuedIndirectOrResponseMessage != null) {
				throw new InvalidOperationException(MessagingStrings.QueuedMessageResponseAlreadyExists);
			}

			this.queuedIndirectOrResponseMessage = response;
		}

		/// <summary>
		/// Calculates a fairly accurate estimation on the size of a message that contains
		/// a given set of fields.
		/// </summary>
		/// <param name="fields">The fields that would be included in a message.</param>
		/// <returns>The size (in bytes) of the message payload.</returns>
		private static int CalculateSizeOfPayload(IDictionary<string, string> fields) {
			Debug.Assert(fields != null, "fields == null");

			int size = 0;
			foreach (var field in fields) {
				size += field.Key.Length;
				size += field.Value.Length;
				size += 2; // & and =
			}
			return size;
		}

		/// <summary>
		/// Ensures a consistent and secure set of binding elements and 
		/// sorts them as necessary for a valid sequence of operations.
		/// </summary>
		/// <param name="elements">The binding elements provided to the channel.</param>
		/// <returns>The properly ordered list of elements.</returns>
		/// <exception cref="ProtocolException">Thrown when the binding elements are incomplete or inconsistent with each other.</exception>
		private static IEnumerable<IChannelBindingElement> ValidateAndPrepareBindingElements(IEnumerable<IChannelBindingElement> elements) {
			// Filter the elements between the mere transforming ones and the protection ones.
			var transformationElements = new List<IChannelBindingElement>(
				elements.Where(element => element.Protection == MessageProtection.None));
			var protectionElements = new List<IChannelBindingElement>(
				elements.Where(element => element.Protection != MessageProtection.None));

			bool wasLastProtectionPresent = true;
			foreach (MessageProtection protectionKind in Enum.GetValues(typeof(MessageProtection))) {
				if (protectionKind == MessageProtection.None) {
					continue;
				}

				int countProtectionsOfThisKind = protectionElements.Count(element => (element.Protection & protectionKind) == protectionKind);
				
				// Each protection binding element is backed by the presence of its dependent protection(s).
				if (countProtectionsOfThisKind > 0 && !wasLastProtectionPresent) {
					throw new ProtocolException(
						string.Format(
							CultureInfo.CurrentCulture,
							MessagingStrings.RequiredProtectionMissing,
							protectionKind));
				}

				// At most one binding element for each protection type.
				if (countProtectionsOfThisKind > 1) {
					throw new ProtocolException(
						string.Format(
							CultureInfo.CurrentCulture,
							MessagingStrings.TooManyBindingsOfferingSameProtection,
							protectionKind,
							countProtectionsOfThisKind));
				}
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
		private static int BindingElementOutgoingMessageApplicationOrder(MessageProtection protection1, MessageProtection protection2) {
			Debug.Assert(protection1 != MessageProtection.None || protection2 != MessageProtection.None, "This comparison function should only be used to compare protection binding elements.  Otherwise we change the order of user-defined message transformations.");

			// Now put the protection ones in the right order.
			return -((int)protection1).CompareTo((int)protection2); // descending flag ordinal order
		}

		/// <summary>
		/// Prepares a message for transmit by applying signatures, nonces, etc.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		private void PrepareMessageForSending(IProtocolMessage message) {
			Debug.Assert(message != null, "message == null");

			MessageProtection appliedProtection = MessageProtection.None;
			foreach (IChannelBindingElement bindingElement in this.bindingElements) {
				if (bindingElement.PrepareMessageForSending(message)) {
					appliedProtection |= bindingElement.Protection;
				}
			}

			// Ensure that the message's protection requirements have been satisfied.
			if ((message.RequiredProtection & appliedProtection) != message.RequiredProtection) {
				throw new UnprotectedMessageException(message, appliedProtection);
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
		private void VerifyMessageAfterReceiving(IProtocolMessage message) {
			Debug.Assert(message != null, "message == null");

			MessageProtection appliedProtection = MessageProtection.None;
			foreach (IChannelBindingElement bindingElement in this.bindingElements.Reverse<IChannelBindingElement>()) {
				if (bindingElement.PrepareMessageForReceiving(message)) {
					appliedProtection |= bindingElement.Protection;
				}
			}

			// Ensure that the message's protection requirements have been satisfied.
			if ((message.RequiredProtection & appliedProtection) != message.RequiredProtection) {
				throw new UnprotectedMessageException(message, appliedProtection);
			}
		}
	}
}
