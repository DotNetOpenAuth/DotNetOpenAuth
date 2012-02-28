//-----------------------------------------------------------------------
// <copyright file="OpenIdInfoCardSelector.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IdentityModel.Claims;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.InfoCard;
	////using DotNetOpenAuth.InfoCard;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An ASP.NET control that provides a user-friendly way of logging into a web site using OpenID.
	/// </summary>
	[ToolboxData("<{0}:OpenIdInfoCardSelector runat=\"server\"></{0}:OpenIdInfoCardSelector>")]
	public class OpenIdInfoCardSelector : OpenIdSelector {
		/// <summary>
		/// The InfoCard selector button.
		/// </summary>
		private SelectorInfoCardButton selectorButton;

		/// <summary>
		/// Occurs when an InfoCard has been submitted and decoded.
		/// </summary>
		public event EventHandler<ReceivedTokenEventArgs> ReceivedToken;

		/// <summary>
		/// Occurs when [token processing error].
		/// </summary>
		public event EventHandler<TokenProcessingErrorEventArgs> TokenProcessingError;

		/// <summary>
		/// Ensures that the child controls have been built, but doesn't set control
		/// properties that require executing <see cref="Control.EnsureID"/> in order to avoid
		/// certain initialization order problems.
		/// </summary>
		/// <remarks>
		/// We don't just call EnsureChildControls() and then set the property on
		/// this.textBox itself because (apparently) setting this property in the ASPX
		/// page and thus calling this EnsureID() via EnsureChildControls() this early
		/// results in no ID.
		/// </remarks>
		protected override void EnsureChildControlsAreCreatedSafe() {
			if (this.selectorButton == null) {
				this.selectorButton = this.Buttons.OfType<SelectorInfoCardButton>().FirstOrDefault();
				if (this.selectorButton != null) {
					var selector = this.selectorButton.InfoCardSelector;
					selector.ClaimsRequested.Add(new ClaimType { Name = ClaimTypes.PPID });
					selector.ImageSize = InfoCardImageSize.Size60x42;
					selector.ReceivedToken += this.InfoCardSelector_ReceivedToken;
					selector.TokenProcessingError += this.InfoCardSelector_TokenProcessingError;
					this.Controls.Add(selector);
				}
			}

			base.EnsureChildControlsAreCreatedSafe();
		}

		/// <summary>
		/// Fires the <see cref="ReceivedToken"/> event.
		/// </summary>
		/// <param name="e">The token, if it was decrypted.</param>
		protected virtual void OnReceivedToken(ReceivedTokenEventArgs e) {
			Contract.Requires(e != null);
			ErrorUtilities.VerifyArgumentNotNull(e, "e");

			var receivedInfoCard = this.ReceivedToken;
			if (receivedInfoCard != null) {
				receivedInfoCard(this, e);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:TokenProcessingError"/> event.
		/// </summary>
		/// <param name="e">The <see cref="DotNetOpenAuth.InfoCard.TokenProcessingErrorEventArgs"/> instance containing the event data.</param>
		protected virtual void OnTokenProcessingError(TokenProcessingErrorEventArgs e) {
			Contract.Requires(e != null);
			ErrorUtilities.VerifyArgumentNotNull(e, "e");

			var tokenProcessingError = this.TokenProcessingError;
			if (tokenProcessingError != null) {
				tokenProcessingError(this, e);
			}
		}

		/// <summary>
		/// Handles the ReceivedToken event of the infoCardSelector control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DotNetOpenAuth.InfoCard.ReceivedTokenEventArgs"/> instance containing the event data.</param>
		private void InfoCardSelector_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
			this.Page.Response.SetCookie(new HttpCookie("openid_identifier", "infocard") {
				Path = this.Page.Request.ApplicationPath,
			});
			this.OnReceivedToken(e);
		}

		/// <summary>
		/// Handles the TokenProcessingError event of the infoCardSelector control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DotNetOpenAuth.InfoCard.TokenProcessingErrorEventArgs"/> instance containing the event data.</param>
		private void InfoCardSelector_TokenProcessingError(object sender, TokenProcessingErrorEventArgs e) {
			this.OnTokenProcessingError(e);
		}
	}
}
