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
		string invalidate_handle;

		public CheckAuthRequest(OpenIdProvider server)
			: base(server) {
			AssociationHandle = Util.GetRequiredArg(Query, Protocol.openid.assoc_handle);
			signature = Util.GetRequiredArg(Query, Protocol.openid.sig);
			signedKeyOrder = Util.GetRequiredArg(Query, Protocol.openid.signed).Split(',');
			invalidate_handle = Util.GetOptionalArg(Query, Protocol.openid.invalidate_handle);

			signedFields = new Dictionary<string, string>();

			foreach (string key in signedKeyOrder) {
				string value = (key == Protocol.openidnp.mode) ?
					Protocol.Args.Mode.id_res : Util.GetRequiredArg(Query, Protocol.openid.Prefix + key);
				signedFields.Add(key, value);
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

			bool is_valid = Provider.Signatory.Verify(AssociationHandle, signature, signedFields, signedKeyOrder);

			Provider.Signatory.Invalidate(AssociationHandle, AssociationRelyingPartyType.Dumb);

			EncodableResponse response = new EncodableResponse(this);

			response.Fields[Protocol.openidnp.is_valid] = (is_valid ? "true" : "false");

			if (!string.IsNullOrEmpty(invalidate_handle)) {
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
CheckAuthRequest.AssocHandle = '{1}'
CheckAuthRequest._invalidate_handle = '{2}' ";
			return base.ToString() + string.Format(CultureInfo.CurrentUICulture, 
				returnString, signature, AssociationHandle, invalidate_handle);
		}

	}
}
