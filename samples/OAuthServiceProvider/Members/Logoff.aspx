<%@ Page Title="Log off" Language="C#" MasterPageFile="~/MasterPage.master" %>

<script runat="server">
	private void Page_Load(object sender, EventArgs e) {
		FormsAuthentication.SignOut();
		Response.Redirect("~/");
	}
</script>
