using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to verify the validity of a previous response.
	/// </summary>
	internal class CheckAuthRequest : AssociatedRequest {
		string signature;
		IDictionary<string, string> signedFields;
		IList<string> signedKeyOrder;

		public CheckAuthRequest(OpenIdProvider provider)
			: base(provider) {
			AssociationHandle = Util.GetRequiredArg(Query, Protocol.openid.assoc_handle);
			signature = Util.GetRequiredArg(Query, Protocol.openid.sig);
			signedKeyOrder = Util.GetRequiredArg(Query, Protocol.openid.signed).Split(',');

			signedFields = new Dictionary<string, string>();
			Debug.Assert(!signedKeyOrder.Contains(Protocol.openidnp.mode), "openid.mode must not be included in signature because it necessarily changes in checkauth requests.");
			foreach (string key in signedKeyOrder) {
				signedFields.Add(key, Util.GetRequiredArg(Query, Protocol.openid.Prefix + key));
			}
		}

		public override bool IsResponseReady {
			// This type of request can always be responded to immediately.
			get { return true; }
		}

		/// <summary>
		/// Gets the string "check_authentication".
		/// </summary>
		internal override string Mode {
			get { return Protocol.Args.Mode.check_authentication; }
		}

		/// <summary>
		/// Respond to this request.
		/// </summary>
		internal EncodableResponse Answer() {
			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start processing Response for CheckAuthRequest");
			}
			EncodableResponse response = new EncodableResponse(this);

			bool validSignature = Provider.Signatory.Verify(AssociationHandle, signature, signedFields, signedKeyOrder);
			response.Fields[Protocol.openidnp.is_valid] = validSignature ?
				Protocol.Args.IsValid.True : Protocol.Args.IsValid.False;

			// By invalidating our dumb association, we make it impossible to
			// verify the same authentication again, making a response_nonce check
			// to protect against replay attacks unnecessary.
			Provider.Signatory.Invalidate(AssociationHandle, AssociationRelyingPartyType.Dumb);

			// The RP may be asking for confirmation that an association should
			// be invalidated.  If so, double-check and send a reply in our response.
			string invalidate_handle = Util.GetOptionalArg(Query, Protocol.openid.invalidate_handle);
			if (invalidate_handle != null) {
				Association assoc = Provider.Signatory.GetAssociation(invalidate_handle, AssociationRelyingPartyType.Smart);

				if (assoc == null) {
					if (TraceUtil.Switch.TraceWarning) {
						Trace.TraceWarning("No matching association found. Returning invalidate_handle. ");
					}
					response.Fields[Protocol.openidnp.invalidate_handle] = invalidate_handle;
				}
			}

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("End processing Response for CheckAuthRequest. CheckAuthRequest response successfully created. ");
				if (TraceUtil.Switch.TraceVerbose) {
					Trace.TraceInformation("Response follows: {0}", response);
				}
			}

			return response;
		}

		internal override IEncodable CreateResponse() {
			return Answer();
		}

		public override string ToString() {
			string returnString = @"
CheckAuthRequest._sig = '{0}'
CheckAuthRequest.AssocHandle = '{1}'";
			return base.ToString() + string.Format(CultureInfo.CurrentUICulture, 
				returnString, signature, AssociationHandle);
		}

	}
}
