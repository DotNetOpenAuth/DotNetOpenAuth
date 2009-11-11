<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
	CodeBehind="OAuthAuthorize.aspx.cs" Inherits="WebFormsRelyingParty.Members.OAuthAuthorize" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="server">
	<h2>
		Client authorization
	</h2>
	<p>
		The
		<asp:Label ID="consumerNameLabel" runat="server" Text="(app name)" />
		application is requesting to access the private data in your account here. Is that
		alright with you?
	</p>
	<div style="display: none" id="responseButtonsDiv">
		<asp:Button ID="yesButton" runat="server" Text="Yes" OnClick="yesButton_Click" />
		<asp:Button ID="noButton" runat="server" Text="No" OnClick="noButton_Click" />
		<asp:HiddenField runat="server" ID="csrfCheck" EnableViewState="false" />
	</div>
	<div id="javascriptDisabled">
		<b>Javascript appears to be disabled in your browser. </b>This page requires Javascript
		to be enabled to better protect your security.
	</div>

	<script language="javascript" type="text/javascript">
		//<![CDATA[
		// we use HTML to hide the action buttons and Javascript to show them
		// to protect against click-jacking in an iframe whose javascript is disabled.
		document.getElementById('responseButtonsDiv').style.display = 'block';
		document.getElementById('javascriptDisabled').style.display = 'none';

		// Frame busting code (to protect us from being hosted in an iframe).
		// This protects us from click-jacking.
		if (document.location !== window.top.location) {
			window.top.location = document.location;
		}
		//]]>
	</script>

</asp:Content>
