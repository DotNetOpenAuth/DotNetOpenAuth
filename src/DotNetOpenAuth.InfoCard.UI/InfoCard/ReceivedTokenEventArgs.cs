//-----------------------------------------------------------------------
// <copyright file="ReceivedTokenEventArgs.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Xml.XPath;

	/// <summary>
	/// Arguments for the <see cref="InfoCardSelector.ReceivedToken"/> event.
	/// </summary>
	public class ReceivedTokenEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReceivedTokenEventArgs"/> class.
		/// </summary>
		/// <param name="token">The token.</param>
		internal ReceivedTokenEventArgs(Token token) {
			this.Token = token;
		}

		/// <summary>
		/// Gets the processed token.
		/// </summary>
		public Token Token { get; private set; }

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Token != null);
		}
#endif
	}
}
