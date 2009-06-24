<%@ Page Title="" Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true" CodeFile="GSALevel1.aspx.cs" Inherits="OP_GSALevel1" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Src="~/OP/ProfileFieldsDisplay.ascx" TagPrefix="cc1" TagName="ProfileFieldsDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" Runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<rp:OpenIdLogin ID="OpenIdBox" runat="server" OnLoggedIn="OpenIdBox_LoggedIn" OnLoggingIn="OpenIdBox_LoggingIn"
				ButtonText="Begin" ExamplePrefix="" ExampleUrl="" LabelText="OpenID Identifier:"
				RegisterVisible="False" TabIndex="1" EnableRequestProfile="False" RequestBirthDate="Request" RequestCountry="Request" RequestEmail="Request" RequestFullName="Request" RequestLanguage="Request" RequestGender="Request" RequestNickname="Request" RequestPostalCode="Request" RequestTimeZone="Request" />
			<br />
			<asp:CheckBox ID="includePii" Text="Ask for PII" runat="server" 
				oncheckedchanged="includePii_CheckedChanged" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
			<asp:Panel runat="server" ID="sregResponsePanel">
				<p>
					In addition to authenticating you, your OpenID Provider may
					have told us something about you using the 
					Simple Registration extension:
				</p>
				<cc1:ProfileFieldsDisplay runat="server" ID="profileFieldsDisplay" />
			</asp:Panel>
		</asp:View>
	</asp:MultiView>
	<h3>
		Instructions:
	</h3>
	<ol>
		<li>Enter the OP Identifier above and click Begin. </li>
	</ol>
</asp:Content>

