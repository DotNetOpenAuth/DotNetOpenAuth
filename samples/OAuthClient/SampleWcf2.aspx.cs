namespace OAuthClient {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Security;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using SampleResourceServer;

	public partial class SampleWcf2 : System.Web.UI.Page {
		/// <summary>
		/// The OAuth 2.0 client object to use to obtain authorization and authorize outgoing HTTP requests.
		/// </summary>
		private static readonly WebServerClient Client;

		/// <summary>
		/// The details about the sample OAuth-enabled WCF service that this sample client calls into.
		/// </summary>
		private static AuthorizationServerDescription authServerDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("http://localhost:50172/OAuth/Token"),
			AuthorizationEndpoint = new Uri("http://localhost:50172/OAuth/Authorize"),
		};

		/// <summary>
		/// Initializes static members of the <see cref="SampleWcf2"/> class.
		/// </summary>
		static SampleWcf2() {
			Client = new WebServerClient(authServerDescription, "sampleconsumer", "samplesecret");
		}

		/// <summary>
		/// Gets or sets the authorization details for the logged in user.
		/// </summary>
		/// <value>The authorization details.</value>
		/// <remarks>
		/// Because this is a sample, we simply store the authorization information in memory with the user session.
		/// A real web app should store at least the access and refresh tokens in this object in a database associated with the user.
		/// </remarks>
		private static IAuthorizationState Authorization {
			get { return (AuthorizationState)HttpContext.Current.Session["Authorization"]; }
			set { HttpContext.Current.Session["Authorization"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (!IsPostBack) {
							// Check to see if we're receiving a end user authorization response.
							var authorization =
								await Client.ProcessUserAuthorizationAsync(new HttpRequestWrapper(Request), Response.ClientDisconnectedToken);
							if (authorization != null) {
								// We are receiving an authorization response.  Store it and associate it with this user.
								Authorization = authorization;
								Response.Redirect(Request.Path); // get rid of the /?code= parameter
							}
						}

						if (Authorization != null) {
							// Indicate to the user that we have already obtained authorization on some of these.
							foreach (var li in this.scopeList.Items.OfType<ListItem>().Where(li => Authorization.Scope.Contains(li.Value))) {
								li.Selected = true;
							}
							this.authorizationLabel.Text = "Authorization received!";
							if (Authorization.AccessTokenExpirationUtc.HasValue) {
								TimeSpan timeLeft = Authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
								this.authorizationLabel.Text += string.Format(
									CultureInfo.CurrentCulture, "  (access token expires in {0} minutes)", Math.Round(timeLeft.TotalMinutes, 1));
							}
						}

						this.getNameButton.Enabled = this.getAgeButton.Enabled = this.getFavoriteSites.Enabled = Authorization != null;
					}));
		}

		protected void getAuthorizationButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						string[] scopes =
							(from item in this.scopeList.Items.OfType<ListItem>() where item.Selected select item.Value).ToArray();

						var request =
							await Client.PrepareRequestUserAuthorizationAsync(scopes, cancellationToken: Response.ClientDisconnectedToken);
						await request.SendAsync();
						this.Context.Response.End();
					}));
		}

		protected void getNameButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						try {
							this.nameLabel.Text = await this.CallServiceAsync(client => client.GetName(), Response.ClientDisconnectedToken);
						} catch (SecurityAccessDeniedException) {
							this.nameLabel.Text = "Access denied!";
						} catch (MessageSecurityException) {
							this.nameLabel.Text = "Access denied!";
						}
					}));
		}

		protected void getAgeButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						try {
							int? age = await this.CallServiceAsync(client => client.GetAge(), Response.ClientDisconnectedToken);
							this.ageLabel.Text = age.HasValue ? age.Value.ToString(CultureInfo.CurrentCulture) : "not available";
						} catch (SecurityAccessDeniedException) {
							this.ageLabel.Text = "Access denied!";
						} catch (MessageSecurityException) {
							this.ageLabel.Text = "Access denied!";
						}
					}));
		}

		protected void getFavoriteSites_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						try {
							string[] favoriteSites =
								await this.CallServiceAsync(client => client.GetFavoriteSites(), Response.ClientDisconnectedToken);
							this.favoriteSitesLabel.Text = string.Join(", ", favoriteSites);
						} catch (SecurityAccessDeniedException) {
							this.favoriteSitesLabel.Text = "Access denied!";
						} catch (MessageSecurityException) {
							this.favoriteSitesLabel.Text = "Access denied!";
						}
					}));
		}

		private async Task<T> CallServiceAsync<T>(Func<DataApiClient, T> predicate, CancellationToken cancellationToken) {
			if (Authorization == null) {
				throw new InvalidOperationException("No access token!");
			}

			var wcfClient = new DataApiClient();

			// Refresh the access token if it expires and if its lifetime is too short to be of use.
			if (Authorization.AccessTokenExpirationUtc.HasValue) {
				if (await Client.RefreshAuthorizationAsync(Authorization, TimeSpan.FromSeconds(30))) {
					TimeSpan timeLeft = Authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
					this.authorizationLabel.Text += string.Format(CultureInfo.CurrentCulture, " - just renewed for {0} more minutes)", Math.Round(timeLeft.TotalMinutes, 1));
				}
			}

			var httpRequest = (HttpWebRequest)WebRequest.Create(wcfClient.Endpoint.Address.Uri);
			ClientBase.AuthorizeRequest(httpRequest, Authorization.AccessToken);

			var httpDetails = new HttpRequestMessageProperty();
			httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
			using (var scope = new OperationContextScope(wcfClient.InnerChannel)) {
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
				return predicate(wcfClient);
			}
		}
	}
}