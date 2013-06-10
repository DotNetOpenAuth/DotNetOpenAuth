<%@ Page Language="C#" AutoEventWireup="true" ContentType="application/xrds+xml" %><?xml version="1.0" encoding="UTF-8"?>
<%@ Import Namespace="DotNetOpenAuth.Messaging" %>
<%--
This page is a required as part of the service discovery phase of the openid 
protocol (step 1). It simply renders the xml for doing service discovery of 
server.aspx using the xrds mechanism. 
This XRDS doc is discovered via the user.aspx page.
--%>
<xrds:XRDS
	xmlns:xrds="xri://$xrds"
	xmlns:openid="http://openid.net/xmlns/1.0"
	xmlns="xri://$xrd*($v*2.0)">
	<XRD>
		<Service priority="10">
			<Type>http://specs.openid.net/auth/2.0/signon</Type>
			<Type>http://openid.net/extensions/sreg/1.1</Type>
			<URI><%=new Uri(new HttpRequestWrapper(Request).GetPublicFacingUrl(), Response.ApplyAppPathModifier("~/server.aspx"))%></URI>
		</Service>
		<Service priority="20">
			<Type>http://openid.net/signon/1.0</Type>
			<Type>http://openid.net/extensions/sreg/1.1</Type>
			<URI><%=new Uri(new HttpRequestWrapper(Request).GetPublicFacingUrl(), Response.ApplyAppPathModifier("~/server.aspx"))%></URI>
		</Service>
	</XRD>
</xrds:XRDS>
