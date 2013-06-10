namespace OAuthClient {
	using System;
	using System.Configuration;
	using System.Net;
	using System.Runtime.Serialization.Json;
	using System.Web;
	using System.Web.UI;

	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	public partial class AzureAD : System.Web.UI.Page {
		private static readonly AzureADClient client = new AzureADClient {
			ClientIdentifier = ConfigurationManager.AppSettings["AzureADAppID"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["AzureADAppSecret"]),
		};

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						IAuthorizationState authorization = await client.ProcessUserAuthorizationAsync(new HttpRequestWrapper(Request), ct);
						if (authorization == null) {
							// Kick off authorization request
							var request = await client.PrepareRequestUserAuthorizationAsync(cancellationToken: ct);
							await request.SendAsync();
							this.Context.Response.End();
						} else {
							string token = authorization.AccessToken;
							AzureADClaims claimsAD = client.ParseAccessToken(token);

							// Request to AD needs a {tenantid}/users/{userid}
							var request =
								WebRequest.Create("https://graph.windows.net/" + claimsAD.Tid + "/users/" + claimsAD.Oid + "?api-version=0.9");
							request.Headers = new WebHeaderCollection();
							request.Headers.Add("authorization", token);
							using (var response = request.GetResponse()) {
								using (var responseStream = response.GetResponseStream()) {
									var serializer = new DataContractJsonSerializer(typeof(AzureADGraph));
									AzureADGraph graphData = (AzureADGraph)serializer.ReadObject(responseStream);
									this.nameLabel.Text = HttpUtility.HtmlEncode(graphData.DisplayName);
								}
							}
						}
					}));
		}
	}
}