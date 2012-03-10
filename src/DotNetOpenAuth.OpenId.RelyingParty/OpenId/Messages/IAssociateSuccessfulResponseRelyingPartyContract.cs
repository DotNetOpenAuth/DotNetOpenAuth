//-----------------------------------------------------------------------
// <copyright file="IAssociateSuccessfulResponseRelyingPartyContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Code contract for the <see cref="IAssociateSuccessfulResponseRelyingParty"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IAssociateSuccessfulResponseRelyingParty))]
	internal abstract class IAssociateSuccessfulResponseRelyingPartyContract : IAssociateSuccessfulResponseRelyingParty {
		#region IProtocolMessage Members

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		Messaging.MessageProtections Messaging.IProtocolMessage.RequiredProtection {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		Messaging.MessageTransport Messaging.IProtocolMessage.Transport {
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region IMessage members

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		Version Messaging.IMessage.Version {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		IDictionary<string, string> Messaging.IMessage.ExtraData {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		void Messaging.IMessage.EnsureValidMessage() {
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <returns>
		/// The created association.
		/// </returns>
		Association IAssociateSuccessfulResponseRelyingParty.CreateAssociationAtRelyingParty(AssociateRequest request) {
			Requires.NotNull(request, "request");
			throw new NotImplementedException();
		}
	}
}
