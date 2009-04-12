using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_Sreg : System.Web.UI.Page {
	protected ClaimsResponse SregResponse;

	protected void Page_Load(object sender, EventArgs e) {
		OpenIdBox.Focus();
	}

	protected void OpenIdBox_LoggingIn(object sender, OpenIdEventArgs e) {
		ClaimsRequest request = new ClaimsRequest {
			Nickname = GetRequestLevel(nickNameList),
			Email = GetRequestLevel(emailList),
			FullName = GetRequestLevel(fullNameList),
			BirthDate = GetRequestLevel(dateOfBirthList),
			Gender = GetRequestLevel(genderList),
			PostalCode = GetRequestLevel(postalCodeList),
			Country = GetRequestLevel(countryList),
			Language = GetRequestLevel(languageList),
			TimeZone = GetRequestLevel(languageList),
			PolicyUrl = privacyPolicyCheckbox.Checked ? new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/PrivacyPolicy.aspx")) : null,
		};
		e.Request.AddExtension(request);
	}

	protected void OpenIdBox_LoggedIn(object sender, OpenIdEventArgs e) {
		e.Cancel = true;
		this.SregResponse = e.Response.GetExtension<ClaimsResponse>();
		MultiView1.ActiveViewIndex = 1;
	}

	private DemandLevel GetRequestLevel(DropDownList list) {
		switch (list.SelectedIndex) {
			case 0: return DemandLevel.NoRequest;
			case 1: return DemandLevel.Request;
			case 2: return DemandLevel.Require;
			default: throw new ApplicationException();
		}
	}
}
