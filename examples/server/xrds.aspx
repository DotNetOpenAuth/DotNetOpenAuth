<%@ Page Language="C#" %>
<?xml version="1.0" encoding="UTF-8"?>
<script runat="server">
public string ServerUrl
{
    get {
        String path = Response.ApplyAppPathModifier("~/server.aspx");
        UriBuilder builder = new UriBuilder(Request.Url);
        builder.Path = path;
        builder.Query = null;
        builder.Fragment = null;
        return builder.ToString();
    }
}
void Page_Load (object sender, System.EventArgs evt) {
Response.ContentType = "application/xrds+xml";
}
</script><xrds:XRDS
    xmlns:xrds="xri://$xrds"
    xmlns:openid="http://openid.net/xmlns/1.0"
    xmlns="xri://$xrd*($v*2.0)">
  <XRD>
    <Service priority="1">
      <Type>http://openid.net/signon/1.0</Type>
      <Type>http://openid.net/sreg/1.0</Type>
      <URI><%=ServerUrl%></URI>
    </Service>
  </XRD>
</xrds:XRDS>
