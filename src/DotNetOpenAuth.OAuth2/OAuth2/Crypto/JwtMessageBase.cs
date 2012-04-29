namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class JwtMessageBase : IMessage {
		private static readonly Version version = new Version(1, 0);

		private readonly Dictionary<string, string> extraData = new Dictionary<string, string>();

		public Version Version {
			get { return version; }
		}

		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		public virtual void EnsureValidMessage() {
			// The JWT spec mandates that any unexpected data in the JWT header or claims set cause a rejection.
			ErrorUtilities.VerifyProtocol(this.ExtraData.Count == 0, "Unrecognized data in JWT access token with key '{0}'.  Token rejected.", this.ExtraData.First().Key);
		}
	}
}
