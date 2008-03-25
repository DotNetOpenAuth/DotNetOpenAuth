using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DotNetOpenId.Provider;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace DotNetOpenId {
	/// <summary>
	/// A message did not conform to the OpenID protocol, or 
	/// some other processing error occurred.
	/// </summary>
	[Serializable]
	public class OpenIdException : Exception, IEncodable {
		IDictionary<string, string> query;
		public Identifier Identifier { get; private set; }
		internal Protocol Protocol = Protocol.Default;
		Protocol IEncodable.Protocol { get { return this.Protocol; } }

		internal OpenIdException(string message, Identifier identifier, IDictionary<string, string> query, Exception innerException)
			: base(message, innerException) {
			this.query = query;
			Identifier = identifier;
			if (query != null) Protocol = Protocol.Detect(query);
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
		internal OpenIdException(string message, Exception innerException)
			: this(message, null, null, innerException) {
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
		internal OpenIdException(string message)
			: this(message, null, null, null) {
		}
		protected OpenIdException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
			query = (IDictionary<string, string>)info.GetValue("query", typeof(IDictionary<string, string>));
			Identifier = (Identifier)info.GetValue("Identifier", typeof(Identifier));
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
		internal OpenIdException() { }
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);
			info.AddValue("query", query, typeof(IDictionary<string, string>));
			info.AddValue("Identifier", Identifier, typeof(Identifier));
		}

		internal bool HasReturnTo {
			get {
				return query == null ? false : query.ContainsKey(Protocol.openid.return_to);
			}
		}

		#region IEncodable Members

		EncodingType IEncodable.EncodingType {
			get {
				if (HasReturnTo)
					return EncodingType.RedirectBrowserUrl;

				if (query != null) {
					string mode = Util.GetOptionalArg(query, Protocol.openid.mode);
					if (mode != null)
						if (mode != Protocol.Args.Mode.checkid_setup &&
							mode != Protocol.Args.Mode.checkid_immediate)
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
				q.Add(Protocol.openid.mode, Protocol.Args.Mode.error);
				q.Add(Protocol.openid.error, Message);
				return q;
			}
		}
		public Uri RedirectUrl {
			get {
				if (query == null)
					return null;
				return new Uri(Util.GetRequiredArg(query, Protocol.openid.return_to));
			}
		}

		#endregion

	}
}
