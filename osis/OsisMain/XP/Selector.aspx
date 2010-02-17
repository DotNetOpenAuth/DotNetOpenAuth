<%@ Page Title="Client Selector" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="Selector.aspx.cs" Inherits="XP_Selector" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">

		<object type="application/x-informationcard" name="xmlToken">
			<param name="privacyUrl" value="https://test-id.org/OP/PrivacyPolicy.aspx"/>
			<param Name="protocol" Value="http://specs.openid.net/auth/2.0"/>
			<param name="tokenType" value="http://specs.openid.net/auth/2.0"/>
			<param name="issuer" value="https://me.yahoo.com/ https://www.myopenid.com/ https://www.google.com/accounts/o8/id"/>
			<param name="issuerExclusive" value="false" />
			<param Name="OpenIDAuthParameters" Value="openid.ns:http://specs.openid.net/auth/2.0
openid.return_to:<%= new Uri(Request.Url, Request.Path).AbsoluteUri %>
openid.realm:<%= new Uri(Request.Url, Request.ApplicationPath).AbsoluteUri %>
openid.ns.sreg:http://openid.net/extensions/sreg/1.1
openid.sreg.required:nickname,email
openid.sreg.optional:fullname,country,timezone" />
 		</object>

		<asp:Button runat="server" Text="Log in using your OpenID Selector" />
			
		</asp:View>
		<asp:View ID="View2" runat="server">
			<p>PASS We got a successful auth response!</p>
		</asp:View>
		<asp:View ID="View3" runat="server">
			<p>FAIL An non-successful auth response was received.</p>
			<asp:Label runat="server" id="errorDetails" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
