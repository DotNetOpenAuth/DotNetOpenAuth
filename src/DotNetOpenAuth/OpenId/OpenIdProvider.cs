//-----------------------------------------------------------------------
// <copyright file="OpenIdProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public sealed class OpenIdProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		/// <param name="associationStore">The association store to use.  Cannot be null.</param>
		public OpenIdProvider(IAssociationStore<AssociationRelyingPartyType> associationStore) {
			ErrorUtilities.VerifyArgumentNotNull(associationStore, "associationStore");

			this.Channel = new OpenIdChannel();
			this.AssociationStore = associationStore;
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets the association store.
		/// </summary>
		internal IAssociationStore<AssociationRelyingPartyType> AssociationStore { get; private set; }

		/// <summary>
		/// Responds automatically to the incoming message.
		/// </summary>
		/// <remarks>
		/// The design of a method like this is flawed... but it helps us get tests going for now.
		/// </remarks>
		internal void AutoRespond() {
			var request = this.Channel.ReadFromRequest();

			var associateRequest = request as AssociateRequest;
			if (associateRequest != null) {
				var associateDiffieHellmanRequest = request as AssociateDiffieHellmanRequest;
				var associateUnencryptedRequest = request as AssociateUnencryptedRequest;

				IProtocolMessage response = associateRequest.CreateResponse(this.AssociationStore);
				this.Channel.Send(response);
			} else {
				// TODO: code here
				throw new NotImplementedException();
			}
		}
	}
}
