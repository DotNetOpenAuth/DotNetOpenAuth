using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;

namespace DotNetOpenId.Provider
{
	public enum RequestType {
		CheckIdRequest,
		CheckAuthRequest,
		AssociateRequest,
	}

	public abstract class Request {
		protected Request(Provider server) {
			Server = server;
		}

		protected Provider Server { get; private set; }
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
		internal static Request CreateRequest(Provider provider, NameValueCollection query) {
			Debug.Assert(query != null);
			
			string mode = query[QueryStringArgs.openid.mode];
			if (string.IsNullOrEmpty(mode)) {
				throw new ProtocolException(query, "No openid.mode value in query");
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
					throw new ProtocolException(query, "No decoder for openid.mode=" + mode);
			}

			return request;
		}

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return String.Format(returnString, Mode);
		}
	}
}
