using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Provider;

public partial class RP_SregAccountCreation : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			// Only allow this to be used as an identity page when the test parameter is included
			IdentityEndpoint1.Visible = !string.IsNullOrEmpty(Request.QueryString["test"]);

			// Set the test parameter that makes each run of the test unique to an RP.
			claimedIdLabel.Text = DeriveUniqueClaimedIdentifier().AbsoluteUri;
		}
	}

	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		ClaimsRequest sregRequest = e.Request.GetExtension<ClaimsRequest>();
		if (sregRequest == null) {
			// The RP isn't even sending an sreg extension, so it already failed.
			NoSregRequest.Visible = true;
		}
		e.Request.AddResponseExtension(CreateSregResponse(sregRequest));
		e.Request.IsAuthenticated = true;
	}

	private ClaimsResponse CreateSregResponse(ClaimsRequest request) {
		ClaimsResponse response = request.CreateResponse();
		if (request.BirthDate != DemandLevel.NoRequest) {
			response.BirthDate = DateTime.Now - TimeSpan.FromDays(18 * 365);
		}
		if (request.Country != DemandLevel.NoRequest) {
			response.Country = "United States";
		}
		if (request.Email != DemandLevel.NoRequest) {
			response.Email = "osistest@test-id.net";
		}
		if (request.FullName != DemandLevel.NoRequest) {
			response.Country = "OSIS Tester";
		}
		if (request.Gender != DemandLevel.NoRequest) {
			response.Gender = Gender.Male;
		}
		if (request.Language != DemandLevel.NoRequest) {
			response.Language = Thread.CurrentThread.CurrentCulture.Name;
		}
		if (request.Nickname != DemandLevel.NoRequest) {
			response.Nickname = "osistester";
		}
		if (request.PostalCode != DemandLevel.NoRequest) {
			response.PostalCode = "12345";
		}
		if (request.TimeZone != DemandLevel.NoRequest) {
			response.TimeZone = "Pacific time";
		}

		return response;
	}

	/// <summary>
	/// Generates a unique Claimed Identifier for use with this test,
	/// allowing each tester to get their own claimed_id, guaranteeing
	/// a valid account creation test since no claimed_id can be used twice.
	/// </summary>
	private Uri DeriveUniqueClaimedIdentifier() {
		UriBuilder claimedId = new UriBuilder(Request.Url);
		claimedId.Query = null;
		claimedId.Fragment = null;

		// We generate random bytes, then base64 encode them to remove any symbols
		// that might trigger HttpRequestValidationException when th request comes back
		// due to security concerns of having < and other symbols in the request URL.
		byte[] unique = new byte[6];
		Random r = new Random();
		r.NextBytes(unique);
		claimedId.Query = "test=" + HttpUtility.UrlEncode(Convert.ToBase64String(unique));
		return claimedId.Uri;
	}
}
