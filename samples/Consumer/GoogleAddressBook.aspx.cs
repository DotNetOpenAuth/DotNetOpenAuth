using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using DotNetOAuth.CommonConsumers;

/// <summary>
/// A page to demonstrate downloading a Gmail address book using OAuth.
/// </summary>
public partial class GoogleAddressBook : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			if (Session["TokenManager"] != null) {
				InMemoryTokenManager tokenManager = (InMemoryTokenManager)Session["TokenManager"];
				GoogleConsumer google = new GoogleConsumer(tokenManager, tokenManager.ConsumerKey);

				var accessToken = google.GetAccessToken();
				if (accessToken != null) {
					// User has approved access
					MultiView1.ActiveViewIndex = 1;
					resultsPlaceholder.Controls.Add(new Label { Text = accessToken });

					XDocument contactsDocument = google.GetContacts(accessToken);
					var contacts = from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
						select new {
							Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value,
							Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value,
						};
					StringBuilder tableBuilder = new StringBuilder();
					tableBuilder.Append("<table><tr><td>Name</td><td>Email</td></tr>");
					foreach (var contact in contacts) {
						tableBuilder.AppendFormat(
							"<tr><td>{0}</td><td>{1}</td></tr>",
							HttpUtility.HtmlEncode(contact.Name),
							HttpUtility.HtmlEncode(contact.Email));
					}
					tableBuilder.Append("</table>");
					resultsPlaceholder.Controls.Add(new Literal { Text = tableBuilder.ToString() });
				}
			}
		}
	}

	protected void authorizeButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		InMemoryTokenManager tokenManager = new InMemoryTokenManager(consumerKeyBox.Text, consumerSecretBox.Text);
		Session["TokenManager"] = tokenManager;
		var google = new GoogleConsumer(tokenManager, consumerKeyBox.Text);
		google.RequestAuthorization(GoogleConsumer.Applications.Contacts);
	}
}
