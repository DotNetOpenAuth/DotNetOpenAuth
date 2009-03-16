<%@ Page Language="C#" AutoEventWireup="true" ContentType="application/xrds+xml" %><?xml version="1.0" encoding="UTF-8"?>
<XRDS xmlns="xri://$xrds" xmlns:openid="http://openid.net/xmlns/1.0">
	<XRD xmlns="xri://$xrd*($v*2.0)">
		<Service priority="10">
			<Type>http://specs.openid.net/auth/2.0/signon</Type>
			<Type>http://openid.net/extensions/sreg/1.1</Type>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OpenId/Provider"))%></URI>
		</Service>
		<Service priority="20">
			<Type>http://openid.net/signon/1.0</Type>
			<Type>http://openid.net/extensions/sreg/1.1</Type>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/OpenId/Provider"))%></URI>
		</Service>
	</XRD>
</XRDS>
