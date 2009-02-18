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
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Facilitates customization and creation and an authentication request
	/// that a Relying Party is preparing to send.
	/// </summary>
	internal class AuthenticationRequest : IAuthenticationRequest {
		/// <summary>
		/// The name of the internal callback parameter to use to store the user-supplied identifier.
		/// </summary>
		internal const string UserSuppliedIdentifierParameterName = "dnoi.userSuppliedIdentifier";

		/// <summary>
		/// The relying party that created this request object.
		/// </summary>
		private readonly OpenIdRelyingParty RelyingParty;

		/// <summary>
		/// The endpoint that describes the particular OpenID Identifier and Provider that
		/// will be used to create the authentication request.
		/// </summary>
		private readonly ServiceEndpoint endpoint;

		/// <summary>
		/// How an association may or should be created or used in the formulation of the 
		/// authentication request.
		/// </summary>
		private AssociationPreference associationPreference = AssociationPreference.IfPossible;

		/// <summary>
		/// The extensions that have been added to this authentication request.
		/// </summary>
		private List<IOpenIdMessageExtension> extensions = new List<IOpenIdMessageExtension>();

		/// <summary>
		/// Arguments to add to the return_to part of the query string, so that
		/// these values come back to the consumer when the user agent returns.
		/// </summary>
		private Dictionary<string, string> returnToArgs = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationRequest"/> class.
		/// </summary>
		/// <param name="endpoint">The endpoint that describes the OpenID Identifier and Provider that will complete the authentication.</param>
		/// <param name="realm">The realm, or root URL, of the host web site.</param>
		/// <param name="returnToUrl">The base return_to URL that the Provider should return the user to to complete authentication.  This should not include callback parameters as these should be added using the <see cref="AddCallbackArguments(string, string)"/> method.</param>
		/// <param name="relyingParty">The relying party that created this instance.</param>
		private AuthenticationRequest(ServiceEndpoint endpoint, Realm realm, Uri returnToUrl, OpenIdRelyingParty relyingParty) {
			ErrorUtilities.VerifyArgumentNotNull(endpoint, "endpoint");
			ErrorUtilities.VerifyArgumentNotNull(realm, "realm");
			ErrorUtilities.VerifyArgumentNotNull(returnToUrl, "returnToUrl");
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");

			this.endpoint = endpoint;
			this.RelyingParty = relyingParty;
			this.Realm = realm;
			this.ReturnToUrl = returnToUrl;

			this.Mode = AuthenticationRequestMode.Setup;
		}

		#region IAuthenticationRequest Members

		/// <summary>
		/// Gets or sets the mode the Provider should use during authentication.
		/// </summary>
		/// <value></value>
		public AuthenticationRequestMode Mode { get; set; }

		/// <summary>
		/// Gets the HTTP response the relying party should send to the user agent
		/// to redirect it to the OpenID Provider to start the OpenID authentication process.
		/// </summary>
		/// <value></value>
		public UserAgentResponse RedirectingResponse {
			get { return this.RelyingParty.Channel.PrepareResponse(this.CreateRequestMessage()); }
		}

		/// <summary>
		/// Gets the URL that the user agent will return to after authentication
		/// completes or fails at the Provider.
		/// </summary>
		/// <value></value>
		public Uri ReturnToUrl { get; private set; }

		/// <summary>
		/// Gets the URL that identifies this consumer web application that
		/// the Provider will display to the end user.
		/// </summary>
		public Realm Realm { get; private set; }

		/// <summary>
		/// Gets the Claimed Identifier that the User Supplied Identifier
		/// resolved to.  Null if the user provided an OP Identifier
		/// (directed identity).
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// Null is returned if the user is using the directed identity feature
		/// of OpenID 2.0 to make it nearly impossible for a relying party site
		/// to improperly store the reserved OpenID URL used for directed identity
		/// as a user's own Identifier.
		/// However, to test for the Directed Identity feature, please test the
		/// <see cref="IsDirectedIdentity"/> property rather than testing this
		/// property for a null value.
		/// </remarks>
		public Identifier ClaimedIdentifier {
			get { return this.IsDirectedIdentity ? null : this.endpoint.ClaimedIdentifier; }
		}

		/// <summary>
		/// Gets a value indicating whether the authenticating user has chosen to let the Provider
		/// determine and send the ClaimedIdentifier after authentication.
		/// </summary>
		/// <value></value>
		public bool IsDirectedIdentity {
			get { return this.endpoint.ClaimedIdentifier == this.endpoint.Protocol.ClaimedIdentifierForOPIdentifier; }
		}

		/// <summary>
		/// Gets information about the OpenId Provider, as advertised by the
		/// OpenId discovery documents found at the <see cref="ClaimedIdentifier"/>
		/// location.
		/// </summary>
		/// <value></value>
		IProviderEndpoint IAuthenticationRequest.Provider {
			get { return this.endpoint; }
		}

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		/// <value></value>
		public Version ProviderVersion {
			get { return this.endpoint.Protocol.Version; }
		}

		#endregion

		/// <summary>
		/// Gets or sets how an association may or should be created or used 
		/// in the formulation of the authentication request.
		/// </summary>
		internal AssociationPreference AssociationPreference {
			get { return this.associationPreference; }
			set { this.associationPreference = value; }
		}

		#region IAuthenticationRequest methods

		/// <summary>
		/// Makes a dictionary of key/value pairs available when the authentication is completed.
		/// </summary>
		/// <param name="arguments">The arguments to add to the request's return_to URI.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against tampering in transit.  No
		/// security-sensitive data should be stored using this method.</para>
		/// 	<para>The values stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArguments"/>.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		public void AddCallbackArguments(IDictionary<string, string> arguments) {
			ErrorUtilities.VerifyArgumentNotNull(arguments, "arguments");
			ErrorUtilities.VerifyOperation(this.RelyingParty.CanSignCallbackArguments, OpenIdStrings.CallbackArgumentsRequireSecretStore, typeof(IAssociationStore<Uri>).Name, typeof(OpenIdRelyingParty).Name);

			foreach (var pair in arguments) {
				ErrorUtilities.VerifyArgument(!string.IsNullOrEmpty(pair.Key), MessagingStrings.UnexpectedNullOrEmptyKey);
				ErrorUtilities.VerifyArgument(pair.Value != null, MessagingStrings.UnexpectedNullValue, pair.Key);

				this.returnToArgs.Add(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// Makes a key/value pair available when the authentication is completed.
		/// </summary>
		/// <param name="key">The parameter name.</param>
		/// <param name="value">The value of the argument.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against tampering in transit.  No
		/// security-sensitive data should be stored using this method.</para>
		/// 	<para>The value stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		public void AddCallbackArguments(string key, string value) {
			ErrorUtilities.VerifyNonZeroLength(key, "key");
			ErrorUtilities.VerifyArgumentNotNull(value, "value");
			ErrorUtilities.VerifyOperation(this.RelyingParty.CanSignCallbackArguments, OpenIdStrings.CallbackArgumentsRequireSecretStore, typeof(IAssociationStore<Uri>).Name, typeof(OpenIdRelyingParty).Name);

			this.returnToArgs.Add(key, value);
		}

		/// <summary>
		/// Adds an OpenID extension to the request directed at the OpenID provider.
		/// </summary>
		/// <param name="extension">The initialized extension to add to the request.</param>
		public void AddExtension(IOpenIdMessageExtension extension) {
			ErrorUtilities.VerifyArgumentNotNull(extension, "extension");
			this.extensions.Add(extension);
		}

		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// Execution of the current page terminates after this call.
		/// </summary>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public void RedirectToProvider() {
			this.RedirectingResponse.Send();
		}

		#endregion

		/// <summary>
		/// Performs identifier discovery, creates associations and generates authentication requests
		/// on-demand for as long as new ones can be generated based on the results of Identifier discovery.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="realm">The realm.</param>
		/// <param name="returnToUrl">The return_to base URL.</param>
		/// <param name="createNewAssociationsAsNeeded">if set to <c>true</c>, associations that do not exist between this Relying Party and the asserting Providers are created before the authentication request is created.</param>
		/// <returns>A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.</returns>
		internal static IEnumerable<AuthenticationRequest> Create(Identifier userSuppliedIdentifier, OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl, bool createNewAssociationsAsNeeded) {
			// We have a long data validation and preparation process
			ErrorUtilities.VerifyArgumentNotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");
			ErrorUtilities.VerifyArgumentNotNull(realm, "realm");

			// Normalize the portion of the return_to path that correlates to the realm for capitalization.
			// (so that if a web app base path is /MyApp/, but the URL of this request happens to be
			// /myapp/login.aspx, we bump up the return_to Url to use /MyApp/ so it matches the realm.
			UriBuilder returnTo = new UriBuilder(returnToUrl);
			if (returnTo.Path.StartsWith(realm.AbsolutePath, StringComparison.OrdinalIgnoreCase) &&
				!returnTo.Path.StartsWith(realm.AbsolutePath, StringComparison.Ordinal)) {
				returnTo.Path = realm.AbsolutePath + returnTo.Path.Substring(realm.AbsolutePath.Length);
				returnToUrl = returnTo.Uri;
			}

			userSuppliedIdentifier = userSuppliedIdentifier.TrimFragment();
			if (relyingParty.SecuritySettings.RequireSsl) {
				// Rather than check for successful SSL conversion at this stage,
				// We'll wait for secure discovery to fail on the new identifier.
				userSuppliedIdentifier.TryRequireSsl(out userSuppliedIdentifier);
			}

			if (Logger.IsWarnEnabled && returnToUrl.Query != null) {
				NameValueCollection returnToArgs = HttpUtility.ParseQueryString(returnToUrl.Query);
				foreach (string key in returnToArgs) {
					if (OpenIdRelyingParty.IsOpenIdSupportingParameter(key)) {
						Logger.WarnFormat("OpenID argument \"{0}\" found in return_to URL.  This can corrupt an OpenID response.", key);
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
		/// Performs deferred request generation for the <see cref="Create"/> method.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="realm">The realm.</param>
		/// <param name="returnToUrl">The return_to base URL.</param>
		/// <param name="serviceEndpoints">The discovered service endpoints on the Claimed Identifier.</param>
		/// <param name="createNewAssociationsAsNeeded">if set to <c>true</c>, associations that do not exist between this Relying Party and the asserting Providers are created before the authentication request is created.</param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// </returns>
		/// <remarks>
		/// All data validation and cleansing steps must have ALREADY taken place
		/// before calling this method.
		/// </remarks>
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
				if (relyingParty.AssociationManager.HasAssociationStore) {
					// In some scenarios (like the AJAX control wanting ALL auth requests possible),
					// we don't want to create associations with every Provider.  But we'll use
					// associations where they are already formed from previous authentications.
					association = createNewAssociationsAsNeeded ? relyingParty.AssociationManager.GetOrCreateAssociation(endpoint.ProviderDescription) : relyingParty.AssociationManager.GetExistingAssociation(endpoint.ProviderDescription);
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
		/// <param name="endpoints">The endpoints.</param>
		/// <param name="relyingParty">The relying party.</param>
		/// <returns>A filtered and sorted list of endpoints; may be empty if the input was empty or the filter removed all endpoints.</returns>
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

		/// <summary>
		/// Creates the authentication request message to send to the Provider,
		/// based on the properties in this instance.
		/// </summary>
		/// <returns>The message to send to the Provider.</returns>
		private CheckIdRequest CreateRequestMessage() {
			Association association = this.GetAssociation();

			CheckIdRequest request = new CheckIdRequest(this.ProviderVersion, this.endpoint.ProviderEndpoint, this.Mode);
			request.ClaimedIdentifier = this.endpoint.ClaimedIdentifier;
			request.LocalIdentifier = this.endpoint.ProviderLocalIdentifier;
			request.Realm = this.Realm;
			request.ReturnTo = this.ReturnToUrl;
			request.AssociationHandle = association != null ? association.Handle : null;
			request.AddReturnToArguments(this.returnToArgs);
			if (this.endpoint.UserSuppliedIdentifier != null) {
				request.AddReturnToArguments(UserSuppliedIdentifierParameterName, this.endpoint.UserSuppliedIdentifier);
			}
			foreach (IOpenIdMessageExtension extension in this.extensions) {
				request.Extensions.Add(extension);
			}

			return request;
		}

		/// <summary>
		/// Gets the association to use for this authentication request.
		/// </summary>
		/// <returns>The association to use; <c>null</c> to use 'dumb mode'.</returns>
		private Association GetAssociation() {
			Association association = null;
			switch (this.associationPreference) {
				case AssociationPreference.IfPossible:
					association = this.RelyingParty.AssociationManager.GetOrCreateAssociation(this.endpoint.ProviderDescription);
					if (association == null) {
						// Avoid trying to create the association again if the redirecting response
						// is generated again.
						this.associationPreference = AssociationPreference.IfAlreadyEstablished;
					}
					break;
				case AssociationPreference.IfAlreadyEstablished:
					association = this.RelyingParty.AssociationManager.GetExistingAssociation(this.endpoint.ProviderDescription);
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
