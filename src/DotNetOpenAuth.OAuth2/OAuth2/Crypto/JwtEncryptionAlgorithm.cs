//-----------------------------------------------------------------------
// <copyright file="JwtEncryptionAlgorithm.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal abstract class JwtEncryptionAlgorithm : IDisposable {
		protected JwtEncryptionAlgorithm(string algorithmName, string encryptionMethod) {
			Requires.NotNullOrEmpty(algorithmName, "algorithmName");
			Requires.NotNullOrEmpty(encryptionMethod, "encryptionMethod");
			this.Header = new JweHeader(algorithmName, encryptionMethod);
		}

		internal JweHeader Header { get; private set; }

		internal abstract void Encrypt(byte[] plainText, out byte[] cipherText, out byte[] integrityValue);

		internal abstract byte[] Decrypt(byte[] cipherText, byte[] integrityValue);

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
		}

		protected void KeyDerivation(byte[] contentMasterKey, out byte[] contentEncryptionKey, out byte[] contentIntegrityKey) {
			// Implementing this would be manual, or involve P/Invoke I think.
			// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375393(v=vs.85).aspx
			throw new NotImplementedException();
		}
	}
}
