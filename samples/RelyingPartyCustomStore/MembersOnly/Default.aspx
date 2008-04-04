<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Welcome OpenID User!</title>
</head>
<body>
	<form id="form1" runat="server">
	<h1>
		Members Only Area
	</h1>
	<p>
		Congratulations, <b>
			<asp:LoginName ID="LoginName1" runat="server" />
		</b>. You have completed the OpenID login process.
	</p>
	<asp:LoginStatus ID="LoginStatus1" runat="server" />
	</form>
</body>
</html>
