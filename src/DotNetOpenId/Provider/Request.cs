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
	abstract class Request : IRequest {
		protected Request(OpenIdProvider server) {
			Server = server;
			Query = server.query;
			ExtraArgs = new Dictionary<string, string>();
		}

		protected NameValueCollection Query { get; private set; }
		protected OpenIdProvider Server { get; private set; }
		internal abstract string Mode { get; }
		/// <summary>
		/// Additional (extension) arguments to be sent back to the consumer.
		/// </summary>
		protected IDictionary<string, string> ExtraArgs { get; private set; }

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
			try {
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
			} catch (OpenIdException ex) {
				request = new FaultyRequest(provider, ex);
			}
			return request;
		}

		/// <summary>
		/// Indicates whether this request has all the information necessary to formulate a response.
		/// </summary>
		public abstract bool IsResponseReady { get; }
		internal abstract IEncodable CreateResponse();
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
					var encodableResponse = CreateResponse();
					EncodableResponse extendableResponse = encodableResponse as EncodableResponse;
					if (extendableResponse != null)
						extendableResponse.AddFields(null, ExtraArgs, true);
					response = Server.EncodeResponse(encodableResponse);
				}
				return response;
			}
		}
		IResponse IRequest.Response { get { return this.Response; } }

		/// <summary>
		/// Adds extra query parameters to the response directed at the OpenID consumer.
		/// </summary>
		/// <param name="extensionPrefix">
		/// The extension-specific prefix associated with these arguments.
		/// This should not include the 'openid.' part of the prefix.
		/// For example, the extension field openid.sreg.fullname would receive
		/// 'sreg' for this value.
		/// </param>
		/// <param name="arguments">
		/// The key/value pairs of parameters and values to pass to the provider.
		/// The keys should NOT have the 'openid.ext.' prefix.
		/// </param>
		public void AddExtensionArguments(string extensionPrefix, IDictionary<string, string> arguments) {
			if (string.IsNullOrEmpty(extensionPrefix)) throw new ArgumentNullException("extensionPrefix");
			if (arguments == null) throw new ArgumentNullException("arguments");
			if (extensionPrefix.StartsWith(".", StringComparison.Ordinal) ||
				extensionPrefix.EndsWith(".", StringComparison.Ordinal))
				throw new ArgumentException(Strings.PrefixWithoutPeriodsExpected, "extensionPrefix");

			foreach (var pair in arguments) {
				if (pair.Key.StartsWith(QueryStringArgs.openid.Prefix) ||
					pair.Key.StartsWith(extensionPrefix))
					throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ExtensionParameterKeysWithoutPrefixExpected, pair.Key), "arguments");
				// Leave off the prefix as it will be added after signing later.
				ExtraArgs.Add(extensionPrefix + "." + pair.Key, pair.Value);
			}
			InvalidateResponse();
		}

		/// <summary>
		/// Gets the key/value pairs of a consumer's request for a given OpenID extension.
		/// </summary>
		/// <param name="extensionPrefix">
		/// The prefix used by the extension, not including the 'openid.' start.
		/// For example, simple registration key/values can be retrieved by passing 
		/// 'sreg' as this argument.
		/// </param>
		/// <returns>
		/// Returns key/value pairs where the keys do not include the 
		/// 'openid.' or the <paramref name="extensionPrefix"/>.
		/// </returns>
		public IDictionary<string, string> GetExtensionArguments(string extensionPrefix) {
			var response = new Dictionary<string, string>();
			extensionPrefix = QueryStringArgs.openid.Prefix + extensionPrefix + ".";
			int prefix_len = extensionPrefix.Length;
			foreach (string key in Query) {
				if (key.StartsWith(extensionPrefix, StringComparison.OrdinalIgnoreCase)) {
					string bareKey = key.Substring(prefix_len);
					response[bareKey] = Query[key];
				}
			}

			return response;
		}

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return String.Format(CultureInfo.CurrentUICulture, returnString, Mode);
		}
	}
}
