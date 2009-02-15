//-----------------------------------------------------------------------
// <copyright file="StandardWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Sockets;
	using System.Reflection;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The default handler for transmitting <see cref="HttpWebRequest"/> instances
	/// and returning the responses.
	/// </summary>
	internal class StandardWebRequestHandler : IDirectWebRequestHandler {
		/// <summary>
		/// The set of options this web request handler supports.
		/// </summary>
		private const DirectWebRequestOptions SupportedOptions = DirectWebRequestOptions.AcceptAllHttpResponses;

		/// <summary>
		/// The value to use for the User-Agent HTTP header.
		/// </summary>
		private static string userAgentValue = Assembly.GetExecutingAssembly().GetName().Name + "/" + Assembly.GetExecutingAssembly().GetName().Version;

		#region IWebRequestHandler Members

		/// <summary>
		/// Determines whether this instance can support the specified options.
		/// </summary>
		/// <param name="options">The set of options that might be given in a subsequent web request.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can support the specified options; otherwise, <c>false</c>.
		/// </returns>
		public bool CanSupport(DirectWebRequestOptions options) {
			return (options & ~SupportedOptions) == 0;
		}

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>The caller should have set the <see cref="HttpWebRequest.ContentLength"/>
		/// and any other appropriate properties <i>before</i> calling this method.</para>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.</para>
		/// </remarks>
		public Stream GetRequestStream(HttpWebRequest request) {
			return this.GetRequestStream(request, DirectWebRequestOptions.None);
		}

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>The caller should have set the <see cref="HttpWebRequest.ContentLength"/>
		/// and any other appropriate properties <i>before</i> calling this method.</para>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.</para>
		/// </remarks>
		public Stream GetRequestStream(HttpWebRequest request, DirectWebRequestOptions options) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifySupported(this.CanSupport(options), MessagingStrings.DirectWebRequestOptionsNotSupported, options, this.GetType().Name);

			return GetRequestStreamCore(request, options);
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>
		/// An instance of <see cref="DirectWebResponse"/> describing the response.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, shoud be Closed before throwing.</para>
		/// </remarks>
		public DirectWebResponse GetResponse(HttpWebRequest request) {
			return this.GetResponse(request, DirectWebRequestOptions.None);
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>
		/// An instance of <see cref="DirectWebResponse"/> describing the response.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, shoud be Closed before throwing.</para>
		/// </remarks>
		public DirectWebResponse GetResponse(HttpWebRequest request, DirectWebRequestOptions options) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifySupported(this.CanSupport(options), MessagingStrings.DirectWebRequestOptionsNotSupported, options, this.GetType().Name);

			// This request MAY have already been prepared by GetRequestStream, but
			// we have no guarantee, so do it just to be safe.
			PrepareRequest(request, false);

			try {
				Logger.DebugFormat("HTTP {0} {1}", request.Method, request.RequestUri);
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				return new NetworkDirectWebResponse(request.RequestUri, response);
			} catch (WebException ex) {
				HttpWebResponse response = (HttpWebResponse)ex.Response;
				if (response != null && response.StatusCode == HttpStatusCode.ExpectationFailed &&
					request.ServicePoint.Expect100Continue) {
					// Some OpenID servers doesn't understand the Expect header and send 417 error back.
					// If this server just failed from that, alter the ServicePoint for this server
					// so that we don't send that header again next time (whenever that is).
					// "Expect: 100-Continue" HTTP header. (see Google Code Issue 72)
					// We don't want to blindly set all ServicePoints to not use the Expect header
					// as that would be a security hole allowing any visitor to a web site change
					// the web site's global behavior when calling that host.
					request.ServicePoint.Expect100Continue = false; // TODO: investigate that CAS may throw here

					// An alternative to ServicePoint if we don't have permission to set that,
					// but we'd have to set it BEFORE each request.
					////request.Expect = "";  
				}

				if ((options & DirectWebRequestOptions.AcceptAllHttpResponses) != 0 && response != null &&
					response.StatusCode != HttpStatusCode.ExpectationFailed) {
					Logger.InfoFormat("The HTTP error code {0} {1} is being accepted because the {2} flag is set.", (int)response.StatusCode, response.StatusCode, DirectWebRequestOptions.AcceptAllHttpResponses);
					return new NetworkDirectWebResponse(request.RequestUri, response);
				}

				if (Logger.IsErrorEnabled) {
					if (response != null) {
						using (var reader = new StreamReader(ex.Response.GetResponseStream())) {
							Logger.ErrorFormat("WebException from {0}: {1}{2}", ex.Response.ResponseUri, Environment.NewLine, reader.ReadToEnd());
						}
					} else {
						Logger.ErrorFormat("WebException {1} from {0}, no response available.", request.RequestUri, ex.Status);
					}
				}

				// Be sure to close the response stream to conserve resources and avoid
				// filling up all our incoming pipes and denying future requests.
				// If in the future, some callers actually want to read this response
				// we'll need to figure out how to reliably call Close on exception
				// responses at all callers.
				response.Close();

				throw ErrorUtilities.Wrap(ex, MessagingStrings.ErrorInRequestReplyMessage);
			}
		}

		#endregion

		/// <summary>
		/// Initiates a POST request and prepares for sending data.
		/// </summary>
		/// <param name="request">The HTTP request with information about the remote party to contact.</param>
		/// <param name="options">The options to apply to this specific web request.</param>
		/// <returns>
		/// The stream where the POST entity can be written.
		/// </returns>
		private static Stream GetRequestStreamCore(HttpWebRequest request, DirectWebRequestOptions options) {
			PrepareRequest(request, true);

			try {
				return request.GetRequestStream();
			} catch (SocketException ex) {
				throw ErrorUtilities.Wrap(ex, MessagingStrings.WebRequestFailed, request.RequestUri);
			} catch (WebException ex) {
				throw ErrorUtilities.Wrap(ex, MessagingStrings.WebRequestFailed, request.RequestUri);
			}
		}

		/// <summary>
		/// Prepares an HTTP request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="preparingPost"><c>true</c> if this is a POST request whose headers have not yet been sent out; <c>false</c> otherwise.</param>
		private static void PrepareRequest(HttpWebRequest request, bool preparingPost) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			// Be careful to not try to change the HTTP headers that have already gone out.
			if (preparingPost || request.Method == "GET") {
				// Some sites, such as Technorati, return 403 Forbidden on identity
				// pages unless a User-Agent header is included.
				if (string.IsNullOrEmpty(request.UserAgent)) {
					request.UserAgent = userAgentValue;
				}
			}
		}
	}
}
