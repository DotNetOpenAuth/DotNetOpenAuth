<%@ Page Language="VB" AutoEventWireup="true" ContentType="application/xrds+xml" %><?xml version="1.0" encoding="UTF-8"?>
<%--
This page is a required for relying party discovery per OpenID 2.0.
It allows Providers to call back to the relying party site to confirm the
identity that it is claiming in the realm and return_to URLs.
This page should be pointed to by the 'realm' home page, which in this sample
is default.aspx.
--%>
<xrds:XRDS
	xmlns:xrds="xri://$xrds"
	xmlns:openid="http://openid.net/xmlns/1.0"
	xmlns="xri://$xrd*($v*2.0)">
	<XRD>
		<Service priority="1">
			<Type>http://specs.openid.net/auth/2.0/return_to</Type>
			<%-- Every page with an OpenID login should be listed here. --%>
			<URI priority="1"><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/login.aspx"))%></URI>
			<URI priority="2"><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/loginProgrammatic.aspx"))%></URI>
			<URI priority="3"><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/ajaxlogin.aspx"))%></URI>
			<URI priority="4"><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/NoIdentityOpenId.aspx"))%></URI>
			<URI priority="5"><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/loginPlusOAuth.aspx"))%></URI>
			<URI priority="6"><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/loginPlusOAuthSampleOP.aspx"))%></URI>
		</Service>
		<Service>
			<Type>http://specs.openid.net/extensions/ui/icon</Type>
			<URI><%=New Uri(Request.Url, Response.ApplyAppPathModifier("~/images/DotNetOpenAuth.png"))%></URI>
		</Service>
	</XRD>
</xrds:XRDS>
