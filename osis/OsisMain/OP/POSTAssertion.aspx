<%@ Page Title="OP sends large assertions as POST" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="POSTAssertion.aspx.cs" Inherits="OP_POSTAssertion" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<div>
				<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" />
				<rp:OpenIdTextBox runat="server" ID="openIdTextBox" OnCanceled="openIdTextBox_Response"
					OnFailed="openIdTextBox_Response" OnLoggedIn="openIdTextBox_Response" />
				<asp:Button ID="beginButton" runat="server" Text="Begin" OnClick="beginButton_Click" />
				<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="openIdTextBox"
					ErrorMessage="Enter an identifier first." Display="Dynamic" />
				<asp:Label ID="errorLabel" runat="server" EnableViewState="False" ForeColor="Red"
					Visible="False" />
			</div>
			<div>
				<asp:Label AssociatedControlID="callbackArgumentSize" ID="Label2" runat="server"
					Text="Inflate request size by:" /> <asp:TextBox Columns="4" MaxLength="4" ID="callbackArgumentSize"
						Text="900" runat="server" /> (this should be about 100 bytes less than the limit
				that causes OP to switch from a GET to a POST when generating its assertion.)
				<asp:RangeValidator ID="RangeValidator1" runat="server" ControlToValidate="callbackArgumentSize"
					MinimumValue="500" MaximumValue="4096" Display="Dynamic" ErrorMessage="Must be a number between 500 and 4096"
					SetFocusOnError="True" Type="Integer" />
				<asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="callbackArgumentSize"
					runat="server" ErrorMessage="(required)" Display="Dynamic" />
					<br />
				<asp:CheckBox runat="server" ID="includeMultibyteCharacters" Text="Include multi-byte UTF-8 characters in callback." />
			</div>
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
