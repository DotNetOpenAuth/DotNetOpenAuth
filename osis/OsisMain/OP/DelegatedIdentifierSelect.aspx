<%@ Page Title="OP Asserts new claimed_id if doing identifier Select and is delegated to from a 3rd party"
	Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true"
	CodeFile="DelegatedIdentifierSelect.aspx.cs" Inherits="OP_DelegatedIdentifierSelect" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
				<asp:Label ID="Label1" runat="server" Text="OpenID Identifier:" /> <asp:TextBox ID="identifierBox"
					runat="server" />
				<asp:Button ID="beginButton" runat="server" Text="Begin" OnClick="beginButton_Click" />
				<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="identifierBox"
					ErrorMessage="Enter an identifier first." />
			<asp:Label ID="opIdentifierRequired" runat="server" Text="You must enter an OP Identifier."
				ForeColor="Red" EnableViewState="false" Visible="false" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
	<h3>Instructions: </h3>
	<ol>
		<li>Enter an OP Identifier and click Begin. </li>
	</ol>
	<h3>Passing criteria </h3>
	<p>If the Provider displays an error it PASSes the test.</p>
</asp:Content>
