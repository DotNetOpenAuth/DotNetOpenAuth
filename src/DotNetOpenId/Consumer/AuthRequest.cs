using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;
using System.Collections.Specialized;
using System.Globalization;

namespace DotNetOpenId.Consumer {
	public class AuthRequest {
		public enum Mode {
			Immediate,
			Setup
		}

		Association assoc;
		ServiceEndpoint endpoint;

		internal AuthRequest(string token, Association assoc, ServiceEndpoint endpoint) {
			this.token = token;
			this.assoc = assoc;
			this.endpoint = endpoint;

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
		public Uri CreateRedirect(string trustRoot, Uri returnTo, Mode mode) {
			UriBuilder returnToBuilder = new UriBuilder(returnTo);
			UriUtil.AppendQueryArgs(returnToBuilder, this.ReturnToArgs);

			var qsArgs = new Dictionary<string, string>();

			qsArgs.Add(QueryStringArgs.openid.mode, (mode == Mode.Immediate) ?
				QueryStringArgs.Modes.checkid_immediate : QueryStringArgs.Modes.checkid_setup);
			qsArgs.Add(QueryStringArgs.openid.identity, this.endpoint.ServerId.AbsoluteUri); //TODO: breaks the Law of Demeter
			qsArgs.Add(QueryStringArgs.openid.return_to, returnToBuilder.ToString());
			qsArgs.Add(QueryStringArgs.openid.trust_root, trustRoot);

			if (this.assoc != null)
				qsArgs.Add(QueryStringArgs.openid.assoc_handle, this.assoc.Handle); // !!!!

			UriBuilder redir = new UriBuilder(this.endpoint.ServerUrl);

			UriUtil.AppendQueryArgs(redir, qsArgs);
			UriUtil.AppendQueryArgs(redir, ExtraArgs);

			return redir.Uri;
		}

	}
}
