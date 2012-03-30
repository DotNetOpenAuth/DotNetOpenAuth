//-----------------------------------------------------------------------
// <copyright file="IAssociateSuccessfulResponseProviderContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Code contract for the <see cref="IAssociateSuccessfulResponseProvider"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IAssociateSuccessfulResponseProvider))]
	internal abstract class IAssociateSuccessfulResponseProviderContract : IAssociateSuccessfulResponseProvider {
		/// <summary>
		/// Gets or sets the expires in.
		/// </summary>
		/// <value>
		/// The expires in.
		/// </value>
		long IAssociateSuccessfulResponseProvider.ExpiresIn {
			get { throw new NotImplementedException();  }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets or sets the association handle.
		/// </summary>
		/// <value>
		/// The association handle.
		/// </value>
		string IAssociateSuccessfulResponseProvider.AssociationHandle {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

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

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		void Messaging.IMessage.EnsureValidMessage() {
			throw new NotImplementedException();
		}
	}
}
