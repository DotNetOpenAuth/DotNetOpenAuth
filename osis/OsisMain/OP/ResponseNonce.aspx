<%@ Page Title="OP sends properly formatted response_nonce" Language="C#" MasterPageFile="~/OP/ProviderTests.master"
	AutoEventWireup="true" CodeFile="ResponseNonce.aspx.cs" Inherits="OP_ResponseNonce" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<asp:Panel ID="Panel1" runat="server" DefaultButton="beginButton">
				<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" /> <asp:TextBox ID="identifierBox"
					runat="server" />
				<asp:Button ID="beginButton" runat="server" Text="Begin" OnClick="beginButton_Click" />
				<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="identifierBox"
					ErrorMessage="Enter an identifier first." />
				<asp:Label ID="errorLabel" runat="server" EnableViewState="False" ForeColor="Red"
					Visible="False" />
			</asp:Panel>
			<p><b>Instructions: </b>Complete a successful login using an OpenID 2.0 provider.</p>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
