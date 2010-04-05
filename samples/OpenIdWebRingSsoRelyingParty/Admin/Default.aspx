<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OpenIdWebRingSsoRelyingParty.Admin.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<div>
		You must be an admin!
	</div>
	<p>
		The roles you're assigned come from the trusted Provider's identity assertion. The
		sample OP comes hard-wired to assert membership in the Admin and Member roles.
	</p>
	</form>
</body>
</html>
