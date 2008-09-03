//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.IO;
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
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.
		/// </param>
		protected Channel(IMessageTypeProvider messageTypeProvider) {
			if (messageTypeProvider == null) {
				throw new ArgumentNullException("messageTypeProvider");
			}

			this.messageTypeProvider = messageTypeProvider;
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
			this.Send(message, null);
		}

		/// <summary>
		/// Queues an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		/// <param name="inResponseTo">
		/// If <paramref name="message"/> is a response to an incoming message, this is the incoming message.
		/// This is useful for error scenarios in deciding just how to send the response message.
		/// May be null.
		/// </param>
		internal void Send(IProtocolMessage message, IProtocolMessage inResponseTo) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			var directedMessage = message as IDirectedProtocolMessage;
			if (directedMessage == null) {
				// This is a response to a direct message.
				this.SendDirectMessageResponse(message);
			} else {
				if (directedMessage.Recipient != null) {
					// This is an indirect message request or reply.
					this.SendIndirectMessage(directedMessage);
				} else {
					ProtocolException exception = message as ProtocolException;
					if (exception != null) {
						if (inResponseTo is IDirectedProtocolMessage) {
							this.ReportErrorAsDirectResponse(exception);
						} else {
							this.ReportErrorToUser(exception);
						}
					} else {
						throw new InvalidOperationException();
					}
				}
			}
		}

		/// <summary>
		/// Gets the protocol message embedded in the given HTTP request, if present.
		/// </summary>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		/// <remarks>
		/// Requires an HttpContext.Current context.
		/// </remarks>
		internal IProtocolMessage ReadFromRequest() {
			return this.ReadFromRequest(new HttpRequestInfo(HttpContext.Current.Request));
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected internal virtual IProtocolMessage ReadFromRequest(HttpRequestInfo request) {
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
		/// Gets the protocol message that may be in the given HTTP response stream.
		/// </summary>
		/// <param name="responseStream">The response that is anticipated to contain an OAuth message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected internal abstract IProtocolMessage ReadFromResponse(Stream responseStream);

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		protected internal abstract IProtocolMessage Request(IDirectedProtocolMessage request);

		/// <summary>
		/// Deserializes a dictionary of values into a message.
		/// </summary>
		/// <param name="fields">The dictionary of values that were read from an HTTP request or response.</param>
		/// <returns>The deserialized message.</returns>
		protected virtual IProtocolMessage Receive(Dictionary<string, string> fields) {
			Type messageType = null;
			if (fields != null) {
				messageType = this.MessageTypeProvider.GetRequestMessageType(fields);
			}

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
				Body = new byte[0],
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
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			WebHeaderCollection headers = new WebHeaderCollection();
			MemoryStream body = new MemoryStream();
			StreamWriter bodyWriter = new StreamWriter(body);
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
				Status = HttpStatusCode.Redirect,
				Headers = headers,
				Body = body.ToArray(),
				OriginalMessage = message
			};

			return response;
		}

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
		/// Reports an error to the user via the user agent.
		/// </summary>
		/// <param name="exception">The error information.</param>
		protected abstract void ReportErrorToUser(ProtocolException exception);

		/// <summary>
		/// Sends an error result directly to the calling remote party according to the
		/// rules of the protocol.
		/// </summary>
		/// <param name="exception">The error information.</param>
		protected abstract void ReportErrorAsDirectResponse(ProtocolException exception);

		/// <summary>
		/// Calculates a fairly accurate estimation on the size of a message that contains
		/// a given set of fields.
		/// </summary>
		/// <param name="fields">The fields that would be included in a message.</param>
		/// <returns>The size (in bytes) of the message payload.</returns>
		private static int CalculateSizeOfPayload(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			int size = 0;
			foreach (var field in fields) {
				size += field.Key.Length;
				size += field.Value.Length;
				size += 2; // & and =
			}
			return size;
		}
	}
}
