<%@ Page Language="C#" AutoEventWireup="true" Async="true"
	Inherits="OAuthConsumer.SignInWithTwitter" Codebehind="SignInWithTwitter.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Sign-in with Twitter</title>
</head>
<body>
	<form id="form1" runat="server">
	<div>
		<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
			<asp:View ID="View1" runat="server">
				<h2>
					Twitter setup</h2>
				<p>
					A Twitter client app must be endorsed by a Twitter user.
				</p>
				<ol>
					<li><a target="_blank" href="https://twitter.com/oauth_clients">Visit Twitter and create
						a client app</a>. </li>
					<li>Modify your web.config file to include your consumer key and consumer secret.</li>
				</ol>
			</asp:View>
			<asp:View ID="View2" runat="server">
				<asp:ImageButton ImageUrl="~/images/Sign-in-with-Twitter-darker.png" runat="server"
					AlternateText="Sign In With Twitter" ID="signInButton" OnClick="signInButton_Click" />
				<asp:CheckBox Text="force re-login" runat="server" ID="forceLoginCheckbox" />
				<br />
				<asp:Panel runat="server" ID="loggedInPanel" Visible="false">
					Now logged in as
					<asp:Label Text="[name]" runat="server" ID="loggedInName" />
				</asp:Panel>
			</asp:View>
		</asp:MultiView>
	</div>
	</form>
</body>
</html>
