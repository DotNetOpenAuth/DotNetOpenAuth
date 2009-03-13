<%@ Page Title="OP return_to verification" Language="C#" MasterPageFile="~/OP/ProviderTests.master"
	AutoEventWireup="true" CodeFile="ReturnToVerification.aspx.cs" Inherits="OP_ReturnToVerification" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" /> <asp:TextBox ID="identifierBox"
		runat="server" />
	<asp:Button ID="beginValidButton" runat="server" Text="Begin verifiable RP" OnClick="beginVerifiableButton_Click" />
	<asp:Button ID="beginInvalidButton" runat="server" OnClick="beginUnverifiableButton_Click"
		Text="Begin UNverifiable RP" />
	<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="identifierBox"
		ErrorMessage="Enter an identifier first." Display="Dynamic" />
	<asp:Label ID="errorLabel" runat="server" EnableViewState="False" ForeColor="Red"
		Visible="False" />
	<p><b>Instructions:</b> After entering in your OpenID Identifier, press each of the
		test buttons above. Record whether the OpenID Provider correctly identifies the
		site requesting authentication as verifiable or not. After recording the Provider's
		result, use your browser's Back button to navigate back here. Continuing to login
		will not succeed as that is beyond the scope of the test. </p>
	<p>Relevant sections: 9.2.1, 13.</p>
</asp:Content>
