//-----------------------------------------------------------------------
// <copyright file="UIConstants.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.UI {
	/// <summary>
	/// Constants used to support the UI extension.
	/// </summary>
	internal static class UIConstants {
		/// <summary>
		/// The type URI associated with this extension.
		/// </summary>
		internal const string UITypeUri = "http://specs.openid.net/extensions/ui/1.0";

		/// <summary>
		/// The Type URI that appears in an XRDS document when the OP supports popups through the UI extension.
		/// </summary>
		internal const string PopupSupported = "http://specs.openid.net/extensions/ui/1.0/mode/popup";

		/// <summary>
		/// The Type URI that appears in an XRDS document when the OP supports the RP
		/// specifying the user's preferred language through the UI extension.
		/// </summary>
		internal const string LangPrefSupported = "http://specs.openid.net/extensions/ui/1.0/lang-pref";

		/// <summary>
		/// The Type URI that appears in the XRDS document when the OP supports the RP
		/// specifying the icon for the OP to display during authentication through the UI extension.
		/// </summary>
		internal const string IconSupported = "http://specs.openid.net/extensions/ui/1.0/icon";
	}
}
