using System;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.Store;
using System.Diagnostics;
using System.Collections.Generic;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to establish an association.
	/// </summary>
	internal class AssociateRequest : Request {
		string associationKeyType = QueryStringArgs.HMAC_SHA1;
		ProviderSession session;

		public AssociateRequest(Provider server, NameValueCollection query)
			: base(server) {
			session = ProviderSession.CreateSession(query);
		}

		public override RequestType RequestType {
			get { return RequestType.AssociateRequest; }
		}

		/// <summary>
		/// Returns the string "associate".
		/// </summary>
		internal override string Mode {
			get { return QueryStringArgs.Modes.associate; }
		}

		/// <summary>
		/// Respond to this request with an association.
		/// </summary>
		public Response Answer() {
			Association assoc = Server.Signatory.CreateAssociation(AssociationConsumerType.Smart);
			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start processing response for AssociateRequest");
				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ProviderTrace("Association to be sent follows:");
					TraceUtil.ProviderTrace(assoc.ToString());
				}
			}
			#endregion

			Response response = new Response(this);

			response.Fields[QueryStringArgs.openidnp.expires_in] = assoc.SecondsTillExpiration.ToString();
			response.Fields[QueryStringArgs.openidnp.assoc_type] = assoc.AssociationType;
			response.Fields[QueryStringArgs.openidnp.assoc_handle] = assoc.Handle;

			IDictionary<string, string> nvc = session.Answer(assoc.SecretKey);
			foreach (var pair in nvc) {
				response.Fields[pair.Key] = nvc[pair.Key];
			}

			if (session.SessionType != QueryStringArgs.plaintext) {
				response.Fields[QueryStringArgs.openidnp.session_type] = session.SessionType;
			}

			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ProviderTrace("End processing response for AssociateRequest. AssociateRequest response successfully created. ");
				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ProviderTrace("Response follows. ");
					TraceUtil.ProviderTrace(response.ToString());
				}
			}
			#endregion

			return response;
		}

		public override string ToString() {
			string returnString = "AssociateRequest._assoc_type = {0}";
			return base.ToString() + Environment.NewLine + String.Format(returnString, associationKeyType);
		}

	}
}
