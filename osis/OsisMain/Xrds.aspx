<%@ Page Language="C#" AutoEventWireup="true" ContentType="application/xrds+xml" %><?xml version="1.0" encoding="UTF-8"?>
<xrds:XRDS
	xmlns:xrds="xri://$xrds"
	xmlns:openid="http://openid.net/xmlns/1.0"
	xmlns="xri://$xrd*($v*2.0)">
	<XRD>
		<Service priority="1">
			<Type>http://specs.openid.net/auth/2.0/return_to</Type>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/AXFetch.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/MultiFactor.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/ResponseNonce.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/ReturnToVerification.Valid.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/RPDiscoveryRealm/"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/DelegatedIdentifierSelect.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/Sreg.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/POSTAssertion.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/POSTRequests.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/IndirectMessageErrors.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/IdentitylessCheckId.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/GSALevel1.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/MaxAuthAge.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/ReplayProtection.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OP/CheckAuthSharedSecret.aspx"))%></URI>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/XP/Selector.aspx"))%></URI>
		</Service>
	</XRD>
</xrds:XRDS>
