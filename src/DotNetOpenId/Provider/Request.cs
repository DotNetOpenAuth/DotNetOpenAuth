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
	/// Represents any OpenId-protocol request that may come to the provider.
	/// </summary>
	public abstract class Request {
		protected Request(OpenIdProvider server) {
			Server = server;
		}

		protected OpenIdProvider Server { get; private set; }
		internal abstract string Mode { get; }

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

		/// <summary>
		/// Indicates whether this request has all the information necessary to formulate a response.
		/// </summary>
		public abstract bool IsResponseReady { get; }
		protected abstract Response CreateResponse();
		/// <summary>
		/// Called whenever a property changes that would cause the response to need to be
		/// regenerated if it had already been generated.
		/// </summary>
		protected void InvalidateResponse() {
			response = null;
		}
		Response response;
		/// <summary>
		/// The authentication response to be sent to the user agent or the calling
		/// OpenId consumer.
		/// </summary>
		public Response Response {
			get {
				if (!IsResponseReady) throw new InvalidOperationException(Strings.ResponseNotReady);
				if (response == null) {
					response = CreateResponse();
				}
				return response;
			}
		}

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return String.Format(CultureInfo.CurrentUICulture, returnString, Mode);
		}
	}
}
