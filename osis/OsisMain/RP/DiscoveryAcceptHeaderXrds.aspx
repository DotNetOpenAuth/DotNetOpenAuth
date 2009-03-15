<%@ Page Language="C#" AutoEventWireup="true" ContentType="application/xrds+xml" %><?xml version="1.0" encoding="UTF-8" ?>
<XRDS xmlns="xri://$xrds" >
	<XRD xmlns="xri://$xrd*($v*2.0)">
		<Service>
			<Type>http://specs.openid.net/auth/2.0/signon</Type>
			<Type>http://openid.net/signon/1.0</Type>
			<URI><%=new Uri(Request.Url, Response.ApplyAppPathModifier("~/RP/DiscoveryAcceptHeader.aspx"))%></URI>
		</Service>
	</XRD>
</XRDS>
