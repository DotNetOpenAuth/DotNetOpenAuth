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

	public class OpenIdSelectorOptions {
		public OpenIdSelectorOptions() {
			this.DownloadYahooUILibrary = true;
			this.AssertionHiddenFieldId = "openid_openidAuthData";
		}

		public bool DownloadYahooUILibrary { get; set; }

		public string AssertionHiddenFieldId { get; set; }

		public bool ShowDiagnosticTrace { get; set; }

		public bool ShowDiagnosticIFrame { get; set; }
	}
}
