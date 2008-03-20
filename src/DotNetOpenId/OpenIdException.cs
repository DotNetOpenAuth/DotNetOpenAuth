using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DotNetOpenId.Provider;
using System.Runtime.Serialization;

namespace DotNetOpenId {
	/// <summary>
	/// A message did not conform to the OpenID protocol, or 
	/// some other processing error occurred.
	/// </summary>
	[Serializable]
	public class OpenIdException : Exception, IEncodable {
		IDictionary<string, string> query;
		public Identifier Identifier { get; private set; }

		internal OpenIdException(string message, Identifier identifier, IDictionary<string, string> query, Exception innerException)
			: base(message, innerException) {
			this.query = query;
			Identifier = identifier;
		}
		internal OpenIdException(string message, Identifier identifier, IDictionary<string, string> query)
			: this(message, identifier, query, null) {
		}
		internal OpenIdException(string message, Identifier identifier, Exception innerException)
			: this(message, identifier, null, innerException) {
		}
		internal OpenIdException(string message, Identifier identifier)
			: this(message, identifier, null, null) {
		}
		internal OpenIdException(string message, IDictionary<string, string> query)
			: this(message, null, query, null) {
		}
		internal OpenIdException(string message, Exception innerException)
			: this(message, null, null, innerException) {
		}
		internal OpenIdException(string message)
			: this(message, null, null, null) {
		}
		protected OpenIdException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
		internal OpenIdException() { }

		internal bool HasReturnTo {
			get {
				return query == null ? false : query.ContainsKey(QueryStringArgs.openid.return_to);
			}
		}

		#region IEncodable Members

		EncodingType IEncodable.EncodingType {
			get {
				if (HasReturnTo)
					return EncodingType.RedirectBrowserUrl;

				if (query != null) {
					string mode = Util.GetOptionalArg(query, QueryStringArgs.openid.mode);
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
		public Uri RedirectUrl {
			get {
				if (query == null)
					return null;
				return new Uri(Util.GetRequiredArg(query, QueryStringArgs.openid.return_to));
			}
		}

		#endregion

	}
}
