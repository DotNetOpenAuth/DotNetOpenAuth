//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementBaseContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Code Contract for the <see cref="SigningBindingElementBase"/> class.
	/// </summary>
	[ContractClassFor(typeof(SigningBindingElementBase))]
	internal abstract class SigningBindingElementBaseContract : SigningBindingElementBase {
		/// <summary>
		/// Prevents a default instance of the SigningBindingElementBaseContract class from being created.
		/// </summary>
		private SigningBindingElementBaseContract()
			: base(string.Empty) {
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A new instance of the binding element.</returns>
		/// <remarks>
		/// Implementations of this method need not clone the SignatureVerificationCallback member, as the
		/// <see cref="SigningBindingElementBase"/> class does this.
		/// </remarks>
		protected override ITamperProtectionChannelBindingElement Clone() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Calculates a signature for a given message.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <returns>The signature for the message.</returns>
		protected override string GetSignature(ITamperResistantOAuthMessage message) {
			Requires.NotNull(message, "message");
			Requires.ValidState(this.Channel != null);
			throw new NotImplementedException();
		}
	}
}
