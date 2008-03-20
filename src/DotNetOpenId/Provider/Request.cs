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
		/// Tracks extension Type URIs and aliases assigned to them.
		/// </summary>
		Dictionary<string, string> extensionTypeUriToAliasMap = new Dictionary<string, string>();

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
						request = new CheckIdRequest(provider);
						break;
					case QueryStringArgs.Modes.checkid_immediate:
						request = new CheckIdRequest(provider);
						break;
					case QueryStringArgs.Modes.check_authentication:
						request = new CheckAuthRequest(provider);
						break;
					case QueryStringArgs.Modes.associate:
						request = new AssociateRequest(provider);
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
		/// Adds query parameters for OpenID extensions to the request directed 
		/// at the OpenID provider.
		/// </summary>
		public void AddExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments) {
			if (string.IsNullOrEmpty(extensionTypeUri)) throw new ArgumentNullException("extensionTypeUri");
			if (arguments == null) throw new ArgumentNullException("arguments");

			string extensionAlias;
			// Affinity to make the simple registration extension use the sreg alias.
			if (extensionTypeUri == QueryStringArgs.sreg_ns) {
				extensionAlias = QueryStringArgs.sreg_compatibility_alias;
				createExtensionAlias(extensionTypeUri, extensionAlias);
			} else {
				extensionAlias = findOrCreateExtensionAlias(extensionTypeUri);
			}
			foreach (var pair in arguments) {
				ExtraArgs.Add(extensionAlias + "." + pair.Key, pair.Value);
			}
		}

		void createExtensionAlias(string extensionTypeUri, string preferredAlias) {
			string existingAlias;
			if (!extensionTypeUriToAliasMap.TryGetValue(extensionTypeUri, out existingAlias)) {
				extensionTypeUriToAliasMap.Add(extensionTypeUri, preferredAlias);
				ExtraArgs.Add(QueryStringArgs.openid.ns + "." + preferredAlias, extensionTypeUri);
			} else {
				if (existingAlias != preferredAlias) {
					throw new InvalidOperationException("Extension " + extensionTypeUri + " already assigned to alias " + existingAlias);
				}
			}
		}

		string findOrCreateExtensionAlias(string extensionTypeUri) {
			string alias;
			lock (extensionTypeUriToAliasMap) {
				if (!extensionTypeUriToAliasMap.TryGetValue(extensionTypeUri, out alias)) {
					alias = "ext" + (extensionTypeUriToAliasMap.Count + 1).ToString();
					createExtensionAlias(extensionTypeUri, alias);
				}
			}
			return alias;
		}

		/// <summary>
		/// Gets the key/value pairs of a provider's response for a given OpenID extension.
		/// </summary>
		/// <param name="extensionTypeUri">
		/// The Type URI of the OpenID extension whose arguments are being sought.
		/// </param>
		/// <returns>
		/// Returns key/value pairs for this extension.
		/// </returns>
		public IDictionary<string, string> GetExtensionArguments(string extensionTypeUri) {
			if (string.IsNullOrEmpty(extensionTypeUri)) throw new ArgumentNullException("extensionTypeUri");
			var response = new Dictionary<string, string>();
			string alias = findAliasForExtension(extensionTypeUri);
			if (alias == null) {
				// for OpenID 1.x compatibility, guess the sreg alias.
				if (extensionTypeUri == QueryStringArgs.sreg_ns &&
					!isExtensionAliasDefined(QueryStringArgs.sreg_compatibility_alias))
					alias = QueryStringArgs.sreg_compatibility_alias;
				else
					return response;
			}

			string extensionPrefix = QueryStringArgs.openid.Prefix + alias + ".";
			foreach (string key in Query) {
				if (key.StartsWith(extensionPrefix, StringComparison.OrdinalIgnoreCase)) {
					string bareKey = key.Substring(extensionPrefix.Length);
					response[bareKey] = Query[key];
				}
			}

			return response;
		}

		bool isExtensionAliasDefined(string alias) {
			string aliasPrefix = QueryStringArgs.openid.ns + "." + alias;
			foreach (string key in Query) {
				if (key.Equals(aliasPrefix, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

		string findAliasForExtension(string extensionTypeUri) {
			string aliasPrefix = QueryStringArgs.openid.ns + ".";
			foreach (string key in Query) {
				if (key.StartsWith(aliasPrefix, StringComparison.OrdinalIgnoreCase) &&
					Query[key].Equals(extensionTypeUri, StringComparison.Ordinal)) {
					return key.Substring(aliasPrefix.Length);
				}
			}
			return null;
		}

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return String.Format(CultureInfo.CurrentUICulture, returnString, Mode);
		}
	}
}
