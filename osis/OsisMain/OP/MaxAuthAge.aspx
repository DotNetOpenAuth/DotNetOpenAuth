<%@ Page Title="OP supports the PAPE extension's max_auth_age parameter" Language="C#"
	MasterPageFile="~/TestMaster.master" AutoEventWireup="true" CodeFile="MaxAuthAge.aspx.cs"
	Inherits="OP_MaxAuthAge" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<rp:OpenIdLogin ID="OpenIdBox" runat="server" OnLoggedIn="OpenIdBox_LoggedIn" OnLoggingIn="OpenIdBox_LoggingIn"
				ButtonText="Begin" ExamplePrefix="" ExampleUrl="" LabelText="OpenID Identifier:"
				RegisterVisible="False" TabIndex="1" />
			<br />
			<b>max_auth_age</b>: <asp:TextBox ID="maxAuthAgeBox" runat="server" Columns="4" MaxLength="4" Text="0" />
			(seconds)
			<asp:RequiredFieldValidator runat="server" ID="maxAuthAgeRequired" ControlToValidate="maxAuthAgeBox"
				ErrorMessage="required" Display="Dynamic" />
			<asp:RangeValidator runat="server" ID="maxAuthAgeRange" ControlToValidate='maxAuthAgeBox'
				Type="Integer" MinimumValue="0" MaximumValue="9999999" Display="Dynamic" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
	<h3>Instructions: </h3>
	<ol>
		<li>Log into your Provider if you are not already logged in. </li>
		<li>Enter your OpenID Identifier above and a value for max_auth_age, and click Begin.
		</li>
	</ol>
	<h3>Passing criteria: </h3>
	<p>A Provider passes this test if the value supplied for max_auth_age is larger than
		the actual authentication age, and the Provider forces you to log in again and reports
		this to the RP. </p>
</asp:Content>
