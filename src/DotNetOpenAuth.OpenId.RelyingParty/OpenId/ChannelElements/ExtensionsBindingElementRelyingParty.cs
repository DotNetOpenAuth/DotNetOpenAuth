//-----------------------------------------------------------------------
// <copyright file="ExtensionsBindingElementRelyingParty.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OpenId.RelyingParty;

	internal class ExtensionsBindingElementRelyingParty : ExtensionsBindingElement {
		/// <summary>
		/// The security settings that apply to this relying party, if it is a relying party.
		/// </summary>
		private readonly RelyingPartySecuritySettings relyingPartySecuritySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionsBindingElement"/> class.
		/// </summary>
		/// <param name="extensionFactory">The extension factory.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal ExtensionsBindingElementRelyingParty(IOpenIdExtensionFactory extensionFactory, RelyingPartySecuritySettings securitySettings)
			: base(extensionFactory, securitySettings, !securitySettings.IgnoreUnsignedExtensions) {
			Contract.Requires<ArgumentNullException>(extensionFactory != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);

			this.relyingPartySecuritySettings = securitySettings;
		}
	}
}
