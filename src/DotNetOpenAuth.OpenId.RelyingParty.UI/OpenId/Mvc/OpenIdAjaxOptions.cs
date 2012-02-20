//-----------------------------------------------------------------------
// <copyright file="OpenIdAjaxOptions.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Mvc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A set of customizations available for the scripts sent to the browser in AJAX OpenID scenarios.
	/// </summary>
	public class OpenIdAjaxOptions {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdAjaxOptions"/> class.
		/// </summary>
		public OpenIdAjaxOptions() {
			this.AssertionHiddenFieldId = "openid_openidAuthData";
			this.ReturnUrlHiddenFieldId = "ReturnUrl";
		}

		/// <summary>
		/// Gets or sets the ID of the hidden field that should carry the positive assertion
		/// until it is posted to the RP.
		/// </summary>
		public string AssertionHiddenFieldId { get; set; }

		/// <summary>
		/// Gets or sets the ID of the hidden field that should be set with the parent window/frame's URL
		/// prior to posting the form with the positive assertion.  Useful for jQuery popup dialogs.
		/// </summary>
		public string ReturnUrlHiddenFieldId { get; set; }

		/// <summary>
		/// Gets or sets the index of the form in the document.forms array on the browser that should
		/// be submitted when the user is ready to send the positive assertion to the RP.
		/// </summary>
		public int FormIndex { get; set; }

		/// <summary>
		/// Gets or sets the id of the form in the document.forms array on the browser that should
		/// be submitted when the user is ready to send the positive assertion to the RP.  A value
		/// in this property takes precedence over any value in the <see cref="FormIndex"/> property.
		/// </summary>
		/// <value>The form id.</value>
		public string FormId { get; set; }

		/// <summary>
		/// Gets or sets the preloaded discovery results.
		/// </summary>
		public string PreloadedDiscoveryResults { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to print diagnostic trace messages in the browser.
		/// </summary>
		public bool ShowDiagnosticTrace { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to show all the "hidden" iframes that facilitate
		/// asynchronous authentication of the user for diagnostic purposes.
		/// </summary>
		public bool ShowDiagnosticIFrame { get; set; }

		/// <summary>
		/// Gets the form key to use when accessing the relevant form.
		/// </summary>
		internal string FormKey {
			get { return string.IsNullOrEmpty(this.FormId) ? this.FormIndex.ToString(CultureInfo.InvariantCulture) : MessagingUtilities.GetSafeJavascriptValue(this.FormId); }
		}
	}
}
