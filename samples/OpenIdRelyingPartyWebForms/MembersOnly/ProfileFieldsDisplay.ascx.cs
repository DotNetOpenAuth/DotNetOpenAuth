namespace OpenIdRelyingPartyWebForms.MembersOnly {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	public partial class ProfileFieldsDisplay : System.Web.UI.UserControl {
		public ClaimsResponse ProfileValues { get; set; }
	}
}