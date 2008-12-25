namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.ChannelElements;

	internal class PrivateSecretMemoryStore : IPrivateSecretStore {
		#region IPrivateSecretStore Members

		public byte[] PrivateSecret { get; set; }

		#endregion
	}
}
