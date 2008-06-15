using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Diagnostics;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Indicates the mode the Provider should use while authenticating the end user.
	/// </summary>
	public enum AuthenticationRequestMode {
		/// <summary>
		/// The Provider should use whatever credentials are immediately available
		/// to determine whether the end user owns the Identifier.  If sufficient
		/// credentials (i.e. cookies) are not immediately available, the Provider
		/// should fail rather than prompt the user.
		/// </summary>
		Immediate,
		/// <summary>
		/// The Provider should determine whether the end user owns the Identifier,
		/// displaying a web page to the user to login etc., if necessary.
		/// </summary>
		Setup
	}

	[DebuggerDisplay("ClaimedIdentifier: {ClaimedIdentifier}, Mode: {Mode}, OpenId: {protocol.Version}")]
	class AuthenticationRequest : IAuthenticationRequest {
		Association assoc;
		ServiceEndpoint endpoint;
		MessageEncoder encoder;
		Protocol protocol { get { return endpoint.Protocol; } }

		AuthenticationRequest(string token, Association assoc, ServiceEndpoint endpoint,
			Realm realm, Uri returnToUrl, MessageEncoder encoder) {
			if (endpoint == null) throw new ArgumentNullException("endpoint");
			if (realm == null) throw new ArgumentNullException("realm");
			if (returnToUrl == null) throw new ArgumentNullException("returnToUrl");
			if (encoder == null) throw new ArgumentNullException("encoder");
			this.assoc = assoc;
			this.endpoint = endpoint;
			this.encoder = encoder;
			Realm = realm;
			ReturnToUrl = returnToUrl;

			Mode = AuthenticationRequestMode.Setup;
			OutgoingExtensions = ExtensionArgumentsManager.CreateOutgoingExtensions(endpoint.Protocol);
			ReturnToArgs = new Dictionary<string, string>();
			if (token != null)
				AddCallbackArguments(DotNetOpenId.RelyingParty.Token.TokenKey, token);
		}
		internal static AuthenticationRequest Create(Identifier userSuppliedIdentifier,
			Realm realm, Uri returnToUrl, IRelyingPartyApplicationStore store, MessageEncoder encoder) {
			if (userSuppliedIdentifier == null) throw new ArgumentNullException("userSuppliedIdentifier");
			if (realm == null) throw new ArgumentNullException("realm");

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Creating authentication request for user supplied Identifier: {0}",
					userSuppliedIdentifier);
			}
			if (TraceUtil.Switch.TraceVerbose) {
				Trace.Indent();
				Trace.TraceInformation("Realm: {0}", realm);
				Trace.TraceInformation("Return To: {0}", returnToUrl);
				Trace.Unindent();
			}
			if (TraceUtil.Switch.TraceWarning && returnToUrl.Query != null) {
				NameValueCollection returnToArgs = HttpUtility.ParseQueryString(returnToUrl.Query);
				foreach (string key in returnToArgs) {
					if (OpenIdRelyingParty.ShouldParameterBeStrippedFromReturnToUrl(key)) {
						Trace.TraceWarning("OpenId argument \"{0}\" found in return_to URL.  This can corrupt an OpenID response.", key);
						break;
					}
				}
			}

			var endpoint = userSuppliedIdentifier.Discover();
			if (endpoint == null)
				throw new OpenIdException(Strings.OpenIdEndpointNotFound);
			if (TraceUtil.Switch.TraceVerbose) {
				Trace.Indent();
				Trace.TraceInformation("Discovered provider endpoint: {0}", endpoint);
				Trace.Unindent();
			}

			// Throw an exception now if the realm and the return_to URLs don't match
			// as required by the provider.  We could wait for the provider to test this and
			// fail, but this will be faster and give us a better error message.
			if (!realm.Contains(returnToUrl))
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ReturnToNotUnderRealm, returnToUrl, realm));

			return new AuthenticationRequest(
				new Token(endpoint).Serialize(store),
				store != null ? getAssociation(endpoint, store) : null,
				endpoint, realm, returnToUrl, encoder);
		}
		static Association getAssociation(ServiceEndpoint provider, IRelyingPartyApplicationStore store) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (store == null) throw new ArgumentNullException("store");
			Association assoc = store.GetAssociation(provider.ProviderEndpoint);

			if (assoc == null || !assoc.HasUsefulLifeRemaining) {
				var req = AssociateRequest.Create(provider);
				if (req.Response != null) {
					// try again if we failed the first time and have a worthy second-try.
					if (req.Response.Association == null && req.Response.SecondAttempt != null) {
						if (TraceUtil.Switch.TraceWarning) {
							Trace.TraceWarning("Initial association attempt failed, but will retry with Provider-suggested parameters.");
						}
						req = req.Response.SecondAttempt;
					}
					assoc = req.Response.Association;
					if (assoc != null) {
						if (TraceUtil.Switch.TraceInfo)
							Trace.TraceInformation("Association with {0} established.", provider.ProviderEndpoint);
						store.StoreAssociation(provider.ProviderEndpoint, assoc);
					} else {
						if (TraceUtil.Switch.TraceError) {
							Trace.TraceError("Association attempt with {0} provider failed.", provider);
						}
					}
				}
			}

			return assoc;
		}

		/// <summary>
		/// Extension arguments to pass to the Provider.
		/// </summary>
		protected ExtensionArgumentsManager OutgoingExtensions { get; private set; }
		/// <summary>
		/// Arguments to add to the return_to part of the query string, so that
		/// these values come back to the consumer when the user agent returns.
		/// </summary>
		protected IDictionary<string, string> ReturnToArgs { get; private set; }

		public AuthenticationRequestMode Mode { get; set; }
		public Realm Realm { get; private set; }
		public Uri ReturnToUrl { get; private set; }
		public Identifier ClaimedIdentifier {
			get { return IsDirectedIdentity ? null : endpoint.ClaimedIdentifier; }
		}
		public bool IsDirectedIdentity {
			get { return endpoint.ClaimedIdentifier == endpoint.Protocol.ClaimedIdentifierForOPIdentifier; }
		}
		/// <summary>
		/// The detected version of OpenID implemented by the Provider.
		/// </summary>
		public Version ProviderVersion { get { return protocol.Version; } }
		/// <summary>
		/// Gets the URL the user agent should be redirected to to begin the 
		/// OpenID authentication process.
		/// </summary>
		public IResponse RedirectingResponse {
			get {
				UriBuilder returnToBuilder = new UriBuilder(ReturnToUrl);
				UriUtil.AppendQueryArgs(returnToBuilder, this.ReturnToArgs);

				var qsArgs = new Dictionary<string, string>();

				qsArgs.Add(protocol.openid.mode, (Mode == AuthenticationRequestMode.Immediate) ?
					protocol.Args.Mode.checkid_immediate : protocol.Args.Mode.checkid_setup);
				qsArgs.Add(protocol.openid.identity, endpoint.ProviderLocalIdentifier);
				if (endpoint.Protocol.QueryDeclaredNamespaceVersion != null)
					qsArgs.Add(protocol.openid.ns, endpoint.Protocol.QueryDeclaredNamespaceVersion);
				if (endpoint.Protocol.Version.Major >= 2) {
					qsArgs.Add(protocol.openid.claimed_id, endpoint.ClaimedIdentifier);
				}
				qsArgs.Add(protocol.openid.Realm, Realm);
				qsArgs.Add(protocol.openid.return_to, returnToBuilder.Uri.AbsoluteUri);

				if (this.assoc != null)
					qsArgs.Add(protocol.openid.assoc_handle, this.assoc.Handle);

				// Add on extension arguments
				foreach(var pair in OutgoingExtensions.GetArgumentsToSend(true))
					qsArgs.Add(pair.Key, pair.Value);

				var request = new IndirectMessageRequest(this.endpoint.ProviderEndpoint, qsArgs);
				return this.encoder.Encode(request);
			}
		}

		public bool IsExtensionAdvertisedAsSupported<T>() where T : Extensions.IExtension, new() {
			T extension = new T();
			return endpoint.IsExtensionSupported(extension);
		}
		public bool IsExtensionAdvertisedAsSupported(Type extensionType) {
			if (extensionType == null) throw new ArgumentNullException("extensionType");
			if (!typeof(Extensions.IExtension).IsAssignableFrom(extensionType))
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
					Strings.TypeMustImplementX, typeof(Extensions.IExtension).FullName),
					"extensionType");
			var extension = (Extensions.IExtension)Activator.CreateInstance(extensionType);
			return endpoint.IsExtensionSupported(extension);
		}

		public void AddExtension(DotNetOpenId.Extensions.IExtensionRequest extension) {
			if (extension == null) throw new ArgumentNullException("extension");
			OutgoingExtensions.AddExtensionArguments(extension.TypeUri, extension.Serialize(this));
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
			if (ReturnToArgs.ContainsKey(key)) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
				Strings.KeyAlreadyExists, key));
			ReturnToArgs.Add(key, value ?? "");
		}

		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// Execution of the current page terminates after this call.
		/// </summary>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public void RedirectToProvider() {
			if (HttpContext.Current == null || HttpContext.Current.Response == null) 
				throw new InvalidOperationException(Strings.CurrentHttpContextRequired);
			RedirectingResponse.Send();
		}
	}
}
