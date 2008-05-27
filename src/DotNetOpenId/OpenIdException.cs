using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DotNetOpenId.Provider;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Diagnostics;

namespace DotNetOpenId {
	/// <summary>
	/// A message did not conform to the OpenID protocol, or 
	/// some other processing error occurred.
	/// </summary>
	[Serializable]
	public class OpenIdException : Exception, IEncodable {
		internal IDictionary<string, string> Query;
		/// <summary>
		/// An Identifier (claimed or local provider) that was being processed when
		/// the exception was thrown.
		/// </summary>
		public Identifier Identifier { get; private set; }
		internal Protocol Protocol = Protocol.Default;
		Protocol IEncodable.Protocol { get { return this.Protocol; } }
		internal IDictionary<string, string> ExtraArgsToReturn;

		internal OpenIdException(string message, Identifier identifier, IDictionary<string, string> query, Exception innerException)
			: base(message, innerException) {
			this.Query = query;
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
		/// <summary>
		/// Instantiates an <see cref="OpenIdException"/> based on deserialized data.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected OpenIdException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
			Query = (IDictionary<string, string>)info.GetValue("query", typeof(IDictionary<string, string>));
			Identifier = (Identifier)info.GetValue("Identifier", typeof(Identifier));
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
		internal OpenIdException() { }
		/// <summary>
		/// Serializes the exception details for binary transmission.
		/// </summary>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);
			info.AddValue("query", Query, typeof(IDictionary<string, string>));
			info.AddValue("Identifier", Identifier, typeof(Identifier));
		}

		internal bool HasReturnTo {
			get {
				return Query == null ? false : Query.ContainsKey(Protocol.openid.return_to);
			}
		}

		#region IEncodable Members

		EncodingType IEncodable.EncodingType {
			get {
				Debug.Assert(Query != null, "An OpenId exception should always be provided with the query if it is to be encoded for transmittal to the RP.");
				if (HasReturnTo)
					return EncodingType.IndirectMessage;

				if (Query != null) {
					string mode = Util.GetOptionalArg(Query, Protocol.openid.mode);
					if (mode != null)
						if (mode != Protocol.Args.Mode.checkid_setup &&
							mode != Protocol.Args.Mode.checkid_immediate)
							return EncodingType.DirectResponse;
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

		/// <summary>
		/// Fields that should be encoded for processing when this exception 
		/// is thrown by a Provider and the details should be passed to the
		/// relying party.
		/// </summary>
		public IDictionary<string, string> EncodedFields {
			get {
				var q = new Dictionary<string, string>();
				q.Add(Protocol.openid.mode, Protocol.Args.Mode.error);
				q.Add(Protocol.openid.error, Message);
				if (ExtraArgsToReturn != null) {
					foreach (var pair in ExtraArgsToReturn) {
						q.Add(pair.Key, pair.Value);
					}
				}
				return q;
			}
		}
		/// <summary>
		/// The URL that the exception details should be forwarded to.
		/// This is used when a Provider throws an exception that a relying
		/// party may find helpful in diagnosing the failure.
		/// </summary>
		public Uri RedirectUrl {
			get {
				if (Query == null)
					return null;
				return new Uri(Util.GetRequiredArg(Query, Protocol.openid.return_to));
			}
		}

		#endregion

	}
}
