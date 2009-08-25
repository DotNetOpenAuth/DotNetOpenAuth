<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<script runat="server">
	private void Page_Load(object sender, EventArgs e) {
		FormsAuthentication.SignOut();
		Response.Redirect("~/");
	}
</script>
