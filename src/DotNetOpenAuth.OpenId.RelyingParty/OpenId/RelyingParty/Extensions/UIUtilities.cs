//-----------------------------------------------------------------------
// <copyright file="UIUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty.Extensions.UI {
	using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.RelyingParty;
using Validation;

	/// <summary>
	/// Constants used in implementing support for the UI extension.
	/// </summary>
	internal static class UIUtilities {
		/// <summary>
		/// Gets the <c>window.open</c> javascript snippet to use to open a popup window
		/// compliant with the UI extension.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="request">The authentication request to place in the window.</param>
		/// <param name="windowName">The name to assign to the popup window.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A string starting with 'window.open' and forming just that one method call.
		/// </returns>
		internal static async Task<string> GetWindowPopupScriptAsync(OpenIdRelyingParty relyingParty, IAuthenticationRequest request, string windowName, CancellationToken cancellationToken) {
			Requires.NotNull(relyingParty, "relyingParty");
			Requires.NotNull(request, "request");
			Requires.NotNullOrEmpty(windowName, "windowName");

			var response = await request.GetRedirectingResponseAsync(cancellationToken);
			Uri popupUrl = response.GetDirectUriRequest();

			return string.Format(
				CultureInfo.InvariantCulture,
				"(window.showModalDialog ? window.showModalDialog({0}, {1}, 'status:0;resizable:1;scroll:1;center:1;dialogWidth:{2}px; dialogHeight:{3}') : window.open({0}, {1}, 'status=0,toolbar=0,location=1,resizable=1,scrollbars=1,left=' + ((screen.width - {2}) / 2) + ',top=' + ((screen.height - {3}) / 2) + ',width={2},height={3}'));",
				MessagingUtilities.GetSafeJavascriptValue(popupUrl.AbsoluteUri),
				MessagingUtilities.GetSafeJavascriptValue(windowName),
				OpenId.Extensions.UI.UIUtilities.PopupWidth,
				OpenId.Extensions.UI.UIUtilities.PopupHeight);
		}
	}
}
