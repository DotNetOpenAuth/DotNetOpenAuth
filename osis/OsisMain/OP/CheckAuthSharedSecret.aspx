<%@ Page Title="OP rejects check_auth messages with shared association handles" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="CheckAuthSharedSecret.aspx.cs" Inherits="OP_CheckAuthRejectsSharedAssociationHandles" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" /> <asp:TextBox ID="identifierBox"
				Columns="40" runat="server" />
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
	<h3>Instructions </h3>
	<ol>
		<li>Enter an OpenID Identifier associated with the Provider to test.</li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The Provider must reject the check_auth message sent by the RP with the handle to a shared association. 
	See <a href="http://openid.net/specs/openid-authentication-2_0.html#verifying_signatures">11.4.2.1.  Request Parameters</a>
	from the OpenID 2.0 spec.
	</p>
</asp:Content>
