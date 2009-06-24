<%@ Page Language="C#" AutoEventWireup="true" ContentType="application/xrds+xml" %><?xml version="1.0" encoding="UTF-8" ?>
<XRDS xmlns="xri://$xrds" >
	<XRD xmlns="xri://$xrd*($v*2.0)">
		<Service>
			<Type>http://specs.openid.net/auth/2.0/server</Type>
			<Type>http://openid.net/extensions/sreg/1.1</Type>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/RP/GSALevel1.aspx"))%></URI>
		</Service>
	</XRD>
</XRDS>
