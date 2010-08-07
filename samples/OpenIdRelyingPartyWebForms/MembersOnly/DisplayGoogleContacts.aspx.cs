namespace OpenIdRelyingPartyWebForms.MembersOnly {
	using System;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Xml.Linq;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

	public partial class DisplayGoogleContacts : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!string.IsNullOrEmpty(State.GoogleAccessToken)) {
				this.MultiView1.ActiveViewIndex = 1;
				if (State.FetchResponse != null && State.FetchResponse.Attributes.Contains(WellKnownAttributes.Contact.Email)) {
					this.emailLabel.Text = State.FetchResponse.Attributes[WellKnownAttributes.Contact.Email].Values[0];
				} else {
					this.emailLabel.Text = "unavailable";
				}
				this.claimedIdLabel.Text = this.User.Identity.Name;
				var contactsDocument = GoogleConsumer.GetContacts(Global.GoogleWebConsumer, State.GoogleAccessToken, 25, 1);
				this.RenderContacts(contactsDocument);
			}
		}

		private void RenderContacts(XDocument contactsDocument) {
			var contacts = from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
						   select new { Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value, Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value };
			StringBuilder tableBuilder = new StringBuilder();
			tableBuilder.Append("<table><tr><td>Name</td><td>Email</td></tr>");
			foreach (var contact in contacts) {
				tableBuilder.AppendFormat(
					"<tr><td>{0}</td><td>{1}</td></tr>",
					HttpUtility.HtmlEncode(contact.Name),
					HttpUtility.HtmlEncode(contact.Email));
			}
			tableBuilder.Append("</table>");
			this.resultsPlaceholder.Controls.Add(new Literal { Text = tableBuilder.ToString() });
		}
	}
}
