//-----------------------------------------------------------------------
// <copyright file="OpenIdSelectorOptions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Mvc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class OpenIdAjaxOptions {
		public OpenIdAjaxOptions() {
			this.AssertionHiddenFieldId = "openid_openidAuthData";
			this.ReturnUrlHiddenFieldId = "ReturnUrl";
		}

		public string AssertionHiddenFieldId { get; set; }

		public string ReturnUrlHiddenFieldId { get; set; }

		public int FormIndex { get; set; }

		public bool ShowDiagnosticTrace { get; set; }

		public bool ShowDiagnosticIFrame { get; set; }
	}
}
