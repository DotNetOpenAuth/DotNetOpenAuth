//-----------------------------------------------------------------------
// <copyright file="AccountInfo.aspx.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Members {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.InfoCard;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using RelyingPartyLogic;

	public partial class AccountInfo : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			Database.LoggedInUser.AuthenticationTokens.Load();
			this.Repeater1.DataSource = Database.LoggedInUser.AuthenticationTokens;

			if (!Database.LoggedInUser.ClientAuthorizations.IsLoaded) {
				Database.LoggedInUser.ClientAuthorizations.Load();
			}
			this.tokenListRepeater.DataSource = Database.LoggedInUser.ClientAuthorizations;
			foreach (var token in Database.LoggedInUser.ClientAuthorizations) {
				if (!token.ClientReference.IsLoaded) {
					token.ClientReference.Load();
				}
			}
			this.authorizedClientsPanel.Visible = Database.LoggedInUser.ClientAuthorizations.Count > 0;

			if (!IsPostBack) {
				this.Repeater1.DataBind();
				this.tokenListRepeater.DataBind();
				this.emailBox.Text = Database.LoggedInUser.EmailAddress;
				this.emailVerifiedLabel.Visible = Database.LoggedInUser.EmailAddressVerified;
				this.firstNameBox.Text = Database.LoggedInUser.FirstName;
				this.lastNameBox.Text = Database.LoggedInUser.LastName;
			}

			this.firstNameBox.Focus();
		}

		protected void openIdBox_LoggedIn(object sender, OpenIdEventArgs e) {
			this.AddIdentifier(e.ClaimedIdentifier, e.Response.FriendlyIdentifierForDisplay);
		}

		protected void deleteOpenId_Command(object sender, CommandEventArgs e) {
			string claimedId = (string)e.CommandArgument;
			var token = Database.DataContext.AuthenticationTokens.First(t => t.ClaimedIdentifier == claimedId && t.User.UserId == Database.LoggedInUser.UserId);
			Database.DataContext.DeleteObject(token);
			Database.DataContext.SaveChanges();
			this.Repeater1.DataBind();
		}

		protected void saveChanges_Click(object sender, EventArgs e) {
			if (!IsValid) {
				return;
			}

			Database.LoggedInUser.EmailAddress = this.emailBox.Text;
			Database.LoggedInUser.FirstName = this.firstNameBox.Text;
			Database.LoggedInUser.LastName = this.lastNameBox.Text;
			this.emailVerifiedLabel.Visible = Database.LoggedInUser.EmailAddressVerified;
		}

		protected void InfoCardSelector1_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
			this.AddIdentifier(AuthenticationToken.SynthesizeClaimedIdentifierFromInfoCard(e.Token.UniqueId), e.Token.SiteSpecificId);
		}

		protected void revokeToken_Command(object sender, CommandEventArgs e) {
			int authorizationId = Convert.ToInt32(e.CommandArgument);
			var tokenToRevoke = Database.DataContext.ClientAuthorizations.FirstOrDefault(a => a.AuthorizationId == authorizationId && a.User.UserId == Database.LoggedInUser.UserId);
			if (tokenToRevoke != null) {
				Database.DataContext.DeleteObject(tokenToRevoke);
			}

			this.tokenListRepeater.DataBind();
			this.noAuthorizedClientsPanel.Visible = Database.LoggedInUser.ClientAuthorizations.Count == 0;
		}

		private void AddIdentifier(string claimedId, string friendlyId) {
			// Check that this identifier isn't already tied to a user account.
			// We do this again here in case the LoggingIn event couldn't verify
			// and in case somehow the OP changed it anyway.
			var existingToken = Database.DataContext.AuthenticationTokens.FirstOrDefault(token => token.ClaimedIdentifier == claimedId);
			if (existingToken == null) {
				var token = new AuthenticationToken();
				token.ClaimedIdentifier = claimedId;
				token.FriendlyIdentifier = friendlyId;
				Database.LoggedInUser.AuthenticationTokens.Add(token);
				Database.DataContext.SaveChanges();
				this.Repeater1.DataBind();

				// Clear the box for the next entry
				this.openIdSelector.Identifier = null;
			} else {
				if (existingToken.User == Database.LoggedInUser) {
					this.alreadyLinkedLabel.Visible = true;
				} else {
					this.differentAccountLabel.Visible = true;
				}
			}
		}
	}
}
