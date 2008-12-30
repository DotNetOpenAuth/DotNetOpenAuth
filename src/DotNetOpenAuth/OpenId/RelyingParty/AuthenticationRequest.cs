//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	internal class AuthenticationRequest : IAuthenticationRequest {
		internal OpenIdRelyingParty RelyingParty;
		internal AssociationPreference associationPreference = AssociationPreference.IfPossible;
		private readonly ServiceEndpoint endpoint;
		private readonly Protocol protocol;
		private List<IOpenIdMessageExtension> extensions = new List<IOpenIdMessageExtension>();

		/// <summary>
		/// Arguments to add to the return_to part of the query string, so that
		/// these values come back to the consumer when the user agent returns.
		/// </summary>
		private Dictionary<string, string> returnToArgs = new Dictionary<string, string>();

		private AuthenticationRequest(ServiceEndpoint endpoint, Realm realm, Uri returnToUrl, OpenIdRelyingParty relyingParty) {
			ErrorUtilities.VerifyArgumentNotNull(endpoint, "endpoint");
			ErrorUtilities.VerifyArgumentNotNull(realm, "realm");
			ErrorUtilities.VerifyArgumentNotNull(returnToUrl, "returnToUrl");
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");

			this.endpoint = endpoint;
			this.protocol = endpoint.Protocol;
			this.RelyingParty = relyingParty;
			this.Realm = realm;
			this.ReturnToUrl = returnToUrl;

			this.Mode = AuthenticationRequestMode.Setup;
		}

		#region IAuthenticationRequest Members

		public AuthenticationRequestMode Mode { get; set; }

		public UserAgentResponse RedirectingResponse {
			get { return this.RelyingParty.Channel.Send(this.CreateRequestMessage()); }
		}

		public Uri ReturnToUrl { get; private set; }

		public Realm Realm { get; private set; }

		public Identifier ClaimedIdentifier {
			get { return this.IsDirectedIdentity ? null : this.endpoint.ClaimedIdentifier; }
		}

		public bool IsDirectedIdentity {
			get { return this.endpoint.ClaimedIdentifier == this.endpoint.Protocol.ClaimedIdentifierForOPIdentifier; }
		}

		IProviderEndpoint IAuthenticationRequest.Provider {
			get { return this.endpoint; }
		}

		public Version ProviderVersion {
			get { return this.endpoint.Protocol.Version; }
		}

		public void AddCallbackArguments(IDictionary<string, string> arguments) {
			ErrorUtilities.VerifyArgumentNotNull(arguments, "arguments");

			foreach (var pair in arguments) {
				this.returnToArgs.Add(pair.Key, pair.Value);
			}
		}

		public void AddCallbackArguments(string key, string value) {
			this.returnToArgs.Add(key, value);
		}

		public void AddExtension(IOpenIdMessageExtension extension) {
			ErrorUtilities.VerifyArgumentNotNull(extension, "extension");
			this.extensions.Add(extension);
		}

		public void RedirectToProvider() {
			this.RedirectingResponse.Send();
		}

		#endregion

		/// <summary>
		/// Performs identifier discovery, creates associations and generates authentication requests
		/// on-demand for as long as new ones can be generated based on the results of Identifier discovery.
		/// </summary>
		internal static IEnumerable<AuthenticationRequest> Create(Identifier userSuppliedIdentifier, OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl, bool createNewAssociationsAsNeeded) {
			// We have a long data validation and preparation process
			ErrorUtilities.VerifyArgumentNotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");
			ErrorUtilities.VerifyArgumentNotNull(realm, "realm");

			userSuppliedIdentifier = userSuppliedIdentifier.TrimFragment();
			if (relyingParty.SecuritySettings.RequireSsl) {
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
			ErrorUtilities.VerifyProtocol(realm.Contains(returnToUrl), OpenIdStrings.ReturnToNotUnderRealm, returnToUrl, realm);

			// Perform discovery right now (not deferred).
			var serviceEndpoints = userSuppliedIdentifier.Discover(relyingParty.WebRequestHandler);

			// Call another method that defers request generation.
			return CreateInternal(userSuppliedIdentifier, relyingParty, realm, returnToUrl, serviceEndpoints, createNewAssociationsAsNeeded);
		}

		/// <summary>
		/// Performs request generation for the <see cref="Create"/> method.
		/// All data validation and cleansing steps must have ALREADY taken place.
		/// </summary>
		private static IEnumerable<AuthenticationRequest> CreateInternal(Identifier userSuppliedIdentifier, OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl, IEnumerable<ServiceEndpoint> serviceEndpoints, bool createNewAssociationsAsNeeded) {
			Logger.InfoFormat("Performing discovery on user-supplied identifier: {0}", userSuppliedIdentifier);
			IEnumerable<ServiceEndpoint> endpoints = FilterAndSortEndpoints(serviceEndpoints, relyingParty);

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
				if (relyingParty.AssociationStore != null) {
					// In some scenarios (like the AJAX control wanting ALL auth requests possible),
					// we don't want to create associations with every Provider.  But we'll use
					// associations where they are already formed from previous authentications.
					association = createNewAssociationsAsNeeded ? relyingParty.GetOrCreateAssociation(endpoint.ProviderDescription) : relyingParty.GetExistingAssociation(endpoint.ProviderDescription);
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

		/// <summary>
		/// Returns a filtered and sorted list of the available OP endpoints for a discovered Identifier.
		/// </summary>
		private static List<ServiceEndpoint> FilterAndSortEndpoints(IEnumerable<ServiceEndpoint> endpoints, OpenIdRelyingParty relyingParty) {
			ErrorUtilities.VerifyArgumentNotNull(endpoints, "endpoints");
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");

			// Construct the endpoints filters based on criteria given by the host web site.
			EndpointSelector versionFilter = ep => ((ServiceEndpoint)ep).Protocol.Version >= Protocol.Lookup(relyingParty.SecuritySettings.MinimumRequiredOpenIdVersion).Version;
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
				if (MessagingUtilities.AreEquivalent(endpoints, endpointList)) {
					Logger.Debug("Filtering and sorting of endpoints did not affect the list.");
				} else {
					Logger.Debug("After filtering and sorting service endpoints, this is the new prioritized list:");
					Logger.Debug(Util.ToStringDeferred(filteredEndpoints, true));
				}
			}

			return endpointList;
		}

		private CheckIdRequest CreateRequestMessage() {
			Association association = this.GetAssociation();

			CheckIdRequest request = new CheckIdRequest(this.ProviderVersion, this.endpoint.ProviderEndpoint, this.Mode);
			request.ClaimedIdentifier = this.endpoint.ClaimedIdentifier;
			request.LocalIdentifier = this.endpoint.ProviderLocalIdentifier;
			request.Realm = this.Realm;
			request.ReturnTo = this.ReturnToUrl;
			request.AssociationHandle = association != null ? association.Handle : null;
			request.AddReturnToArguments(this.returnToArgs);
			foreach (IOpenIdMessageExtension extension in this.extensions) {
				request.Extensions.Add(extension);
			}

			return request;
		}

		private Association GetAssociation() {
			Association association = null;
			switch (this.associationPreference) {
				case AssociationPreference.IfPossible:
					association = this.RelyingParty.GetOrCreateAssociation(this.endpoint.ProviderDescription);
					if (association == null) {
						// Avoid trying to create the association again if the redirecting response
						// is generated again.
						this.associationPreference = AssociationPreference.IfAlreadyEstablished;
					}
					break;
				case AssociationPreference.IfAlreadyEstablished:
					association = this.RelyingParty.GetExistingAssociation(this.endpoint.ProviderDescription);
					break;
				case AssociationPreference.Never:
					break;
				default:
					throw new InternalErrorException();
			}

			return association;
		}
	}
}
