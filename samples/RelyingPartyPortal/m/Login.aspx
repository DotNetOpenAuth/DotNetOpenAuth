<%@ Page Language="C#" CodeBehind="Login.aspx.cs" Inherits="ConsumerPortal.m.Login" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="RP" %>
<%@ Register Assembly="System.Web.Mobile" Namespace="System.Web.UI.MobileControls"
	TagPrefix="MC" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<body>
	<MC:Form id="form1" runat="server">
		<RP:OpenIdMobileTextBox ID="openIdTextBox" runat="server" />
		<MC:Command runat="server" ID="loginButton" OnClick="loginButton_Click" Text="Login" />
	</MC:Form>
</body>
</html>
