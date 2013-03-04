<%@ Page Language="C#" AutoEventWireup="true" Inherits="OpenIdProviderWebForms.server" CodeBehind="server.aspx.cs" ValidateRequest="false" Async="true" EnableSessionState="true" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.Provider.UI" Namespace="DotNetOpenAuth.OpenId.Provider" TagPrefix="openid" %>
<html>
<head>
	<title>This is an OpenID server</title>
</head>
<body>
	<form runat='server'>
	<%-- This page provides an example of how to use the ProviderEndpoint control on an ASPX page
	     to host an OpenID Provider.  Alternatively for greater performance an .ashx file can be used.
	     See Provider.ashx for an example. A typical web site will NOT use both .ashx and .aspx 
	     provider endpoints.
	     This server.aspx page is the default provider endpoint to use.  To switch to the .ashx handler,
	     change the user_xrds.aspx and op_xrds.aspx files to point to provider.ashx instead of server.aspx.
	     --%>
	<openid:ProviderEndpoint runat="server" OnAuthenticationChallenge="provider_AuthenticationChallenge" OnAnonymousRequest="provider_AnonymousRequest" />
	<p>
		<asp:Label ID="serverEndpointUrl" runat="server" EnableViewState="false" />
		is an OpenID server endpoint.
	</p>
	<p>
		For more information about OpenID, see:
	</p>
	<table>
		<tr>
			<td>
				<a href="http://dotnetopenauth.net/">http://dotnetopenauth.net/</a>
			</td>
			<td>
				Home of this library
			</td>
		</tr>
		<tr>
			<td>
				<a href="http://www.openid.net/">http://www.openid.net/</a>
			</td>
			<td>
				The official OpenID Web site
			</td>
		</tr>
		<tr>
			<td>
				<a href="http://www.openidenabled.com/">http://www.openidenabled.com/</a>
			</td>
			<td>
				An OpenID community Web site
			</td>
		</tr>
	</table>
	</form>
</body>
</html>
