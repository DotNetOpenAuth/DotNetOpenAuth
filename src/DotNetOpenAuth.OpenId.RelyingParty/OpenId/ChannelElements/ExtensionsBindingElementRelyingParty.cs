//-----------------------------------------------------------------------
// <copyright file="ExtensionsBindingElementRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// The OpenID binding element responsible for reading/writing OpenID extensions
	/// at the Relying Party.
	/// </summary>
	internal class ExtensionsBindingElementRelyingParty : ExtensionsBindingElement {
		/// <summary>
		/// The security settings that apply to this relying party, if it is a relying party.
		/// </summary>
		private readonly RelyingPartySecuritySettings relyingPartySecuritySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionsBindingElementRelyingParty"/> class.
		/// </summary>
		/// <param name="extensionFactory">The extension factory.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal ExtensionsBindingElementRelyingParty(IOpenIdExtensionFactory extensionFactory, RelyingPartySecuritySettings securitySettings)
			: base(extensionFactory, securitySettings, !securitySettings.IgnoreUnsignedExtensions) {
			Requires.NotNull(extensionFactory, "extensionFactory");
			Requires.NotNull(securitySettings, "securitySettings");

			this.relyingPartySecuritySettings = securitySettings;
		}
	}
}
