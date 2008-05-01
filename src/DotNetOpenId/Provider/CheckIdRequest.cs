using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to confirm the identity of a user.
	/// </summary>
	/// <remarks>
	/// This class handles requests for openid modes checkid_immediate and checkid_setup.
	/// </remarks>
	[DebuggerDisplay("Mode: {Mode}, IsAuthenticated: {IsAuthenticated}, LocalIdentifier: {LocalIdentifier}, OpenId: {Protocol.Version}")]
	class CheckIdRequest : AssociatedRequest, IAuthenticationRequest {
		bool? isAuthenticated;
		/// <summary>
		/// Gets/sets whether the provider has determined that the 
		/// <see cref="ClaimedIdentifier"/> belongs to the currently logged in user
		/// and wishes to share this information with the consumer.
		/// </summary>
		public bool? IsAuthenticated {
			get { return isAuthenticated; }
			set {
				isAuthenticated = value;
				InvalidateResponse();
			}
		}

		/// <summary>
		/// Whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		public bool Immediate { get; private set; }
		/// <summary>
		/// The URL the consumer site claims to use as its 'base' address.
		/// </summary>
		public Realm Realm { get; private set; }
		/// <summary>
		/// Whether the Provider should help the user select a Claimed Identifier
		/// to send back to the relying party.
		/// </summary>
		public bool IsDirectedIdentity { get; private set; }
		Identifier localIdentifier;
		/// <summary>
		/// The user identifier used by this particular provider.
		/// </summary>
		public Identifier LocalIdentifier {
			get { return localIdentifier; }
			set {
				if (IsDirectedIdentity) {
					// Keep LocalIdentifier and ClaimedIdentifier in sync
					if (ClaimedIdentifier != null && ClaimedIdentifier != value) {
						throw new InvalidOperationException(Strings.IdentifierSelectRequiresMatchingIdentifiers);
					} else {
						localIdentifier = value;
						claimedIdentifier = value;
					}
				} else {
					throw new InvalidOperationException(Strings.IdentifierSelectModeOnly);
				}
			}
		}
		Identifier claimedIdentifier;
		/// <summary>
		/// The identifier this user is claiming to control.  
		/// </summary>
		public Identifier ClaimedIdentifier {
			get { return claimedIdentifier; }
			set {
				if (IsDirectedIdentity) {
					// Keep LocalIdentifier and ClaimedIdentifier in sync
					if (LocalIdentifier != null && LocalIdentifier != value) {
						throw new InvalidOperationException(Strings.IdentifierSelectRequiresMatchingIdentifiers);
					} else {
						claimedIdentifier = value;
						localIdentifier = value;
					}
				} else {
					throw new InvalidOperationException(Strings.IdentifierSelectModeOnly);
				}
			}
		}
		/// <summary>
		/// The URL to redirect the user agent to after the authentication attempt.
		/// This must fall "under" the realm URL.
		/// </summary>
		internal Uri ReturnTo { get; private set; }
		internal override string Mode {
			get { return Immediate ? Protocol.Args.Mode.checkid_immediate : Protocol.Args.Mode.checkid_setup; }
		}
		/// <summary>
		/// Indicates whether this request has all the information necessary to formulate a response.
		/// </summary>
		public override bool IsResponseReady {
			get { 
				// The null checks on the identifiers is to make sure that an identifier_select
				// has been resolved to actual identifiers.
				return IsAuthenticated.HasValue &&
					(!IsAuthenticated.Value || !IsDirectedIdentity || (LocalIdentifier != null && ClaimedIdentifier != null));
			}
		}
		/// <summary>
		/// Get the URL to cancel this request.
		/// </summary>
		internal Uri CancelUrl {
			get {
				if (Immediate)
					throw new InvalidOperationException("Cancel is not an appropriate response to immediate mode requests.");

				UriBuilder builder = new UriBuilder(ReturnTo);
				var args = new Dictionary<string, string>();
				args.Add(Protocol.openid.mode, Protocol.Args.Mode.cancel);
				UriUtil.AppendQueryArgs(builder, args);

				return builder.Uri;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
		internal CheckIdRequest(OpenIdProvider provider) : base(provider) {
			// handle the mandatory protocol fields
			string mode = Util.GetRequiredArg(Query, Protocol.openid.mode);
			if (Protocol.Args.Mode.checkid_immediate.Equals(mode, StringComparison.Ordinal)) {
				Immediate = true;
			} else if (Protocol.Args.Mode.checkid_setup.Equals(mode, StringComparison.Ordinal)) {
				Immediate = false; // implied
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.InvalidOpenIdQueryParameterValue, Protocol.openid.mode, mode), Query);
			}

			// The spec says claimed_id and identity can both be either present or
			// absent.  But for now we don't have or support extensions that don't
			// use these parameters, so we require them.  In the future that may change.
			if (Protocol.Version.Major >= 2) {
				claimedIdentifier = Util.GetRequiredIdentifierArg(Query, Protocol.openid.claimed_id);
			}
			localIdentifier = Util.GetRequiredIdentifierArg(Query, Protocol.openid.identity);
			// The spec says return_to is optional, but what good is authenticating
			// a user if the user won't be sent back?
			ReturnTo = Util.GetRequiredUriArg(Query, Protocol.openid.return_to);
			Realm = Util.GetOptionalRealmArg(Query, Protocol.openid.Realm) ?? ReturnTo;
			AssociationHandle = Util.GetOptionalArg(Query, Protocol.openid.assoc_handle);

			if (!Realm.Contains(ReturnTo)) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.ReturnToNotUnderRealm, ReturnTo.AbsoluteUri, Realm), Query);
			}

			if (Protocol.Version.Major >= 2) {
				if (LocalIdentifier == ((Identifier)Protocol.ClaimedIdentifierForOPIdentifier) ^
					ClaimedIdentifier == ((Identifier)Protocol.ClaimedIdentifierForOPIdentifier)) {
					throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
						Strings.MatchingArgumentsExpected, Protocol.openid.claimed_id,
						Protocol.openid.identity, Protocol.ClaimedIdentifierForOPIdentifier),
						Query);
				}
			}

			if (ClaimedIdentifier == ((Identifier)Protocol.ClaimedIdentifierForOPIdentifier) &&
				Protocol.ClaimedIdentifierForOPIdentifier != null) {
				// Force the OP to deal with identifier_select by nulling out the two identifiers.
				IsDirectedIdentity = true;
				claimedIdentifier = null;
				localIdentifier = null;
			}
		}

		internal override IEncodable CreateResponse() {
			Debug.Assert(IsAuthenticated.HasValue, "This should be checked internally before CreateResponse is called.");
			return AssertionMessage.CreateAssertion(this);
		}

		/// <summary>
		/// Encode this request as a URL to GET.
		/// </summary>
		internal Uri SetupUrl {
			get {
				Debug.Assert(Provider.Endpoint != null, "The OpenIdProvider should have guaranteed this.");
				var q = new Dictionary<string, string>();

				q.Add(Protocol.openid.mode, Protocol.Args.Mode.checkid_setup);
				q.Add(Protocol.openid.identity, LocalIdentifier.ToString());
				q.Add(Protocol.openid.return_to, ReturnTo.AbsoluteUri);

				if (Realm != null)
					q.Add(Protocol.openid.Realm, Realm);

				if (this.AssociationHandle != null)
					q.Add(Protocol.openid.assoc_handle, this.AssociationHandle);

				UriBuilder builder = new UriBuilder(Provider.Endpoint);
				UriUtil.AppendQueryArgs(builder, q);

				return builder.Uri;
			}
		}

		public override string ToString() {
			string returnString = @"
CheckIdRequest.Immediate = '{0}'
CheckIdRequest.Realm = '{1}'
CheckIdRequest.Identity = '{2}' 
CheckIdRequest._mode = '{3}' 
CheckIdRequest.ReturnTo = '{4}' 
";

			return base.ToString() + string.Format(CultureInfo.CurrentCulture,
				returnString, Immediate, Realm, LocalIdentifier, Mode, ReturnTo);
		}
	}
}
