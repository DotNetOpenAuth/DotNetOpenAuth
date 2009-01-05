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
		internal AssociationPreference associationPreference = AssociationPreference.IfPossible;
		ServiceEndpoint endpoint;
		Protocol protocol { get { return endpoint.Protocol; } }
		internal OpenIdRelyingParty RelyingParty;

		AuthenticationRequest(ServiceEndpoint endpoint,
			Realm realm, Uri returnToUrl, OpenIdRelyingParty relyingParty) {
			if (endpoint == null) throw new ArgumentNullException("endpoint");
			if (realm == null) throw new ArgumentNullException("realm");
			if (returnToUrl == null) throw new ArgumentNullException("returnToUrl");
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");

			this.endpoint = endpoint;
			RelyingParty = relyingParty;
			Realm = realm;
			ReturnToUrl = returnToUrl;

			Mode = AuthenticationRequestMode.Setup;
			OutgoingExtensions = ExtensionArgumentsManager.CreateOutgoingExtensions(endpoint.Protocol);
			ReturnToArgs = new Dictionary<string, string>();
		}

		/// <summary>
		/// Performs identifier discovery and creates associations and generates authentication requests
		/// on-demand for as long as new ones can be generated based on the results of Identifier discovery.
		/// </summary>
		internal static IEnumerable<AuthenticationRequest> Create(Identifier userSuppliedIdentifier,
			OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl, bool createNewAssociationsAsNeeded) {
			// We have a long data validation and preparation process
			if (userSuppliedIdentifier == null) throw new ArgumentNullException("userSuppliedIdentifier");
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (realm == null) throw new ArgumentNullException("realm");

			userSuppliedIdentifier = userSuppliedIdentifier.TrimFragment();
			if (relyingParty.Settings.RequireSsl) {
				// Rather than check for successful SSL conversion at this stage,
				// We'll wait for secure discovery to fail on the new identifier.
				userSuppliedIdentifier.TryRequireSsl(out userSuppliedIdentifier);
			}

			if (Logger.IsWarnEnabled && returnToUrl.Query != null) {
				NameValueCollection returnToArgs = HttpUtility.ParseQueryString(returnToUrl.Query);
				foreach (string key in returnToArgs) {
					if (OpenIdRelyingParty.ShouldParameterBeStrippedFromReturnToUrl(key)) {
						Logger.WarnFormat("OpenId argument \"{0}\" found in return_to URL.  This can corrupt an OpenID response.", key);
						break;
					}
				}
			}

			// Throw an exception now if the realm and the return_to URLs don't match
			// as required by the provider.  We could wait for the provider to test this and
			// fail, but this will be faster and give us a better error message.
			if (!realm.Contains(returnToUrl))
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ReturnToNotUnderRealm, returnToUrl, realm));

			// Perform discovery right now (not deferred).
			var serviceEndpoints = userSuppliedIdentifier.Discover();

			// Call another method that defers request generation.
			return CreateInternal(userSuppliedIdentifier, relyingParty, realm, returnToUrl, serviceEndpoints, createNewAssociationsAsNeeded);
		}

		/// <summary>
		/// Performs request generation for the <see cref="Create"/> method.
		/// All data validation and cleansing steps must have ALREADY taken place.
		/// </summary>
		private static IEnumerable<AuthenticationRequest> CreateInternal(Identifier userSuppliedIdentifier,
			OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl,
			IEnumerable<ServiceEndpoint> serviceEndpoints, bool createNewAssociationsAsNeeded) {
			Logger.InfoFormat("Performing discovery on user-supplied identifier: {0}", userSuppliedIdentifier);
			IEnumerable<ServiceEndpoint> endpoints = filterAndSortEndpoints(serviceEndpoints, relyingParty);

			// Maintain a list of endpoints that we could not form an association with.
			// We'll fallback to generating requests to these if the ones we CAN create
			// an association with run out.
			var failedAssociationEndpoints = new List<ServiceEndpoint>(0);

			foreach (var endpoint in endpoints) {
				Logger.InfoFormat("Creating authentication request for user supplied Identifier: {0}", userSuppliedIdentifier);
				Logger.DebugFormat("Realm: {0}", realm);
				Logger.DebugFormat("Return To: {0}", returnToUrl);

				// The strategy here is to prefer endpoints with whom we can create associations.
				Association association = null;
				if (relyingParty.Store != null) {
					// In some scenarios (like the AJAX control wanting ALL auth requests possible),
					// we don't want to create associations with every Provider.  But we'll use
					// associations where they are already formed from previous authentications.
					association = getAssociation(relyingParty, endpoint, createNewAssociationsAsNeeded);
					if (association == null && createNewAssociationsAsNeeded) {
						Logger.WarnFormat("Failed to create association with {0}.  Skipping to next endpoint.", endpoint.ProviderEndpoint);
						// No association could be created.  Add it to the list of failed association
						// endpoints and skip to the next available endpoint.
						failedAssociationEndpoints.Add(endpoint);
						continue;
					}
				}

				yield return new AuthenticationRequest(endpoint, realm, returnToUrl, relyingParty);
			}

			// Now that we've run out of endpoints that respond to association requests,
			// since we apparently are still running, the caller must want another request.
			// We'll go ahead and generate the requests to OPs that may be down.
			if (failedAssociationEndpoints.Count > 0) {
				Logger.WarnFormat("Now generating requests for Provider endpoints that failed initial association attempts.");

				foreach (var endpoint in failedAssociationEndpoints) {
					Logger.WarnFormat("Creating authentication request for user supplied Identifier: {0}", userSuppliedIdentifier);
					Logger.DebugFormat("Realm: {0}", realm);
					Logger.DebugFormat("Return To: {0}", returnToUrl);

					// Create the auth request, but prevent it from attempting to create an association
					// because we've already tried.  Let's not have it waste time trying again.
					var authRequest = new AuthenticationRequest(endpoint, realm, returnToUrl, relyingParty);
					authRequest.associationPreference = AssociationPreference.IfAlreadyEstablished;
					yield return authRequest;
				}
			}
		}

		internal static AuthenticationRequest CreateSingle(Identifier userSuppliedIdentifier,
			OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl) {

			// Just return the first generated request.
			var requests = Create(userSuppliedIdentifier, relyingParty, realm, returnToUrl, true).GetEnumerator();
			if (requests.MoveNext()) {
				return requests.Current;
			} else {
				throw new OpenIdException(Strings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Returns a filtered and sorted list of the available OP endpoints for a discovered Identifier.
		/// </summary>
		private static List<ServiceEndpoint> filterAndSortEndpoints(IEnumerable<ServiceEndpoint> endpoints,
			OpenIdRelyingParty relyingParty) {
			if (endpoints == null) throw new ArgumentNullException("endpoints");
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");

			// Construct the endpoints filters based on criteria given by the host web site.
			EndpointSelector versionFilter = ep => ((ServiceEndpoint)ep).Protocol.Version >= Protocol.Lookup(relyingParty.Settings.MinimumRequiredOpenIdVersion).Version;
			EndpointSelector hostingSiteFilter = relyingParty.EndpointFilter ?? (ep => true);

			bool anyFilteredOut = false;
			var filteredEndpoints = new List<IXrdsProviderEndpoint>();
			foreach (ServiceEndpoint endpoint in endpoints) {
				if (versionFilter(endpoint) && hostingSiteFilter(endpoint)) {
					filteredEndpoints.Add(endpoint);
				} else {
					anyFilteredOut = true;
				}
			}

			// Sort endpoints so that the first one in the list is the most preferred one.
			filteredEndpoints.Sort(relyingParty.EndpointOrder);

			List<ServiceEndpoint> endpointList = new List<ServiceEndpoint>(filteredEndpoints.Count);
			foreach (ServiceEndpoint endpoint in filteredEndpoints) {
				endpointList.Add(endpoint);
			}

			if (anyFilteredOut) {
				Logger.DebugFormat("Some endpoints were filtered out.  Total endpoints remaining: {0}", filteredEndpoints.Count);
			}
			if (Logger.IsDebugEnabled) {
				if (Util.AreSequencesEquivalent(endpoints, endpointList)) {
					Logger.Debug("Filtering and sorting of endpoints did not affect the list.");
				} else {
					Logger.Debug("After filtering and sorting service endpoints, this is the new prioritized list:");
					Logger.Debug(Util.ToString(filteredEndpoints, true));
				}
			}

			return endpointList;
		}

		static Association getAssociation(OpenIdRelyingParty relyingParty, ServiceEndpoint provider, bool createNewAssociationIfNeeded) {
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (provider == null) throw new ArgumentNullException("provider");

			// If the RP has no application store for associations, there's no point in creating one.
			if (relyingParty.Store == null) {
				return null;
			}

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
					// Confirm that the association matches the type we requested (section 8.2.1)
					// if this is a 2.0 OP (1.x OPs had freedom to differ from the requested type).
					if (assoc != null && provider.Protocol.Version.Major >= 2) {
						if (!string.Equals(
							req.Args[provider.Protocol.openid.assoc_type],
							Util.GetRequiredArg(req.Response.Args, provider.Protocol.openidnp.assoc_type),
							StringComparison.Ordinal) ||
							!string.Equals(
							req.Args[provider.Protocol.openid.session_type],
							Util.GetRequiredArg(req.Response.Args, provider.Protocol.openidnp.session_type),
							StringComparison.Ordinal)) {
							Logger.ErrorFormat("Provider responded with contradicting association parameters.  Requested [{0}, {1}] but got [{2}, {3}] back.",
								req.Args[provider.Protocol.openid.assoc_type],
								req.Args[provider.Protocol.openid.session_type],
								Util.GetRequiredArg(req.Response.Args, provider.Protocol.openidnp.assoc_type),
								Util.GetRequiredArg(req.Response.Args, provider.Protocol.openidnp.session_type));

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

				string token = new Token(endpoint).Serialize(this.RelyingParty.Store);
				if (token != null) {
					UriUtil.AppendQueryArgs(returnToBuilder, new Dictionary<string, string> {
						{ DotNetOpenId.RelyingParty.Token.TokenKey, token },
					});
				}

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

				Association association = null;
				if (associationPreference != AssociationPreference.Never) {
					association = getAssociation(RelyingParty, endpoint, associationPreference == AssociationPreference.IfPossible);
					if (association != null) {
						qsArgs.Add(protocol.openid.assoc_handle, association.Handle);
					} else {
						// Avoid trying to create the association again if the redirecting response
						// is generated again.
						associationPreference = AssociationPreference.IfAlreadyEstablished;
					}
				}

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
