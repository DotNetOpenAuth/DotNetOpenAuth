//-----------------------------------------------------------------------
// <copyright file="UIModes.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.UI {
	/// <summary>
	/// Valid values for the <c>mode</c> parameter of the OpenID User Interface extension.
	/// </summary>
	public static class UIModes {
		/// <summary>
		/// Indicates that the Provider's authentication page appears in a popup window.
		/// </summary>
		/// <value>The constant <c>"popup"</c>.</value>
		/// <remarks>
		/// <para>The RP SHOULD create the popup to be 450 pixels wide and 500 pixels tall. The popup MUST have the address bar displayed, and MUST be in a standalone browser window. The contents of the popup MUST NOT be framed by the RP. </para>
		/// <para>The RP SHOULD open the popup centered above the main browser window, and SHOULD dim the contents of the parent window while the popup is active. The RP SHOULD ensure that the user is not surprised by the appearance of the popup, and understands how to interact with it. </para>
		/// <para>To keep the user popup user experience consistent, it is RECOMMENDED that the OP does not resize the popup window unless the OP requires additional space to show special features that are not usually displayed as part of the default popup user experience. </para>
		/// <para>The OP MAY close the popup without returning a response to the RP. Closing the popup without sending a response should be interpreted as a negative assertion. </para>
		/// <para>The response to an authentication request in a popup is unchanged from [OpenID 2.0] (OpenID 2.0 Workgroup, “OpenID 2.0,” .). Relying Parties detecting that the popup was closed without receiving an authentication response SHOULD interpret the close event to be a negative assertion.  </para>
		/// </remarks>
		public const string Popup = "popup";
	}
}
