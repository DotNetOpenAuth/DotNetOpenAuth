namespace OAuthConsumer {
	using System;
	using System.Configuration;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Xml.Linq;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth;

	/// <summary>
	/// A page to demonstrate downloading a Gmail address book using OAuth.
	/// </summary>
	public partial class GoogleAddressBook : System.Web.UI.Page {
		private AccessToken AccessToken {
			get { return (AccessToken)Session["GoogleAccessToken"]; }
			set { Session["GoogleAccessToken"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var google = new GoogleConsumer();
						if (google.ConsumerKey != null) {
							this.MultiView1.ActiveViewIndex = 1;

							if (!IsPostBack) {
								// Is Google calling back with authorization?
								var accessTokenResponse = await google.ProcessUserAuthorizationAsync(this.Request.Url);
								if (accessTokenResponse != null) {
									this.AccessToken = accessTokenResponse.AccessToken;
								} else if (this.AccessToken.Token == null) {
									// If we don't yet have access, immediately request it.
									Uri redirectUri = await google.RequestUserAuthorizationAsync(GoogleConsumer.Applications.Contacts);
									this.Response.Redirect(redirectUri.AbsoluteUri);
								}
							}
						}
					}));
		}

		protected void getAddressBookButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var google = new GoogleConsumer();

						XDocument contactsDocument =
							await google.GetContactsAsync(this.AccessToken, 5, 1, Response.ClientDisconnectedToken);
						var contacts =
							from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
							select new {
								Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value,
								Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value,
							};
						StringBuilder tableBuilder = new StringBuilder();
						tableBuilder.Append("<table><tr><td>Name</td><td>Email</td></tr>");
						foreach (var contact in contacts) {
							tableBuilder.AppendFormat(
								"<tr><td>{0}</td><td>{1}</td></tr>", HttpUtility.HtmlEncode(contact.Name), HttpUtility.HtmlEncode(contact.Email));
						}
						tableBuilder.Append("</table>");
						this.resultsPlaceholder.Controls.Add(new Literal { Text = tableBuilder.ToString() });
					}));
		}
	}
}