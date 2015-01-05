//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Net.Http;
	using System.ServiceModel.Channels;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Facilitates customization and creation and an authentication request
	/// that a Relying Party is preparing to send.
	/// </summary>
	internal class AuthenticationRequest : IAuthenticationRequest {
		/// <summary>
		/// The name of the internal callback parameter to use to store the user-supplied identifier.
		/// </summary>
		internal const string UserSuppliedIdentifierParameterName = OpenIdUtilities.CustomParameterPrefix + "userSuppliedIdentifier";

		/// <summary>
		/// The relying party that created this request object.
		/// </summary>
		private readonly OpenIdRelyingParty RelyingParty;

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
		/// A value indicating whether the return_to callback arguments must be signed.
		/// </summary>
		/// <remarks>
		/// This field defaults to false, but is set to true as soon as the first callback argument
		/// is added that indicates it must be signed.  At which point, all arguments are signed
		/// even if individual ones did not need to be.
		/// </remarks>
		private bool returnToArgsMustBeSigned;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationRequest"/> class.
		/// </summary>
		/// <param name="discoveryResult">The endpoint that describes the OpenID Identifier and Provider that will complete the authentication.</param>
		/// <param name="realm">The realm, or root URL, of the host web site.</param>
		/// <param name="returnToUrl">The base return_to URL that the Provider should return the user to to complete authentication.  This should not include callback parameters as these should be added using the <see cref="AddCallbackArguments(string, string)"/> method.</param>
		/// <param name="relyingParty">The relying party that created this instance.</param>
		private AuthenticationRequest(IdentifierDiscoveryResult discoveryResult, Realm realm, Uri returnToUrl, OpenIdRelyingParty relyingParty) {
			Requires.NotNull(discoveryResult, "discoveryResult");
			Requires.NotNull(realm, "realm");
			Requires.NotNull(returnToUrl, "returnToUrl");
			Requires.NotNull(relyingParty, "relyingParty");

			this.DiscoveryResult = discoveryResult;
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
			get { return this.IsDirectedIdentity ? null : this.DiscoveryResult.ClaimedIdentifier; }
		}

		/// <summary>
		/// Gets a value indicating whether the authenticating user has chosen to let the Provider
		/// determine and send the ClaimedIdentifier after authentication.
		/// </summary>
		public bool IsDirectedIdentity {
			get { return this.DiscoveryResult.ClaimedIdentifier == this.DiscoveryResult.Protocol.ClaimedIdentifierForOPIdentifier; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this request only carries extensions
		/// and is not a request to verify that the user controls some identifier.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this request is merely a carrier of extensions and is not
		/// about an OpenID identifier; otherwise, <c>false</c>.
		/// </value>
		public bool IsExtensionOnly { get; set; }

		/// <summary>
		/// Gets information about the OpenId Provider, as advertised by the
		/// OpenId discovery documents found at the <see cref="ClaimedIdentifier"/>
		/// location.
		/// </summary>
		public IProviderEndpoint Provider {
			get { return this.DiscoveryResult; }
		}

		/// <summary>
		/// Gets the discovery result leading to the formulation of this request.
		/// </summary>
		/// <value>The discovery result.</value>
		public IdentifierDiscoveryResult DiscoveryResult { get; private set; }

		#endregion

		/// <summary>
		/// Gets or sets how an association may or should be created or used 
		/// in the formulation of the authentication request.
		/// </summary>
		internal AssociationPreference AssociationPreference {
			get { return this.associationPreference; }
			set { this.associationPreference = value; }
		}

		/// <summary>
		/// Gets the extensions that have been added to the request.
		/// </summary>
		internal IEnumerable<IOpenIdMessageExtension> AppliedExtensions {
			get { return this.extensions; }
		}

		/// <summary>
		/// Gets the list of extensions for this request.
		/// </summary>
		internal IList<IOpenIdMessageExtension> Extensions {
			get { return this.extensions; }
		}

		#region IAuthenticationRequest methods

		/// <summary>
		/// Gets the HTTP response the relying party should send to the user agent
		/// to redirect it to the OpenID Provider to start the OpenID authentication process.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The response message that will cause the client to redirect to the Provider.
		/// </returns>
		public async Task<HttpResponseMessage> GetRedirectingResponseAsync(CancellationToken cancellationToken) {
			foreach (var behavior in this.RelyingParty.Behaviors) {
				behavior.OnOutgoingAuthenticationRequest(this);
			}

			var request = await this.CreateRequestMessageAsync(cancellationToken);
			return await this.RelyingParty.Channel.PrepareResponseAsync(request, cancellationToken);
		}

		/// <summary>
		/// Makes a dictionary of key/value pairs available when the authentication is completed.
		/// </summary>
		/// <param name="arguments">The arguments to add to the request's return_to URI.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against eavesdropping in transit.  No
		/// privacy-sensitive data should be stored using this method.</para>
		/// 	<para>The values stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArguments"/>, which will only return the value
		/// if it hasn't been tampered with in transit.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		public void AddCallbackArguments(IDictionary<string, string> arguments) {
			ErrorUtilities.VerifyOperation(this.RelyingParty.CanSignCallbackArguments, OpenIdStrings.CallbackArgumentsRequireSecretStore, typeof(IRelyingPartyAssociationStore).Name, typeof(OpenIdRelyingParty).Name);

			this.returnToArgsMustBeSigned = true;
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
		/// 	<para>Note that these values are NOT protected against eavesdropping in transit.  No
		/// privacy-sensitive data should be stored using this method.</para>
		/// 	<para>The value stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>, which will only return the value
		/// if it hasn't been tampered with in transit.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		public void AddCallbackArguments(string key, string value) {
			ErrorUtilities.VerifyOperation(this.RelyingParty.CanSignCallbackArguments, OpenIdStrings.CallbackArgumentsRequireSecretStore, typeof(IRelyingPartyAssociationStore).Name, typeof(OpenIdRelyingParty).Name);

			this.returnToArgsMustBeSigned = true;
			this.returnToArgs.Add(key, value);
		}

		/// <summary>
		/// Makes a key/value pair available when the authentication is completed.
		/// </summary>
		/// <param name="key">The parameter name.</param>
		/// <param name="value">The value of the argument.  Must not be null.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against tampering in transit.  No
		/// security-sensitive data should be stored using this method.</para>
		/// 	<para>The value stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		public void SetCallbackArgument(string key, string value) {
			ErrorUtilities.VerifyOperation(this.RelyingParty.CanSignCallbackArguments, OpenIdStrings.CallbackArgumentsRequireSecretStore, typeof(IRelyingPartyAssociationStore).Name, typeof(OpenIdRelyingParty).Name);

			this.returnToArgsMustBeSigned = true;
			this.returnToArgs[key] = value;
		}

		/// <summary>
		/// Makes a key/value pair available when the authentication is completed without
		/// requiring a return_to signature to protect against tampering of the callback argument.
		/// </summary>
		/// <param name="key">The parameter name.</param>
		/// <param name="value">The value of the argument.  Must not be null.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against eavesdropping or tampering in transit.  No
		/// security-sensitive data should be stored using this method. </para>
		/// 	<para>The value stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		public void SetUntrustedCallbackArgument(string key, string value) {
			this.returnToArgs[key] = value;
		}

		/// <summary>
		/// Adds an OpenID extension to the request directed at the OpenID provider.
		/// </summary>
		/// <param name="extension">The initialized extension to add to the request.</param>
		public void AddExtension(IOpenIdMessageExtension extension) {
			this.extensions.Add(extension);
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		internal static async Task<IEnumerable<AuthenticationRequest>> CreateAsync(Identifier userSuppliedIdentifier, OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl, bool createNewAssociationsAsNeeded, CancellationToken cancellationToken) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			Requires.NotNull(relyingParty, "relyingParty");
			Requires.NotNull(realm, "realm");

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
				if (!userSuppliedIdentifier.TryRequireSsl(out userSuppliedIdentifier)) {
					// But at least log the failure.
					Logger.OpenId.WarnFormat("RequireSsl mode is on, so discovery on insecure identifier {0} will yield no results.", userSuppliedIdentifier);
				}
			}

			if (Logger.OpenId.IsWarnEnabled() && returnToUrl.Query != null) {
				NameValueCollection returnToArgs = HttpUtility.ParseQueryString(returnToUrl.Query);
				foreach (string key in returnToArgs) {
					if (OpenIdRelyingParty.IsOpenIdSupportingParameter(key)) {
						Logger.OpenId.WarnFormat("OpenID argument \"{0}\" found in return_to URL.  This can corrupt an OpenID response.", key);
					}
				}
			}

			// Throw an exception now if the realm and the return_to URLs don't match
			// as required by the provider.  We could wait for the provider to test this and
			// fail, but this will be faster and give us a better error message.
			ErrorUtilities.VerifyProtocol(realm.Contains(returnToUrl), OpenIdStrings.ReturnToNotUnderRealm, returnToUrl, realm);

			// Perform discovery right now (not deferred).
			IEnumerable<IdentifierDiscoveryResult> serviceEndpoints;
			try {
				var identifierDiscoveryResults = await relyingParty.DiscoverAsync(userSuppliedIdentifier, cancellationToken);
				var results = identifierDiscoveryResults.CacheGeneratedResults();

				// If any OP Identifier service elements were found, we must not proceed
				// to use any Claimed Identifier services, per OpenID 2.0 sections 7.3.2.2 and 11.2.
				// For a discussion on this topic, see
				// http://groups.google.com/group/dotnetopenid/browse_thread/thread/4b5a8c6b2210f387/5e25910e4d2252c8
				// Usually the Discover method we called will automatically filter this for us, but
				// just to be sure, we'll do it here as well since the RP may be configured to allow
				// these dual identifiers for assertion verification purposes.
				var opIdentifiers = results.Where(result => result.ClaimedIdentifier == result.Protocol.ClaimedIdentifierForOPIdentifier).CacheGeneratedResults();
				var claimedIdentifiers = results.Where(result => result.ClaimedIdentifier != result.Protocol.ClaimedIdentifierForOPIdentifier);
				serviceEndpoints = opIdentifiers.Any() ? opIdentifiers : claimedIdentifiers;
			} catch (ProtocolException ex) {
				Logger.Yadis.ErrorFormat("Error while performing discovery on: \"{0}\": {1}", userSuppliedIdentifier, ex);
				serviceEndpoints = Enumerable.Empty<IdentifierDiscoveryResult>();
			}

			// Filter disallowed endpoints.
			serviceEndpoints = relyingParty.SecuritySettings.FilterEndpoints(serviceEndpoints);

			// Call another method that defers request generation.
			return await CreateInternalAsync(userSuppliedIdentifier, relyingParty, realm, returnToUrl, serviceEndpoints, createNewAssociationsAsNeeded, cancellationToken);
		}

		/// <summary>
		/// Creates an instance of <see cref="AuthenticationRequest"/> FOR TESTING PURPOSES ONLY.
		/// </summary>
		/// <param name="discoveryResult">The discovery result.</param>
		/// <param name="realm">The realm.</param>
		/// <param name="returnTo">The return to.</param>
		/// <param name="rp">The relying party.</param>
		/// <returns>The instantiated <see cref="AuthenticationRequest"/>.</returns>
		internal static AuthenticationRequest CreateForTest(IdentifierDiscoveryResult discoveryResult, Realm realm, Uri returnTo, OpenIdRelyingParty rp) {
			return new AuthenticationRequest(discoveryResult, realm, returnTo, rp);
		}

		/// <summary>
		/// Creates the request message to send to the Provider,
		/// based on the properties in this instance.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The message to send to the Provider.
		/// </returns>
		internal Task<SignedResponseRequest> CreateRequestMessageTestHookAsync(CancellationToken cancellationToken) {
			return this.CreateRequestMessageAsync(cancellationToken);
		}

		/// <summary>
		/// Performs deferred request generation for the <see cref="CreateAsync" /> method.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="realm">The realm.</param>
		/// <param name="returnToUrl">The return_to base URL.</param>
		/// <param name="serviceEndpoints">The discovered service endpoints on the Claimed Identifier.</param>
		/// <param name="createNewAssociationsAsNeeded">if set to <c>true</c>, associations that do not exist between this Relying Party and the asserting Providers are created before the authentication request is created.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <remarks>
		/// All data validation and cleansing steps must have ALREADY taken place
		/// before calling this method.
		/// </remarks>
		private static async Task<IEnumerable<AuthenticationRequest>> CreateInternalAsync(Identifier userSuppliedIdentifier, OpenIdRelyingParty relyingParty, Realm realm, Uri returnToUrl, IEnumerable<IdentifierDiscoveryResult> serviceEndpoints, bool createNewAssociationsAsNeeded, CancellationToken cancellationToken) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			Requires.NotNull(relyingParty, "relyingParty");
			Requires.NotNull(realm, "realm");
			Requires.NotNull(serviceEndpoints, "serviceEndpoints");
			////
			// If shared associations are required, then we had better have an association store.
			ErrorUtilities.VerifyOperation(!relyingParty.SecuritySettings.RequireAssociation || relyingParty.AssociationManager.HasAssociationStore, OpenIdStrings.AssociationStoreRequired);

			Logger.Yadis.InfoFormat("Performing discovery on user-supplied identifier: {0}", userSuppliedIdentifier);
			IEnumerable<IdentifierDiscoveryResult> endpoints = FilterAndSortEndpoints(serviceEndpoints, relyingParty);

			var authRequestResults = endpoints.Select(async endpoint => {
				Logger.OpenId.DebugFormat("Creating authentication request for user supplied Identifier: {0}", userSuppliedIdentifier);

				// The strategy here is to prefer endpoints with whom we can create associations.
				if (relyingParty.AssociationManager.HasAssociationStore) {
					// In some scenarios (like the AJAX control wanting ALL auth requests possible),
					// we don't want to create associations with every Provider.  But we'll use
					// associations where they are already formed from previous authentications.
					Association association = createNewAssociationsAsNeeded ? await relyingParty.AssociationManager.GetOrCreateAssociationAsync(endpoint, cancellationToken) : relyingParty.AssociationManager.GetExistingAssociation(endpoint);
					if (association == null && createNewAssociationsAsNeeded) {
						Logger.OpenId.WarnFormat("Failed to create association with {0}.  Skipping to next endpoint.", endpoint.ProviderEndpoint);

						// No association could be created.  Add it to the list of failed association
						// endpoints and skip to the next available endpoint.
						return new KeyValuePair<IdentifierDiscoveryResult, AuthenticationRequest>(endpoint, null);
					}
				}

				return new KeyValuePair<IdentifierDiscoveryResult, AuthenticationRequest>(endpoint, new AuthenticationRequest(endpoint, realm, returnToUrl, relyingParty));
			}).ToList();

			await Task.WhenAll(authRequestResults);
			var results = (from pair in authRequestResults where pair.Result.Value != null select pair.Result.Value).ToList();

			// Maintain a list of endpoints that we could not form an association with.
			// We'll fallback to generating requests to these if the ones we CAN create
			// an association with run out.
			var failedAssociationEndpoints = (from pair in authRequestResults where pair.Result.Value == null select pair.Result.Key).ToList();

			// Now that we've run out of endpoints that respond to association requests,
			// since we apparently are still running, the caller must want another request.
			// We'll go ahead and generate the requests to OPs that may be down -- 
			// unless associations are set as required in our security settings.
			if (failedAssociationEndpoints.Count > 0) {
				if (relyingParty.SecuritySettings.RequireAssociation) {
					Logger.OpenId.Warn("Associations could not be formed with some Providers.  Security settings require shared associations for authentication requests so these will be skipped.");
				} else {
					Logger.OpenId.Debug("Now generating requests for Provider endpoints that failed initial association attempts.");

					foreach (var endpoint in failedAssociationEndpoints) {
						Logger.OpenId.DebugFormat("Creating authentication request for user supplied Identifier: {0} at endpoint: {1}", userSuppliedIdentifier, endpoint.ProviderEndpoint.AbsoluteUri);

						// Create the auth request, but prevent it from attempting to create an association
						// because we've already tried.  Let's not have it waste time trying again.
						var authRequest = new AuthenticationRequest(endpoint, realm, returnToUrl, relyingParty);
						authRequest.associationPreference = AssociationPreference.IfAlreadyEstablished;
						results.Add(authRequest);
					}
				}
			}

			return results;
		}

		/// <summary>
		/// Returns a filtered and sorted list of the available OP endpoints for a discovered Identifier.
		/// </summary>
		/// <param name="endpoints">The endpoints.</param>
		/// <param name="relyingParty">The relying party.</param>
		/// <returns>A filtered and sorted list of endpoints; may be empty if the input was empty or the filter removed all endpoints.</returns>
		private static List<IdentifierDiscoveryResult> FilterAndSortEndpoints(IEnumerable<IdentifierDiscoveryResult> endpoints, OpenIdRelyingParty relyingParty) {
			Requires.NotNull(endpoints, "endpoints");
			Requires.NotNull(relyingParty, "relyingParty");

			bool anyFilteredOut = false;
			var filteredEndpoints = new List<IdentifierDiscoveryResult>();
			foreach (var endpoint in endpoints) {
				if (relyingParty.FilterEndpoint(endpoint)) {
					filteredEndpoints.Add(endpoint);
				} else {
					anyFilteredOut = true;
				}
			}

			// Sort endpoints so that the first one in the list is the most preferred one.
			filteredEndpoints.OrderBy(ep => ep, relyingParty.EndpointOrder);

			var endpointList = new List<IdentifierDiscoveryResult>(filteredEndpoints.Count);
			foreach (var endpoint in filteredEndpoints) {
				endpointList.Add(endpoint);
			}

			if (anyFilteredOut) {
				Logger.Yadis.DebugFormat("Some endpoints were filtered out.  Total endpoints remaining: {0}", filteredEndpoints.Count);
			}
			if (Logger.Yadis.IsDebugEnabled()) {
				if (MessagingUtilities.AreEquivalent(endpoints, endpointList)) {
					Logger.Yadis.Debug("Filtering and sorting of endpoints did not affect the list.");
				} else {
					Logger.Yadis.Debug("After filtering and sorting service endpoints, this is the new prioritized list:");
					Logger.Yadis.Debug(Util.ToStringDeferred(filteredEndpoints, true).ToString());
				}
			}

			return endpointList;
		}

		/// <summary>
		/// Creates the request message to send to the Provider,
		/// based on the properties in this instance.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The message to send to the Provider.
		/// </returns>
		private async Task<SignedResponseRequest> CreateRequestMessageAsync(CancellationToken cancellationToken) {
			Association association = await this.GetAssociationAsync(cancellationToken);

			SignedResponseRequest request;
			if (!this.IsExtensionOnly) {
				CheckIdRequest authRequest = new CheckIdRequest(this.DiscoveryResult.Version, this.DiscoveryResult.ProviderEndpoint, this.Mode);
				authRequest.ClaimedIdentifier = this.DiscoveryResult.ClaimedIdentifier;
				authRequest.LocalIdentifier = this.DiscoveryResult.ProviderLocalIdentifier;
				request = authRequest;
			} else {
				request = new SignedResponseRequest(this.DiscoveryResult.Version, this.DiscoveryResult.ProviderEndpoint, this.Mode);
			}
			request.Realm = this.Realm;
			request.ReturnTo = this.ReturnToUrl;
			request.AssociationHandle = association != null ? association.Handle : null;
			request.SignReturnTo = this.returnToArgsMustBeSigned;
			request.AddReturnToArguments(this.returnToArgs);
			if (this.DiscoveryResult.UserSuppliedIdentifier != null && OpenIdElement.Configuration.RelyingParty.PreserveUserSuppliedIdentifier) {
				request.AddReturnToArguments(UserSuppliedIdentifierParameterName, this.DiscoveryResult.UserSuppliedIdentifier.OriginalString);
			}
			foreach (IOpenIdMessageExtension extension in this.extensions) {
				request.Extensions.Add(extension);
			}

			return request;
		}

		/// <summary>
		/// Gets the association to use for this authentication request.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The association to use; <c>null</c> to use 'dumb mode'.
		/// </returns>
		private async Task<Association> GetAssociationAsync(CancellationToken cancellationToken) {
			Association association = null;
			switch (this.associationPreference) {
				case AssociationPreference.IfPossible:
					association = await this.RelyingParty.AssociationManager.GetOrCreateAssociationAsync(this.DiscoveryResult, cancellationToken);
					if (association == null) {
						// Avoid trying to create the association again if the redirecting response
						// is generated again.
						this.associationPreference = AssociationPreference.IfAlreadyEstablished;
					}
					break;
				case AssociationPreference.IfAlreadyEstablished:
					association = this.RelyingParty.AssociationManager.GetExistingAssociation(this.DiscoveryResult);
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
