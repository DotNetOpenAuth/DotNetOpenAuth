using System;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.Store;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to establish an association.
	/// </summary>
	internal class AssociateRequest : Request {
		string associationKeyType = QueryStringArgs.HMAC_SHA1;
		ServerSession session;

		public AssociateRequest(Server server, NameValueCollection query)
			: base(server) {
			string session_type = query.Get(QueryStringArgs.openid.session_type);

			switch (session_type) {
				case null:
					session = new PlainTextServerSession();
					break;
				case QueryStringArgs.DH_SHA1:
					session = new DiffieHellmanServerSession(query);
					break;
				default:
					throw new ProtocolException(query, "Unknown session type " + session_type);
			}
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
				TraceUtil.ServerTrace("Start processing response for AssociateRequest");
				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ServerTrace("Association to be sent follows:");
					TraceUtil.ServerTrace(assoc.ToString());
				}
			}
			#endregion

			Response response = new Response(this);

			response.Fields[QueryStringArgs.openidnp.expires_in] = assoc.SecondsTillExpiration.ToString();
			response.Fields[QueryStringArgs.openidnp.assoc_type] = assoc.AssociationType;
			response.Fields[QueryStringArgs.openidnp.assoc_handle] = assoc.Handle;

			NameValueCollection nvc = session.Answer(assoc.SecretKey);
			foreach (string key in nvc) {
				response.Fields[key] = nvc[key];
			}

			if (session.SessionType != "plaintext") {
				response.Fields[QueryStringArgs.openidnp.session_type] = session.SessionType;
			}

			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("End processing response for AssociateRequest. AssociateRequest response successfully created. ");
				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ServerTrace("Response follows. ");
					TraceUtil.ServerTrace(response.ToString());
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
