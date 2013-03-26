<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OpenIdWebRingSsoRelyingParty.Login" Async="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<div>
				Sorry. We couldn't log you in.
			</div>
			<asp:Label runat="server" ID="errorLabel" />
			<p>
				<asp:Button ID="retryButton" runat="server" Text="Try Again" OnClick="retryButton_Click" />
			</p>
		</asp:View>
		<asp:View ID="View2" runat="server">
			You don't have permission to visit <%=HttpUtility.HtmlEncode(Request.QueryString["ReturnUrl"]) %>.
		</asp:View>
	</asp:MultiView>
	</form>
</body>
</html>
