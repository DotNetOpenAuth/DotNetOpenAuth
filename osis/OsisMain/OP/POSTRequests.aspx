<%@ Page Title="OP accepts POSTed authentication requests" Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true"
	CodeFile="POSTRequests.aspx.cs" Inherits="OP_POSTRequests" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" />
			<rp:OpenIdTextBox runat="server" ID="openIdTextBox" OnCanceled="openIdTextBox_Response"
				OnFailed="openIdTextBox_Response" OnLoggedIn="openIdTextBox_Response" />
			<asp:Button ID="beginButton" runat="server" Text="Begin" OnClick="beginButton_Click" />
			<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="openIdTextBox"
				ErrorMessage="Enter an identifier first." Display="Dynamic" />
			<asp:Label ID="errorLabel" runat="server" EnableViewState="False" ForeColor="Red"
				Visible="False" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
	<h3>Instructions</h3>
	<ol>
		<li>Enter an OpenID Identifier that resolves to the OP endpoint you wish to test.</li>
		<li>Complete any necessary login steps at the OP.</li>
	</ol>
	<h3>Passing criteria</h3>
	<p>This page will display a PASS/FAIL result upon authentication completion. An inability
		to reach the final page would indicate a failure. </p>
</asp:Content>
