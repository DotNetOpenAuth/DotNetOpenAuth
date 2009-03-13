<%@ Page Title="OP return_to verification" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="ReturnToVerification.aspx.cs" Inherits="OP_ReturnToVerification" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" /> <asp:TextBox ID="identifierBox"
		runat="server" />
	<asp:Button ID="beginValidButton" runat="server" Text="Begin verifiable RP" OnClick="beginVerifiableButton_Click" />
	<asp:Button ID="beginValid2Button" runat="server" OnClick="beginValid2Button_Click"
		Text="Begin verifiable RP (realm style)" />
	<asp:Button ID="beginInvalidButton" runat="server" OnClick="beginUnverifiableButton_Click"
		Text="Begin UNverifiable RP" />
	<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="identifierBox"
		ErrorMessage="Enter an identifier first." Display="Dynamic" />
	<asp:Label ID="errorLabel" runat="server" EnableViewState="False" ForeColor="Red"
		Visible="False" />
	<h3>Instructions </h3>
	<ol>
		<li>Enter your OpenID Identifier to an OpenID 2.0 provider. </li>
		<li>Click each of the buttons above in sequence. </li>
		<li>Record whether the OpenID Provider correctly identifies the site requesting authentication
			as verifiable or not. </li>
	</ol>
	<p>Relevant sections: 9.2.1, 13.</p>
</asp:Content>
