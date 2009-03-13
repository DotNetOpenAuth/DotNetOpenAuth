<%@ Page Title="OP offers fully SSL-protected authentication" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="RequireSsl.aspx.cs" Inherits="OP_RequireSsl" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<rp:OpenIdLogin ID="OpenIdBox" RequireSsl="true" runat="server" OnLoggingIn="OpenIdBox_LoggingIn"
			OnFailed="OpenIdBox_Failed"
				ButtonText="Begin" ExamplePrefix="" ExampleUrl="" LabelText="OpenID Identifier:"
				RegisterVisible="False" TabIndex="1" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
	<h3>
		Instructions:
	</h3>
	<ol>
		<li>Enter an HTTPS OpenID Identifier. </li>
	</ol>
</asp:Content>
