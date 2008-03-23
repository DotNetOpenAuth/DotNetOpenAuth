using System;
using System.Collections.Specialized;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to establish an association.
	/// </summary>
	internal class AssociateRequest : Request {
		string associationKeyType;
		ProviderSession session;

		public AssociateRequest(OpenIdProvider server)
			: base(server) {
			session = ProviderSession.CreateSession(Query);
			associationKeyType = Util.GetRequiredArg(Query, Protocol.Constants.openidnp.assoc_type);
		}

		public override bool IsResponseReady {
			// This type of request can always be responded to immediately.
			get { return true; }
		}

		/// <summary>
		/// Returns the string "associate".
		/// </summary>
		internal override string Mode {
			get { return Protocol.Constants.Modes.associate; }
		}

		/// <summary>
		/// Respond to this request with an association.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		public EncodableResponse Answer() {
			Association assoc = Server.Signatory.CreateAssociation(AssociationRelyingPartyType.Smart);
			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start processing response for AssociateRequest");
				if (TraceUtil.Switch.TraceVerbose) {
					Trace.TraceInformation("Association to be sent: {0}", assoc);
				}
			}

			EncodableResponse response = new EncodableResponse(this);

			response.Fields[Protocol.Constants.openidnp.expires_in] = assoc.SecondsTillExpiration.ToString(CultureInfo.InvariantCulture);
			response.Fields[Protocol.Constants.openidnp.assoc_type] = assoc.AssociationType;
			response.Fields[Protocol.Constants.openidnp.assoc_handle] = assoc.Handle;

			IDictionary<string, string> nvc = session.Answer(assoc.SecretKey);
			foreach (var pair in nvc) {
				response.Fields[pair.Key] = nvc[pair.Key];
			}

			if (session.SessionType != Protocol.Constants.SessionType.NoEncryption11) {
				response.Fields[Protocol.Constants.openidnp.session_type] = session.SessionType;
			}

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("End processing response for AssociateRequest. AssociateRequest response successfully created. ");
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
			string returnString = "AssociateRequest._assoc_type = {0}";
			return base.ToString() + Environment.NewLine + String.Format(CultureInfo.CurrentUICulture,
				returnString, associationKeyType);
		}

	}
}
