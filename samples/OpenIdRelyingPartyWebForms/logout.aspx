<%@ Page Language="C#" %>
<%@ Import Namespace="OpenIdRelyingPartyWebForms" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	protected void Page_Load(object sender, EventArgs e) {
		State.FriendlyLoginName = null;
		State.ProfileFields = null;
		System.Web.Security.FormsAuthentication.SignOut();
		DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyControlBase.LogOff();
		Response.Redirect("~/");
	}
</script>

