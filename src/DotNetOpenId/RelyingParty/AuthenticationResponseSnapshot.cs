using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	[Serializable]
	class AuthenticationResponseSnapshot : IAuthenticationResponse {
		internal AuthenticationResponseSnapshot(IAuthenticationResponse copyFrom) {
			if (copyFrom == null) throw new ArgumentNullException("copyFrom");

			ClaimedIdentifier = copyFrom.ClaimedIdentifier;
			FriendlyIdentifierForDisplay = copyFrom.FriendlyIdentifierForDisplay;
			Status = copyFrom.Status;
		}

		#region IAuthenticationResponse Members

		public T GetExtension<T>() where T : DotNetOpenId.Extensions.IExtensionResponse, new() {
			throw new NotSupportedException(Strings.NotSupportedByAuthenticationSnapshot);
		}

		public DotNetOpenId.Extensions.IExtensionResponse GetExtension(Type extensionType) {
			throw new NotSupportedException(Strings.NotSupportedByAuthenticationSnapshot);
		}

		public Identifier ClaimedIdentifier { get; private set; }

		public string FriendlyIdentifierForDisplay { get; private set; }

		public AuthenticationStatus Status { get; private set; }

		public Exception Exception {
			get { throw new NotSupportedException(Strings.NotSupportedByAuthenticationSnapshot); }
		}

		#endregion
	}
}
