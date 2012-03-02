//-----------------------------------------------------------------------
// <copyright file="AcmeResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock.CustomExtensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	[Serializable]
	public class AcmeResponse : IOpenIdMessageExtension {
		private IDictionary<string, string> extraData = new Dictionary<string, string>();

		[MessagePart]
		public string FavoriteIceCream { get; set; }

		#region IOpenIdMessageExtension Members

		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		public string TypeUri {
			get { return Acme.CustomExtensionTypeUri; }
		}

		/// <summary>
		/// Gets the additional TypeURIs that are supported by this extension, in preferred order.
		/// May be empty if none other than <see cref="TypeUri"/> is supported, but
		/// should not be null.
		/// </summary>
		/// <remarks>
		/// Useful for reading in messages with an older version of an extension.
		/// The value in the <see cref="TypeUri"/> property is always checked before
		/// trying this list.
		/// If you do support multiple versions of an extension using this method,
		/// consider adding a CreateResponse method to your request extension class
		/// so that the response can have the context it needs to remain compatible
		/// given the version of the extension in the request message.
		/// The <see cref="Extensions.SimpleRegistration.ClaimsRequest.CreateResponse"/> for an example.
		/// </remarks>
		public IEnumerable<string> AdditionalSupportedTypeUris {
			get { return Enumerable.Empty<string>(); }
		}

		public bool IsSignedByRemoteParty { get; set; }

		#endregion

		#region IMessage Members

		public Version Version {
			get { return Acme.Version; }
		}

		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		public void EnsureValidMessage() {
		}

		#endregion
	}
}
