//-----------------------------------------------------------------------
// <copyright file="AssociateSuccessfulResponseRelyingPartyContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Code contract for the <see cref="AssociateSuccessfulResponseRelyingParty"/> class.
	/// </summary>
	[ContractClassFor(typeof(IAssociateSuccessfulResponseRelyingParty))]
	internal abstract class IAssociateSuccessfulResponseRelyingPartyContract : IAssociateSuccessfulResponseRelyingParty {
		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <returns>
		/// The created association.
		/// </returns>
		Association IAssociateSuccessfulResponseRelyingParty.CreateAssociationAtRelyingParty(AssociateRequest request) {
			Contract.Requires<ArgumentNullException>(request != null);
			throw new NotImplementedException();
		}

		Messaging.MessageProtections Messaging.IProtocolMessage.RequiredProtection {
			get { throw new NotImplementedException(); }
		}

		Messaging.MessageTransport Messaging.IProtocolMessage.Transport {
			get { throw new NotImplementedException(); }
		}

		Version Messaging.IMessage.Version {
			get { throw new NotImplementedException(); }
		}

		IDictionary<string, string> Messaging.IMessage.ExtraData {
			get { throw new NotImplementedException(); }
		}

		void Messaging.IMessage.EnsureValidMessage() {
			throw new NotImplementedException();
		}
	}
}
