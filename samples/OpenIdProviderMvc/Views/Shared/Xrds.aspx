<%@ Page Language="C#" AutoEventWireup="true" ContentType="application/xrds+xml" %>
<%@ OutputCache Duration="86400" VaryByParam="none" Location="Any" %><?xml version="1.0" encoding="UTF-8"?>
<%--
This XRDS view is used for both the OP identifier and the user identity pages.
Only a couple of conditional checks are required to share the view, but sharing the view
makes it very easy to ensure that all the Type URIs that this server supports are included
for all XRDS discovery.
--%>
<xrds:XRDS
	xmlns:xrds="xri://$xrds"
	xmlns:openid="http://openid.net/xmlns/1.0"
	xmlns="xri://$xrd*($v*2.0)">
	<XRD>
		<Service priority="10">
<% if (ViewData["OPIdentifier"] != null) { %>
			<Type>http://specs.openid.net/auth/2.0/server</Type>
<% } else { %>
			<Type>http://specs.openid.net/auth/2.0/signon</Type>
<% } %>
			<Type>http://openid.net/extensions/sreg/1.1</Type>
			<Type>http://axschema.org/contact/email</Type>
			
			<%--
			Add these types when and if the Provider supports the respective aspects of the UI extension.
			<Type>http://specs.openid.net/extensions/ui/1.0/mode/popup</Type>
			<Type>http://specs.openid.net/extensions/ui/1.0/lang-pref</Type>
			<Type>http://specs.openid.net/extensions/ui/1.0/icon</Type>--%>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OpenId/Provider"))%></URI>
		</Service>
<% if (ViewData["OPIdentifier"] == null) { %>
		<Service priority="20">
			<Type>http://openid.net/signon/1.0</Type>
			<Type>http://openid.net/extensions/sreg/1.1</Type>
			<Type>http://axschema.org/contact/email</Type>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OpenId/Provider"))%></URI>
		</Service>
<% } %>
	</XRD>
</xrds:XRDS>
