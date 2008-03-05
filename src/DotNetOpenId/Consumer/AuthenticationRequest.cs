using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

namespace DotNetOpenId.Consumer {
	public enum AuthenticationRequestMode {
		Immediate,
		Setup
	}

	class AuthenticationRequest : IAuthenticationRequest {
		Association assoc;
		ServiceEndpoint endpoint;

		internal AuthenticationRequest(string token, Association assoc, ServiceEndpoint endpoint,
			TrustRoot trustRootUrl, Uri returnToUrl) {
			this.token = token;
			this.assoc = assoc;
			this.endpoint = endpoint;
			TrustRootUrl = trustRootUrl;
			ReturnToUrl = returnToUrl;

			Mode = AuthenticationRequestMode.Setup;
			ExtraArgs = new Dictionary<string, string>();
			ReturnToArgs = new Dictionary<string, string>();
			AddCallbackArguments(DotNetOpenId.Consumer.Token.TokenKey, token);
		}

		string token { get; set; }
		/// <summary>
		/// Arguments to add to the query string to be sent to the provider.
		/// </summary>
		protected IDictionary<string, string> ExtraArgs { get; private set; }
		/// <summary>
		/// Arguments to add to the return_to part of the query string, so that
		/// these values come back to the consumer when the user agent returns.
		/// </summary>
		protected IDictionary<string, string> ReturnToArgs { get; private set; }

		public AuthenticationRequestMode Mode { get; set; }
		public TrustRoot TrustRootUrl { get; private set; }
		public Uri ReturnToUrl { get; private set; }
		/// <summary>
		/// Gets the URL the user agent should be redirected to to begin the 
		/// OpenID authentication process.
		/// </summary>
		public Uri RedirectToProviderUrl {
			get {
				UriBuilder returnToBuilder = new UriBuilder(ReturnToUrl);
				UriUtil.AppendQueryArgs(returnToBuilder, this.ReturnToArgs);

				var qsArgs = new Dictionary<string, string>();

				qsArgs.Add(QueryStringArgs.openid.mode, (Mode == AuthenticationRequestMode.Immediate) ?
					QueryStringArgs.Modes.checkid_immediate : QueryStringArgs.Modes.checkid_setup);
				qsArgs.Add(QueryStringArgs.openid.identity, this.endpoint.ServerId.AbsoluteUri); //TODO: breaks the Law of Demeter
				qsArgs.Add(QueryStringArgs.openid.return_to, returnToBuilder.ToString());
				qsArgs.Add(QueryStringArgs.openid.trust_root, TrustRootUrl.ToString());

				if (this.assoc != null)
					qsArgs.Add(QueryStringArgs.openid.assoc_handle, this.assoc.Handle); // !!!!

				UriBuilder redir = new UriBuilder(this.endpoint.ServerUrl);

				UriUtil.AppendQueryArgs(redir, qsArgs);
				UriUtil.AppendQueryArgs(redir, ExtraArgs);

				return redir.Uri;
			}
		}

		/// <summary>
		/// Adds extra query parameters to the request directed at the OpenID provider.
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
				ExtraArgs.Add(QueryStringArgs.openid.Prefix + extensionPrefix + "." + pair.Key, pair.Value);
			}
		}
		/// <summary>
		/// Adds given key/value pairs to the query that the provider will use in
		/// the request to return to the consumer web site.
		/// </summary>
		public void AddCallbackArguments(IDictionary<string, string> arguments) {
			if (arguments == null) throw new ArgumentNullException("arguments");
			foreach (var pair in arguments) {
				AddCallbackArguments(pair.Key, pair.Value);
			}
		}
		/// <summary>
		/// Adds a given key/value pair to the query that the provider will use in
		/// the request to return to the consumer web site.
		/// </summary>
		public void AddCallbackArguments(string key, string value) {
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
			if (ReturnToArgs.ContainsKey(key)) throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
				Strings.KeyAlreadyExists, key));
			ReturnToArgs.Add(key, value ?? "");
		}

		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// </summary>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public void RedirectToProvider() {
			RedirectToProvider(false);
		}
		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// </summary>
		/// <param name="endResponse">
		/// Whether execution of this response should cease after this call.
		/// </param>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public void RedirectToProvider(bool endResponse) {
			if (HttpContext.Current == null || HttpContext.Current.Response == null) 
				throw new InvalidOperationException(Strings.CurrentHttpContextRequired);
			HttpContext.Current.Response.Redirect(RedirectToProviderUrl.AbsoluteUri, endResponse);
		}
	}
}
