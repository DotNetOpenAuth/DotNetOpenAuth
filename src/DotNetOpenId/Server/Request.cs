using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace DotNetOpenId.Server
{
	public enum RequestType {
		CheckIdRequest,
		CheckAuthRequest,
		AssociateRequest,
	}

	public abstract class Request {

		internal abstract string Mode { get; }
		public abstract RequestType RequestType { get; }

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return String.Format(returnString, Mode);
		}

		/// <summary>
		/// Decodes an incoming web request in to a <see cref="Request"/>.
		/// </summary>
		/// <param name="query">The query parameters as a dictionary with each key mapping to one value. </param>
		internal static Request GetRequestFromQuery(NameValueCollection query) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("Start message decoding");
			}

			if (query == null) return null;

			NameValueCollection myquery = new NameValueCollection();
			foreach (string key in query) {
				if (!String.IsNullOrEmpty(key)) {
					if (key.StartsWith(QueryStringArgs.openid.Prefix)) { myquery[key] = query[key]; }
				}
			}

			if (myquery.Count == 0) return null;

			string mode = myquery.Get(QueryStringArgs.openid.mode);
			if (mode == null)
				throw new ProtocolException(query, "No openid.mode value in query");

			if (mode == QueryStringArgs.Modes.checkid_setup) {
				CheckIdRequest request = new CheckIdRequest(query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckIdRequest in setup mode");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("CheckIdRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			} else if (mode == QueryStringArgs.Modes.checkid_immediate) {
				CheckIdRequest request = new CheckIdRequest(query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckIdRequest in immediate mode");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("CheckIdRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			} else if (mode == QueryStringArgs.Modes.check_authentication) {
				CheckAuthRequest request = new CheckAuthRequest(query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckAuthRequest");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("CheckAuthRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			} else if (mode == QueryStringArgs.Modes.associate) {
				AssociateRequest request = new AssociateRequest(query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new AssociateRequest ");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("AssociateRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			}

			throw new ProtocolException(query, "No decoder for openid.mode=" + mode);

		}
	}
}
