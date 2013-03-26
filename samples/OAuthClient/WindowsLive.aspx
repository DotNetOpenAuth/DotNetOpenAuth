<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WindowsLive.aspx.cs" Inherits="OAuthClient.WindowsLive" Async="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<asp:Panel runat="server" ID="localhostDoesNotWorkPanel" Visible="False">
		<p>
			Windows Live requires a public domain (not localhost) that matches the registered
			client app's callback URL. You can either host this sample on a public URL and register
			your own client, or you can modify your "%windows%\system32\drivers\etc\hosts" file
			to temporarily add this entry:
		</p>
		<pre>127.0.0.1 samples.dotnetopenauth.net</pre>
		<p>
			Then access this sample via this url:
			<asp:HyperLink ID="publicLink" NavigateUrl="http://samples.dotnetopenauth.net:59722/WindowsLive.aspx"
				runat="server">http://samples.dotnetopenauth.net:59722/WindowsLive.aspx</asp:HyperLink></p>
	</asp:Panel>
	<div>
		Welcome,
		<asp:Label Text="[name]" ID="nameLabel" runat="server" />
	</div>
	</form>
</body>
</html>
