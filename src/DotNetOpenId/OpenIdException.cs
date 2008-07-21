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

			if (query != null) {
				Logger.ErrorFormat("OpenIdException: {0}{1}{2}", message, Environment.NewLine, Util.ToString(query));
			} else {
				Logger.ErrorFormat("OpenIdException: {0}", message);
			}
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

		/// <summary>
		/// Gets whether this exception was generated on an OP as the result of processing a message
		/// that came directly from the RP.  
		/// </summary>
		/// <remarks>
		/// This is useful because it allows us to determine what kind of error reporting we'll send
		/// in the HTTP response.
		/// </remarks>
		private bool IsDirectMessage {
			get {
				Debug.Assert(Query != null, "An OpenId exception should always be provided with the query if it is to be encoded for transmittal to the RP.");

				if (Query != null) {
					string mode = Util.GetOptionalArg(Query, Protocol.openid.mode);
					if (mode != null) {
						return mode == Protocol.Args.Mode.associate ||
							mode == Protocol.Args.Mode.check_authentication;
					}
				}

				// Unable to figure it out, so we'll default to indirect message.
				return false;
			}
		}

		EncodingType IEncodable.EncodingType {
			get {
				if (IsDirectMessage)
					return EncodingType.ResponseBody;

				if (HasReturnTo)
					return EncodingType.RedirectBrowserUrl;

				Debug.Fail("Somehow we cannot tell whether this is a direct message or indirect message.  Did we construct an exception without a Query parameter?");
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
				if (IsDirectMessage) {
					q.Add(Protocol.openidnp.mode, Protocol.Args.Mode.error);
					q.Add(Protocol.openidnp.error, Message);
				} else {
					q.Add(Protocol.openid.mode, Protocol.Args.Mode.error);
					q.Add(Protocol.openid.error, Message);
				}
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
