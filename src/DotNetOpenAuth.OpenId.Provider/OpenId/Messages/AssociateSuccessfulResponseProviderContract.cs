//-----------------------------------------------------------------------
// <copyright file="AssociateSuccessfulResponseProviderContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Code contract for the <see cref="AssociateSuccessfulResponseProvider"/> class.
	/// </summary>
	[ContractClassFor(typeof(IAssociateSuccessfulResponseProvider))]
	internal abstract class IAssociateSuccessfulResponseProviderContract : IAssociateSuccessfulResponseProvider {
		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <param name="associationStore">The Provider's association store.</param>
		/// <param name="securitySettings">The security settings of the Provider.</param>
		/// <returns>
		/// The created association.
		/// </returns>
		Association IAssociateSuccessfulResponseProvider.CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Requires.NotNull(request, "request");
			Requires.NotNull(associationStore, "associationStore");
			Requires.NotNull(securitySettings, "securitySettings");
			throw new NotImplementedException();
		}

		long IAssociateSuccessfulResponseProvider.ExpiresIn {
			get { throw new NotImplementedException();  }
			set { throw new NotImplementedException(); }
		}

		string IAssociateSuccessfulResponseProvider.AssociationHandle {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
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
