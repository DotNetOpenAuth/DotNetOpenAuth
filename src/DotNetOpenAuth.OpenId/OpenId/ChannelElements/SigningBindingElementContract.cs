//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Net.Security;
	using System.Web;
	using DotNetOpenAuth.Loggers;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Code contract for the <see cref="SigningBindingElement"/> class.
	/// </summary>
	[ContractClassFor(typeof(SigningBindingElement))]
	internal abstract class SigningBindingElementContract : SigningBindingElement {
		/// <summary>
		/// Verifies the signature by unrecognized handle.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="signedMessage">The signed message.</param>
		/// <param name="protectionsApplied">The protections applied.</param>
		/// <returns>
		/// The applied protections.
		/// </returns>
		protected override MessageProtections VerifySignatureByUnrecognizedHandle(IProtocolMessage message, ITamperResistantOpenIdMessage signedMessage, MessageProtections protectionsApplied) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the association to use to sign or verify a message.
		/// </summary>
		/// <param name="signedMessage">The message to sign or verify.</param>
		/// <returns>
		/// The association to use to sign or verify the message.
		/// </returns>
		protected override Association GetAssociation(ITamperResistantOpenIdMessage signedMessage) {
			Requires.NotNull(signedMessage, "signedMessage");
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a specific association referenced in a given message's association handle.
		/// </summary>
		/// <param name="signedMessage">The signed message whose association handle should be used to lookup the association to return.</param>
		/// <returns>
		/// The referenced association; or <c>null</c> if such an association cannot be found.
		/// </returns>
		protected override Association GetSpecificAssociation(ITamperResistantOpenIdMessage signedMessage) {
			Requires.NotNull(signedMessage, "signedMessage");
			throw new NotImplementedException();
		}
	}
}
