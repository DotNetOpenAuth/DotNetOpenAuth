﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" ContentType="application/xrds+xml" %><?xml version="1.0" encoding="UTF-8"?>
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
			<%-- We use the Authenticate action instead of Login because Authenticate
			     is the action that receives OpenId assertions. --%>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/User/Authenticate"))%></URI>
		</Service>
	</XRD>
</xrds:XRDS>
