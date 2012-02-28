<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Setup.aspx.cs" Inherits="WebFormsRelyingParty.Setup" %>

<%@ Register Assembly="DotNetOpenAuth.OpenID.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>OpenID RP one-time setup</title>
</head>
<body>
	<form id="form1" runat="server">
	<h2>
		First steps:
	</h2>
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<p>
				Before you can use this site, you must create your SQL database that will store
				your user accounts and add an admin account to that database.
				Just tell me what OpenID you will use to administer the site.
			</p>
			<rp:OpenIdLogin runat="server" ButtonText="Create database" ID="openidLogin" 
				OnLoggingIn="openidLogin_LoggingIn" Stateless="true"
				TabIndex="1" LabelText="Administrator's OpenID:"
				ButtonToolTip="Clicking this button will create the database and initialize the OpenID you specify as an admin of this web site." 
				RegisterText="get an OpenID" />
			<asp:Label ID="noOPIdentifierLabel" Visible="false" EnableViewState="false" ForeColor="Red" Font-Bold="true" runat="server" Text="Sorry.  To help your admin account remain functional when you push this web site to production, directed identity is disabled on this page.  Please use your personal claimed identifier." />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<p>
				Your database has been successfully initialized.
			</p>
			<p>
				<b>Remember to delete this Setup.aspx page.</b>
			</p>
			<p>
				Visit the <a href="Default.aspx">home page</a>.
			</p>
		</asp:View>
	</asp:MultiView>
	</form>
</body>
</html>
