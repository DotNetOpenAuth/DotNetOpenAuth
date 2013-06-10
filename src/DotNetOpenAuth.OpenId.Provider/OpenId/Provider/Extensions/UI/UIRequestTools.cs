//-----------------------------------------------------------------------
// <copyright file="UIRequestTools.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider.Extensions.UI {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.UI;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.Xrds;
	using Validation;

	/// <summary>
	/// OpenID User Interface extension 1.0 request message.
	/// </summary>
	/// <remarks>
	/// 	<para>Implements the extension described by: http://wiki.openid.net/f/openid_ui_extension_draft01.html </para>
	/// 	<para>This extension only applies to checkid_setup requests, since checkid_immediate requests display
	/// no UI to the user. </para>
	/// 	<para>For rules about how the popup window should be displayed, please see the documentation of
	/// <see cref="UIModes.Popup"/>. </para>
	/// 	<para>An RP may determine whether an arbitrary OP supports this extension (and thereby determine
	/// whether to use a standard full window redirect or a popup) via the
	/// <see cref="IdentifierDiscoveryResult.IsExtensionSupported&lt;T&gt;()"/> method.</para>
	/// </remarks>
	public static class UIRequestTools {
		/// <summary>
		/// Gets the URL of the RP icon for the OP to display.
		/// </summary>
		/// <param name="realm">The realm of the RP where the authentication request originated.</param>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of the RP's icons it has available for the Provider to display, in decreasing preferred order.
		/// </returns>
		/// <value>The icon URL.</value>
		/// <remarks>
		/// This property is automatically set for the OP with the result of RP discovery.
		/// RPs should set this value by including an entry such as this in their XRDS document.
		/// <example>
		/// &lt;Service xmlns="xri://$xrd*($v*2.0)"&gt;
		/// &lt;Type&gt;http://specs.openid.net/extensions/ui/icon&lt;/Type&gt;
		/// &lt;URI&gt;http://consumer.example.com/images/image.jpg&lt;/URI&gt;
		/// &lt;/Service&gt;
		/// </example>
		/// </remarks>
		public static async Task<IEnumerable<Uri>> GetRelyingPartyIconUrlsAsync(Realm realm, IHostFactories hostFactories, CancellationToken cancellationToken) {
			Requires.NotNull(realm, "realm");
			Requires.NotNull(hostFactories, "hostFactories");

			XrdsDocument xrds = await realm.DiscoverAsync(hostFactories, false, cancellationToken);
			if (xrds == null) {
				return Enumerable.Empty<Uri>();
			} else {
				return xrds.FindRelyingPartyIcons();
			}
		}
	}
}
