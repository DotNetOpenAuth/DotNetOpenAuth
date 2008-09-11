//-----------------------------------------------------------------------
// <copyright file="StandardWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.IO;
	using System.Net;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// The default handler for transmitting <see cref="HttpWebRequest"/> instances
	/// and returning the responses.
	/// </summary>
	internal class StandardWebRequestHandler : IWebRequestHandler {
		#region IWebRequestHandler Members

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>The stream the caller should write out the entity data to.</returns>
		public TextWriter GetRequestStream(HttpWebRequest request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			try {
				return new StreamWriter(request.GetRequestStream());
			} catch (WebException ex) {
				throw new ProtocolException(MessagingStrings.ErrorInRequestReplyMessage, ex);
			}
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="Response"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>An instance of <see cref="Response"/> describing the response.</returns>
		public Response GetResponse(HttpWebRequest request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			try {
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
					return new Response(response);
				}
			} catch (WebException ex) {
				throw new ProtocolException(MessagingStrings.ErrorInRequestReplyMessage, ex);
			}
		}

		#endregion
	}
}
