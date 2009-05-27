//-----------------------------------------------------------------------
// <copyright file="UIUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.UI {
	using System;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Constants used in implementing support for the UI extension.
	/// </summary>
	public static class UIUtilities {
		/// <summary>
		/// The required width of the popup window the relying party creates for the provider.
		/// </summary>
		public const int PopupWidth = 450;

		/// <summary>
		/// The required height of the popup window the relying party creates for the provider.
		/// </summary>
		public const int PopupHeight = 500;

		/// <summary>
		/// Gets the <c>window.open</c> javascript snippet to use to open a popup window
		/// compliant with the UI extension.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="request">The authentication request to place in the window.</param>
		/// <param name="windowName">The name to assign to the popup window.</param>
		/// <returns>A string starting with 'window.open' and forming just that one method call.</returns>
		internal static string GetWindowPopupScript(OpenIdRelyingParty relyingParty, IAuthenticationRequest request, string windowName) {
			Contract.Requires(relyingParty != null);
			Contract.Requires(request != null);
			Contract.Requires(!string.IsNullOrEmpty(windowName));

			Uri popupUrl = request.RedirectingResponse.GetDirectUriRequest(relyingParty.Channel);

			return string.Format(
				CultureInfo.InvariantCulture,
				"(window.showModalDialog ? window.showModalDialog({0}, {1}, 'status:0;resizable:1;scroll:1;center:1;dialogWidth:{2}px; dialogHeight:{3}') : window.open({0}, {1}, 'status=0,toolbar=0,location=1,resizable=1,scrollbars=1,left=' + ((screen.width - {2}) / 2) + ',top=' + ((screen.height - {3}) / 2) + ',width={2},height={3}'));",
				MessagingUtilities.GetSafeJavascriptValue(popupUrl.AbsoluteUri),
				MessagingUtilities.GetSafeJavascriptValue(windowName),
				PopupWidth,
				PopupHeight);
		}
	}
}
