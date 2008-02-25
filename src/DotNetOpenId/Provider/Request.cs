using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Web;

namespace DotNetOpenId.Provider
{
	/// <summary>
	/// Identifies the type of requests that can come to an OpenId provider.
	/// </summary>
	public enum RequestType {
		/// <summary>
		/// An OpenId consumer has directed a user agent to this provider
		/// for authentication.
		/// </summary>
		CheckIdRequest,
		/// <summary>
		/// An OpenId consumer is requesting verification of a user agent's
		/// claim of successful authentication when a prior association
		/// between consumer and provider was not available to do the verification
		/// immediately.
		/// </summary>
		CheckAuthRequest,
		/// <summary>
		/// An OpenId consumer is requesting a shared secret with this provider
		/// so that future authentication requests do not need to be verified
		/// with the provider seperately.
		/// </summary>
		AssociateRequest,
	}

	/// <summary>
	/// Represents any OpenId-protocol request that may come to the provider.
	/// </summary>
	public abstract class Request {
		protected Request(OpenIdProvider server) {
			Server = server;
		}

		protected OpenIdProvider Server { get; private set; }
		internal abstract string Mode { get; }
		public abstract RequestType RequestType { get; }

		/// <summary>
		/// Tests whether a given dictionary represents an incoming OpenId request.
		/// </summary>
		/// <param name="query">The name/value pairs in the querystring or Form submission.  Cannot be null.</param>
		/// <returns>True if the request is an OpenId request, false otherwise.</returns>
		internal static bool IsOpenIdRequest(NameValueCollection query) {
			Debug.Assert(query != null);
			foreach (string key in query) {
				if (key.StartsWith(QueryStringArgs.openid.Prefix, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Creates the appropriate Request-derived type based on the request dictionary.
		/// </summary>
		/// <param name="provider">The Provider instance that called this method.</param>
		/// <param name="query">A dictionary of name/value pairs given in the request's
		/// querystring or form submission.</param>
		/// <returns>A Request-derived type appropriate for this stage in authentication.</returns>
		internal static Request CreateRequest(OpenIdProvider provider, NameValueCollection query) {
			Debug.Assert(query != null);
			
			string mode = query[QueryStringArgs.openid.mode];
			if (string.IsNullOrEmpty(mode)) {
				throw new OpenIdException("No openid.mode value in query.", query);
			}

			Request request;
			switch (mode) {
				case QueryStringArgs.Modes.checkid_setup:
					request = new CheckIdRequest(provider, query);
					break;
				case QueryStringArgs.Modes.checkid_immediate:
					request = new CheckIdRequest(provider, query);
					break;
				case QueryStringArgs.Modes.check_authentication:
					request = new CheckAuthRequest(provider, query);
					break;
				case QueryStringArgs.Modes.associate:
					request = new AssociateRequest(provider, query);
					break;
				default:
					throw new OpenIdException("No decoder for openid.mode=" + mode, query);
			}

			return request;
		}

		protected abstract AuthenticationResponse CreateResponse();
		/// <summary>
		/// Called whenever a property changes that would cause the response to need to be
		/// regenerated if it had already been generated.
		/// </summary>
		protected void InvalidateResponse() {
			response = null;
		}
		AuthenticationResponse response;
		/// <summary>
		/// The authentication response to be sent to the user agent or the calling
		/// OpenId consumer.
		/// </summary>
		public AuthenticationResponse Response {
			get {
				if (response == null) {
					response = CreateResponse();
				}
				return response;
			}
		}

		/// <summary>
		/// Sends the appropriate response to the OpenId request to the user agent or
		/// OpenId consumer.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public void Respond() {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);
			HttpContext.Current.Response.Clear();
			HttpContext.Current.Response.StatusCode = (int)Response.Code;
			foreach (string headerName in Response.Headers)
				HttpContext.Current.Response.AddHeader(headerName, Response.Headers[headerName]);
			HttpContext.Current.Response.OutputStream.Write(Response.Body, 0, Response.Body.Length);
			HttpContext.Current.Response.OutputStream.Flush();
			HttpContext.Current.Response.OutputStream.Close();
		}

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return String.Format(CultureInfo.CurrentUICulture, returnString, Mode);
		}
	}
}
