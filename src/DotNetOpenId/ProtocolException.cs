using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DotNetOpenId.Provider;

namespace DotNetOpenId {
	/// <summary>
	/// A message did not conform to the OpenID protocol.
	/// </summary>
	public class ProtocolException : Exception, IEncodable {
		NameValueCollection query;
		public Uri IdentityUrl { get; private set; }

		internal ProtocolException(string message, Uri identityUrl, NameValueCollection query, Exception innerException)
			: base(message, innerException) {
			this.query = query;
			IdentityUrl = identityUrl;
		}
		internal ProtocolException(string message, Uri identityUrl, NameValueCollection query)
			: this(message, identityUrl, query, null) {
		}
		internal ProtocolException(string message, Uri identityUrl, Exception innerException)
			: this(message, identityUrl, null, innerException) {
		}
		internal ProtocolException(string message, Uri identityUrl)
			: this(message, identityUrl, null, null) {
		}
		internal ProtocolException(string message, NameValueCollection query)
			: this(message, null, query, null) {
		}
		internal ProtocolException(string message)
			: this(message, null, null, null) {
		}

		internal bool HasReturnTo {
			get {
				return query == null ? false : (query[QueryStringArgs.openid.return_to] != null);
			}
		}

		#region IEncodable Members

		EncodingType IEncodable.EncodingType {
			get {
				if (HasReturnTo)
					return EncodingType.RedirectBrowserUrl;

				if (query != null) {
					string mode = query.Get(QueryStringArgs.openid.mode);
					if (mode != null)
						if (mode != QueryStringArgs.Modes.checkid_setup &&
							mode != QueryStringArgs.Modes.checkid_immediate)
							return EncodingType.ResponseBody;
				}

				// Notes from the original port
				//# According to the OpenID spec as of this writing, we are
				//# probably supposed to switch on request type here (GET
				//# versus POST) to figure out if we're supposed to print
				//# machine-readable or human-readable content at this
				//# point.  GET/POST seems like a pretty lousy way of making
				//# the distinction though, as it's just as possible that
				//# the user agent could have mistakenly been directed to
				//# post to the server URL.

				//# Basically, if your request was so broken that you didn't
				//# manage to include an openid.mode, I'm not going to worry
				//# too much about returning you something you can't parse.
				return EncodingType.None;
			}
		}

		public IDictionary<string, string> EncodedFields {
			get {
				var q = new Dictionary<string, string>();
				q.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.error);
				q.Add(QueryStringArgs.openid.error, Message);
				return q;
			}
		}
		public Uri BaseUri {
			get {
				if (query == null)
					return null;
				string return_to = query.Get(QueryStringArgs.openid.return_to);
				if (return_to == null)
					throw new InvalidOperationException("return_to URL has not been set.");
				return new Uri(return_to);
			}
		}

		#endregion

	}
}
