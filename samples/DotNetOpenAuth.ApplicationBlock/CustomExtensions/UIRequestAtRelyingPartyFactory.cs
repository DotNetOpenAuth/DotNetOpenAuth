//-----------------------------------------------------------------------
// <copyright file="UIRequestAtRelyingPartyFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock.CustomExtensions {
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions.UI;

	/// <summary>
	/// An extension factory that allows the <see cref="UIRequest"/> extension to be received by the relying party.
	/// </summary>
	/// <remarks>
	/// Typically UIRequest is only received by the Provider.  But Google mirrors back this data to the relying party
	/// if our web user is already logged into Google.
	/// See the OpenIdRelyingPartyWebForms sample's DetectGoogleSession.aspx page for usage of this factory.
	/// </remarks>
	public class UIRequestAtRelyingPartyFactory : IOpenIdExtensionFactory {
		/// <summary>
		/// The Type URI for the UI extension.
		/// </summary>
		private const string UITypeUri = "http://specs.openid.net/extensions/ui/1.0";

		/// <summary>
		/// Allows UIRequest extensions to be received by the relying party.  Useful when Google mirrors back the request
		/// to indicate that a user is logged in.
		/// </summary>
		/// <param name="typeUri">The type URI of the extension.</param>
		/// <param name="data">The parameters associated specifically with this extension.</param>
		/// <param name="baseMessage">The OpenID message carrying this extension.</param>
		/// <param name="isProviderRole">A value indicating whether this extension is being received at the OpenID Provider.</param>
		/// <returns>
		/// An instance of <see cref="IOpenIdMessageExtension"/> if the factory recognizes
		/// the extension described in the input parameters; <c>null</c> otherwise.
		/// </returns>
		public DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage, bool isProviderRole) {
			if (typeUri == UITypeUri && !isProviderRole) {
				return new UIRequest();
			}

			return null;
		}
	}
}
