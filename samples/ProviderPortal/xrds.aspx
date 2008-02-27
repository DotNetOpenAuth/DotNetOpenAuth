<%@ Page Language="C#" AutoEventWireup="true" Inherits="xrds" Codebehind="xrds.aspx.cs" %><?xml version="1.0" encoding="UTF-8"?>
<xrds:XRDS
    xmlns:xrds="xri://$xrds"
    xmlns:openid="http://openid.net/xmlns/1.0"
    xmlns="xri://$xrd*($v*2.0)">
  <XRD>
    <Service priority="1">
      <Type>http://openid.net/signon/1.0</Type>
      <Type>http://openid.net/sreg/1.0</Type>
      <URI><%=ServerUrl %></URI>
    </Service>
  </XRD>
</xrds:XRDS>
