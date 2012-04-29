//-----------------------------------------------------------------------
// <copyright file="JwtSigningAlgorithm.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal abstract class JwtSigningAlgorithm : IDisposable {
		protected JwtSigningAlgorithm(string algorithmName) {
			Requires.NotNullOrEmpty(algorithmName, "algorithmName");
			this.Header = new JwsHeader(algorithmName);
		}

		internal JwsHeader Header { get; private set; }

		internal abstract byte[] Sign(byte[] securedInput);

		internal abstract bool Verify(byte[] securedInput, byte[] signature);

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
		}
	}
}
