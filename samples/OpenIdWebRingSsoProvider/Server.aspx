<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Server.aspx.cs" Inherits="OpenIdWebRingSsoProvider.Server" ValidateRequest="false" Async="true" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.Provider.UI" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<openid:ProviderEndpoint runat="server" ID="providerEndpoint1" OnAuthenticationChallenge="providerEndpoint1_AuthenticationChallenge" />
</head>
<body>
	<form id="form1" runat="server">
	<div>
	</div>
	</form>
</body>
</html>
