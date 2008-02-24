using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
using DotNetOpenId.Store;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to verify the validity of a previous response.
	/// </summary>
	internal class CheckAuthRequest : AssociatedRequest {
		string signature;
		IDictionary<string, string> signedFields;
		string invalidate_handle;

		public CheckAuthRequest(Provider server, NameValueCollection query)
			: base(server) {
			AssociationHandle = getRequiredField(query, QueryStringArgs.openid.assoc_handle);
			signature = getRequiredField(query, QueryStringArgs.openid.sig);
			string[] signedList = getRequiredField(query, QueryStringArgs.openid.signed).Split(',');
			invalidate_handle = query[QueryStringArgs.openid.invalidate_handle];

			signedFields = new Dictionary<string, string>();

			foreach (string key in signedList) {
				string value = (key == QueryStringArgs.openidnp.mode) ?
					QueryStringArgs.Modes.id_res : getRequiredField(query, QueryStringArgs.openid.Prefix + key);
				signedFields.Add(key, value);
			}
		}

		public override RequestType RequestType {
			get { return RequestType.CheckAuthRequest; }
		}

		/// <summary>
		/// Gets the string "check_authentication".
		/// </summary>
		internal override string Mode {
			get { return QueryStringArgs.Modes.check_authentication; }
		}

		/// <summary>
		/// Respond to this request.
		/// </summary>
		internal IEncodable Answer() {
			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start processing Response for CheckAuthRequest");
			}

			bool is_valid = Server.Signatory.Verify(AssociationHandle, signature, signedFields);

			Server.Signatory.Invalidate(AssociationHandle, AssociationConsumerType.Dumb);

			Response response = new Response(this);

			response.Fields[QueryStringArgs.openidnp.is_valid] = (is_valid ? "true" : "false");

			if (!string.IsNullOrEmpty(invalidate_handle)) {
				Association assoc = Server.Signatory.GetAssociation(invalidate_handle, AssociationConsumerType.Smart);

				if (assoc == null) {
					if (TraceUtil.Switch.TraceWarning) {
						Trace.TraceWarning("No matching association found. Returning invalidate_handle. ");
					}

					response.Fields[QueryStringArgs.openidnp.invalidate_handle] = invalidate_handle;
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

		string getRequiredField(NameValueCollection query, string key) {
			string val = query[key];

			if (val == null)
				throw new OpenIdException(Mode + " request missing required parameter " + key, query);

			return val;
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
