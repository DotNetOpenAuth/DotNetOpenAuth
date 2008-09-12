using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Web;

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
		Protocol protocol { get { return endpoint.Protocol; } }
		internal OpenIdRelyingParty RelyingParty;

		AuthenticationRequest(string token, Association assoc, ServiceEndpoint endpoint,
			Realm realm, Uri returnToUrl, OpenIdRelyingParty relyingParty) {
			if (endpoint == null) throw new ArgumentNullException("endpoint");
			if (realm == null) throw new ArgumentNullException("realm");
			if (returnToUrl == null) throw new ArgumentNullException("returnToUrl");
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");

			this.assoc = assoc;
			this.endpoint = endpoint;
			RelyingParty = relyingParty;
			Realm = realm;
			ReturnToUrl = returnToUrl;

			Mode = AuthenticationRequestMode.Setup;
			OutgoingExtensions = ExtensionArgumentsManager.CreateOutgoingExtensions(endpoint.Protocol);
			ReturnToArgs = new Dictionary<string, string>();
			if (token != null)
				AddCallbackArguments(DotNetOpenId.RelyingParty.Token.TokenKey, token);
		}
		internal static AuthenticationRequest Create(Identifier userSuppliedIdentifier,
			OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl) {
			if (userSuppliedIdentifier == null) throw new ArgumentNullException("userSuppliedIdentifier");
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (realm == null) throw new ArgumentNullException("realm");

			userSuppliedIdentifier = userSuppliedIdentifier.TrimFragment();
			if (relyingParty.Settings.RequireSsl) {
				// Rather than check for successful SSL conversion at this stage,
				// We'll wait for secure discovery to fail on the new identifier.
				userSuppliedIdentifier.TryRequireSsl(out userSuppliedIdentifier);
			}
			Logger.InfoFormat("Creating authentication request for user supplied Identifier: {0}",
				userSuppliedIdentifier);
			Logger.DebugFormat("Realm: {0}", realm);
			Logger.DebugFormat("Return To: {0}", returnToUrl);

			if (Logger.IsWarnEnabled && returnToUrl.Query != null) {
				NameValueCollection returnToArgs = HttpUtility.ParseQueryString(returnToUrl.Query);
				foreach (string key in returnToArgs) {
					if (OpenIdRelyingParty.ShouldParameterBeStrippedFromReturnToUrl(key)) {
						Logger.WarnFormat("OpenId argument \"{0}\" found in return_to URL.  This can corrupt an OpenID response.", key);
						break;
					}
				}
			}

			var endpoints = new List<ServiceEndpoint>(userSuppliedIdentifier.Discover());
			ServiceEndpoint endpoint = selectEndpoint(endpoints.AsReadOnly(), relyingParty);
			if (endpoint == null)
				throw new OpenIdException(Strings.OpenIdEndpointNotFound);

			// Throw an exception now if the realm and the return_to URLs don't match
			// as required by the provider.  We could wait for the provider to test this and
			// fail, but this will be faster and give us a better error message.
			if (!realm.Contains(returnToUrl))
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ReturnToNotUnderRealm, returnToUrl, realm));

			string token = new Token(endpoint).Serialize(relyingParty.Store);
			// Retrieve the association, but don't create one, as a creation was already
			// attempted by the selectEndpoint method.
			Association association = relyingParty.Store != null ? getAssociation(relyingParty, endpoint, false) : null;

			return new AuthenticationRequest(
				token, association, endpoint, realm, returnToUrl, relyingParty);
		}

		/// <summary>
		/// Returns a filtered and sorted list of the available OP endpoints for a discovered Identifier.
		/// </summary>
		private static List<ServiceEndpoint> filterAndSortEndpoints(ReadOnlyCollection<ServiceEndpoint> endpoints,
			OpenIdRelyingParty relyingParty) {
			if (endpoints == null) throw new ArgumentNullException("endpoints");
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");

			// Construct the endpoints filters based on criteria given by the host web site.
			EndpointSelector versionFilter = ep => ((ServiceEndpoint)ep).Protocol.Version >= Protocol.Lookup(relyingParty.Settings.MinimumRequiredOpenIdVersion).Version;
			EndpointSelector hostingSiteFilter = relyingParty.EndpointFilter ?? (ep => true);

			var filteredEndpoints = new List<IXrdsProviderEndpoint>(endpoints.Count);
			foreach (ServiceEndpoint endpoint in endpoints) {
				if (versionFilter(endpoint) && hostingSiteFilter(endpoint)) {
					filteredEndpoints.Add(endpoint);
				}
			}

			// Sort endpoints so that the first one in the list is the most preferred one.
			filteredEndpoints.Sort(relyingParty.EndpointOrder);

			List<ServiceEndpoint> endpointList = new List<ServiceEndpoint>(filteredEndpoints.Count);
			foreach (ServiceEndpoint endpoint in filteredEndpoints) {
				endpointList.Add(endpoint);
			}
			return endpointList;
		}

		/// <summary>
		/// Chooses which provider endpoint is the best one to use.
		/// </summary>
		/// <returns>The best endpoint, or null if no acceptable endpoints were found.</returns>
		private static ServiceEndpoint selectEndpoint(ReadOnlyCollection<ServiceEndpoint> endpoints,
			OpenIdRelyingParty relyingParty) {

			List<ServiceEndpoint> filteredEndpoints = filterAndSortEndpoints(endpoints, relyingParty);
			if (filteredEndpoints.Count != endpoints.Count) {
				Logger.DebugFormat("Some endpoints were filtered out.  Total endpoints remaining: {0}", filteredEndpoints.Count);
			}
			if (Logger.IsDebugEnabled) {
				if (Util.AreSequencesEquivalent(endpoints, filteredEndpoints)) {
					Logger.Debug("Filtering and sorting of endpoints did not affect the list.");
				} else {
					Logger.Debug("After filtering and sorting service endpoints, this is the new prioritized list:");
					Logger.Debug(Util.ToString(filteredEndpoints, true));
				}
			}

			// If there are no endpoint candidates...
			if (filteredEndpoints.Count == 0) {
				return null;
			}

			// If we don't have an application store, we have no place to record an association to
			// and therefore can only take our best shot at one of the endpoints.
			if (relyingParty.Store == null) {
				Logger.Debug("No state store, so the first endpoint available is selected.");
				return filteredEndpoints[0];
			}

			// Go through each endpoint until we find one that we can successfully create
			// an association with.  This is our only hint about whether an OP is up and running.
			// The idea here is that we don't want to redirect the user to a dead OP for authentication.
			// If the user has multiple OPs listed in his/her XRDS document, then we'll go down the list
			// and try each one until we find one that's good.
			int winningEndpointIndex = 0;
			foreach (ServiceEndpoint endpointCandidate in filteredEndpoints) {
				winningEndpointIndex++;
				// One weakness of this method is that an OP that's down, but with whom we already
				// created an association in the past will still pass this "are you alive?" test.
				Association association = getAssociation(relyingParty, endpointCandidate, true);
				if (association != null) {
					Logger.DebugFormat("Endpoint #{0} (1-based index) responded to an association request.  Selecting that endpoint.", winningEndpointIndex);
					// We have a winner!
					return endpointCandidate;
				}
			}

			// Since all OPs failed to form an association with us, just return the first endpoint
			// and hope for the best.
			Logger.Debug("All endpoints failed to respond to an association request.  Selecting first endpoint to try to authenticate to.");
			return endpoints[0];
		}
		static Association getAssociation(OpenIdRelyingParty relyingParty, ServiceEndpoint provider, bool createNewAssociationIfNeeded) {
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (provider == null) throw new ArgumentNullException("provider");
			// TODO: we need a way to lookup an association that fulfills a given set of security
			// requirements.  We may have a SHA-1 association and a SHA-256 association that need
			// to be called for specifically. (a bizzare scenario, admittedly, making this low priority).
			Association assoc = relyingParty.Store.GetAssociation(provider.ProviderEndpoint);

			// If the returned association does not fulfill security requirements, ignore it.
			if (assoc != null && !relyingParty.Settings.IsAssociationInPermittedRange(provider.Protocol, assoc.GetAssociationType(provider.Protocol))) {
				assoc = null;
			}

			if ((assoc == null || !assoc.HasUsefulLifeRemaining) && createNewAssociationIfNeeded) {
				var req = AssociateRequest.Create(relyingParty, provider);
				if (req == null) {
					// this can happen if security requirements and protocol conflict
					// to where there are no association types to choose from.
					return null;
				}
				if (req.Response != null) {
					// try again if we failed the first time and have a worthy second-try.
					if (req.Response.Association == null && req.Response.SecondAttempt != null) {
						Logger.Warn("Initial association attempt failed, but will retry with Provider-suggested parameters.");
						req = req.Response.SecondAttempt;
					}
					assoc = req.Response.Association;
					// Confirm that the association matches the type we requested (section 8.2.1).
					if (assoc != null) {
						string responseSessionType = provider.Protocol.Version.Major > 2 ?
							Util.GetRequiredArg(req.Response.Args, provider.Protocol.openidnp.session_type) :
							(Util.GetOptionalArg(req.Response.Args, provider.Protocol.openidnp.session_type) ?? string.Empty);
						if (!string.Equals(
							req.Args[provider.Protocol.openid.assoc_type],
							Util.GetRequiredArg(req.Response.Args, provider.Protocol.openidnp.assoc_type),
							StringComparison.Ordinal) ||
							!string.Equals(
							req.Args[provider.Protocol.openid.session_type],
							responseSessionType,
							StringComparison.Ordinal)) {
							Logger.ErrorFormat("Provider responded with contradicting association parameters.  Requested [{0}, {1}] but got [{2}, {3}] back.",
								req.Args[provider.Protocol.openid.assoc_type],
								req.Args[provider.Protocol.openid.session_type],
								Util.GetRequiredArg(req.Response.Args, provider.Protocol.openidnp.assoc_type),
								responseSessionType);

							assoc = null;
						}
					}
					if (assoc != null) {
						Logger.InfoFormat("Association with {0} established.", provider.ProviderEndpoint);
						relyingParty.Store.StoreAssociation(provider.ProviderEndpoint, assoc);
					} else {
						Logger.ErrorFormat("Association attempt with {0} provider failed.", provider.ProviderEndpoint);
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
		/// Gets information about the OpenId Provider, as advertised by the
		/// OpenId discovery documents found at the <see cref="ClaimedIdentifier"/>
		/// location.
		/// </summary>
		IProviderEndpoint IAuthenticationRequest.Provider { get { return endpoint; } }

		/// <summary>
		/// Gets the response to send to the user agent to begin the
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
				foreach (var pair in OutgoingExtensions.GetArgumentsToSend(true))
					qsArgs.Add(pair.Key, pair.Value);

				var request = new IndirectMessageRequest(this.endpoint.ProviderEndpoint, qsArgs);
				return RelyingParty.Encoder.Encode(request);
			}
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
