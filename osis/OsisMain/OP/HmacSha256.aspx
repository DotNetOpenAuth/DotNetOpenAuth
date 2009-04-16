<%@ Page Title="OP supports HMAC-SHA256 associations" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="HmacSha256.aspx.cs" Inherits="OP_HmacSha256" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" /> <asp:TextBox ID="identifierBox"
				runat="server" />
			<asp:Button ID="beginButton" runat="server" Text="Begin" OnClick="beginButton_Click" />
			<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="identifierBox"
				ErrorMessage="Enter an identifier first." Display="Dynamic" />
			<asp:Label ID="errorLabel" runat="server" EnableViewState="False" ForeColor="Red"
				Visible="False" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
	<h3>Instructions: </h3>
	<ol>
		<li>Enter an OpenID Identifier. </li>
	</ol>
</asp:Content>
