using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using DotNetOAuth;
using DotNetOAuth.ChannelElements;
using DotNetOAuth.Messaging;

/// <summary>
/// A page to demonstrate downloading a Gmail address book using OAuth.
/// </summary>
public partial class GoogleAddressBook : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			if (Session["TokenManager"] != null) {
				InMemoryTokenManager tokenManager = (InMemoryTokenManager)Session["TokenManager"];
				Consumer google = new Consumer(Constants.GoogleDescription, tokenManager) {
					ConsumerKey = tokenManager.ConsumerKey,
					ConsumerSecret = tokenManager.ConsumerSecret,
				};

				var accessTokenMessage = google.ProcessUserAuthorization();
				if (accessTokenMessage != null) {
					// User has approved access
					MultiView1.ActiveViewIndex = 1;
					resultsPlaceholder.Controls.Add(new Label { Text = accessTokenMessage.AccessToken });

					Response contactsResponse = google.SendAuthorizedRequest(Constants.GoogleScopes.GetContacts, accessTokenMessage.AccessToken);
					XDocument contactsDocument = XDocument.Parse(contactsResponse.Body);
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
		Consumer google = new Consumer(Constants.GoogleDescription, tokenManager);
		google.ConsumerKey = consumerKeyBox.Text;
		google.ConsumerSecret = consumerSecretBox.Text;

		var extraParameters = new Dictionary<string, string> {
			{ "scope", Constants.GoogleScopes.Contacts },
		};
		google.RequestUserAuthorization(new Uri(Request.Url, Request.RawUrl), extraParameters, null).Send();
	}
}
