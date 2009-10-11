//-----------------------------------------------------------------------
// <copyright file="AccountInfo.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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

	public partial class AccountInfo : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			Global.LoggedInUser.AuthenticationTokens.Load();
			this.Repeater1.DataSource = Global.LoggedInUser.AuthenticationTokens;
			if (!IsPostBack) {
				this.Repeater1.DataBind();
				this.emailBox.Text = Global.LoggedInUser.EmailAddress;
				this.firstNameBox.Text = Global.LoggedInUser.FirstName;
				this.lastNameBox.Text = Global.LoggedInUser.LastName;
			}

			this.firstNameBox.Focus();
		}

		protected void openIdBox_LoggingIn(object sender, OpenIdEventArgs e) {
			// Find out if this is an OpenID that is already bound to any account,
			// but we can only check this for non-directed identity cases at this stage.
			if (!e.IsDirectedIdentity) {
				string claimedId = e.ClaimedIdentifier;
				var existingToken = Global.DataContext.AuthenticationToken.FirstOrDefault(token => token.ClaimedIdentifier == claimedId);
				if (existingToken != null) {
					// Report the error to the user and cancel the login.
					e.Cancel = true;
				}
			}
		}

		protected void openIdBox_LoggedIn(object sender, OpenIdEventArgs e) {
			this.AddIdentifier(e.ClaimedIdentifier, e.Response.FriendlyIdentifierForDisplay);
		}

		protected void addOpenId_Click(object sender, EventArgs e) {
			// We don't need to do anything here.  The postback itself
			// is enough to fire the event that gets the new openid added.
		}

		protected void deleteOpenId_Command(object sender, CommandEventArgs e) {
			string claimedId = (string)e.CommandArgument;
			var token = Global.DataContext.AuthenticationToken.First(t => t.ClaimedIdentifier == claimedId && t.User.Id == Global.LoggedInUser.Id);
			Global.DataContext.DeleteObject(token);
			Global.DataContext.SaveChanges();
			this.Repeater1.DataBind();
		}

		protected void saveChanges_Click(object sender, EventArgs e) {
			if (!IsValid) {
				return;
			}

			Global.LoggedInUser.EmailAddress = this.emailBox.Text;
			Global.LoggedInUser.FirstName = this.firstNameBox.Text;
			Global.LoggedInUser.LastName = this.lastNameBox.Text;
		}

		protected void InfoCardSelector1_ReceivedToken(object sender, ReceivedTokenEventArgs e) {
			this.AddIdentifier(AuthenticationToken.SynthesizeClaimedIdentifierFromInfoCard(e.Token.UniqueId), e.Token.SiteSpecificId);
		}

		private void AddIdentifier(string claimedId, string friendlyId) {
			// Check that this identifier isn't already tied to a user account.
			// We do this again here in case the LoggingIn event couldn't verify
			// and in case somehow the OP changed it anyway.
			var existingToken = Global.DataContext.AuthenticationToken.FirstOrDefault(token => token.ClaimedIdentifier == claimedId);
			if (existingToken == null) {
				var token = new AuthenticationToken();
				token.ClaimedIdentifier = claimedId;
				token.FriendlyIdentifier = friendlyId;
				Global.LoggedInUser.AuthenticationTokens.Add(token);
				Global.DataContext.SaveChanges();
				this.Repeater1.DataBind();

				// Clear the box for the next entry
				this.openIdBox.Text = string.Empty;
			} else {
				if (existingToken.User == Global.LoggedInUser) {
					this.alreadyLinkedLabel.Visible = true;
				} else {
					this.differentAccountLabel.Visible = true;
				}
			}
		}
	}
}
