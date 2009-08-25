<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (ViewData["ActiveIndex"] != null && !String.IsNullOrEmpty(ViewData["ActiveIndex"].ToString()))
            multiView.ActiveViewIndex = Int32.Parse(ViewData["ActiveIndex"].ToString());
    }
</script>
<asp:Content ID="Content1" ContentPlaceHolderID="Body" runat="server">

	<asp:MultiView runat="server" ActiveViewIndex="0" ID="multiView">
		<asp:View ID="View1" runat="server">
			<div style="background-color: Yellow">
				<b>Warning</b>: Never give your login credentials to another web site or application.
			</div>
			<input name="OAuthAuthorizationSecToken" type="hidden" value="<%= TempData["OAuthAuthorizationSecToken"]%>" /> 
			<asp:HiddenField runat="server" ID="OAuthAuthorizationSecToken" EnableViewState="false" />
			<p>The client web site or application <%= ViewData["ConsumerKey"]%> wants access to your <span style="font-weight:bold"><%= ViewData["Scope"]%></span>.</p>
			<p>Do you want to allow this? </p>
			<div>
			    <input name=allow type="radio" value="1" />Yes
			    <input name=allow type="radio" value="0" />No
			    <input type=submit value="confirm" />
			</div>
			<p>If you grant access now, you can revoke it at any time by returning to this page.
			</p>
			<% if (Boolean.Parse(ViewData["OAuth10ConsumerWarningVisible"].ToString())) { %>
				This website is registered with service_PROVIDER_DOMAIN_NAME to make authorization requests, but has not been configured to send requests securely. If you grant access but you did not initiate this request at consumer_DOMAIN_NAME, it may be possible for other users of consumer_DOMAIN_NAME to access your data. We recommend you deny access unless you are certain that you initiated this request directly with consumer_DOMAIN_NAME.
            <% } %>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<p>Authorization has been granted.</p>
			<% if (ViewData["VerifierMultiView"]!=null && ViewData["VerifierMultiView"].ToString() == "1")
      { %>

					<p>You must enter this verification code at the Consumer: <%= ViewData["VerificationCode"] %> </p>
            <% }
      else
      { %>
					<p>You may now close this window and return to the Consumer. </p>
					
            <% } %>
		</asp:View>
		<asp:View ID="View5" runat="server">
			<p>Authorization has been denied. You're free to do whatever now. </p>
		</asp:View>
	</asp:MultiView>

</asp:Content>