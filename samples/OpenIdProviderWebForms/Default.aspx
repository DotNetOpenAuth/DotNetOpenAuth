<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="Default.aspx.cs"
	Inherits="OpenIdProviderWebForms._default" Async="true" %>

<%@ Import Namespace="OpenIdProviderWebForms.Code" %>
<%@ Register Assembly="DotNetOpenAuth.OpenId.UI" Namespace="DotNetOpenAuth.OpenId" TagPrefix="openid" %>
<%@ Register Assembly="DotNetOpenAuth.OpenId.UI" Namespace="DotNetOpenAuth" TagPrefix="openauth" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
	<openauth:XrdsPublisher runat="server" XrdsUrl="~/op_xrds.aspx" />

	<script language="javascript">
		String.prototype.startsWith = function(substring) {
			if (this.length < substring.length) {
				return false;
			}
			return this.substring(0, substring.length) == substring;
		};

		function updateBookmark(rpRealm) {
			if (!(rpRealm.startsWith("http://") || rpRealm.startsWith("https://"))) {
				rpRealm = "http://" + rpRealm;
			}

			var bookmarkUrl = document.location + "?rp=" + encodeURIComponent(rpRealm);
			var bookmarkParagraph = document.getElementById('bookmarkParagraph');
			var bookmark = document.getElementById('bookmark');
			bookmarkParagraph.style.display = '';
			bookmark.href = bookmarkUrl;
			bookmark.innerHTML = bookmarkUrl;
		}
	</script>

</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Main">
	<h2>Provider </h2>
	<p>Welcome. This site doesn't do anything more than simple authentication of users.
		Start the authentication process on the Relying Party sample site, or log in here
		and send an unsolicited assertion. </p>
	<asp:LoginView runat="server" ID="loginView">
		<LoggedInTemplate>
			<asp:Panel runat="server" DefaultButton="sendAssertionButton">
				<p>You're logged in as <b>
					<%= HttpUtility.HtmlEncode(User.Identity.Name) %></b> </p>
				<p>Your claimed identifier is <b>
					<%= HttpUtility.HtmlEncode(Util.BuildIdentityUrl()) %></b> </p>
				<p>Since you're logged in, try sending an unsolicited assertion to an OpenID 2.0 relying
					party site. Just type in the URL to the site's home page. This could be the sample
					relying party web site. </p>
				<div>
					<asp:TextBox runat="server" ID="relyingPartySite" Columns="40" onchange="updateBookmark(this.value)"
						onkeyup="updateBookmark(this.value)" />
					<asp:Button runat="server" ID="sendAssertionButton" Text="Login" OnClick="sendAssertionButton_Click" />
					<asp:RequiredFieldValidator runat="server" ControlToValidate="relyingPartySite" Text="Specify relying party site first" />
				</div>
				<p id="bookmarkParagraph" style="display: none">Bookmark <a id="bookmark"></a>&nbsp;so
					you can log into the RP automatically in the future.</p>
				<p>An unsolicited assertion is a way to log in to a relying party site directly from
					your OpenID Provider. </p>
				<p><asp:Label runat="server" EnableViewState="false" Visible="false" ID="errorLabel"
					ForeColor="Red" /> </p>
			</asp:Panel>
		</LoggedInTemplate>
	</asp:LoginView>
	<asp:LoginStatus runat="server" />
</asp:Content>
