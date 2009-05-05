<%@ Page Title="RP Infocard PPID display" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="InfoCardPpid.aspx.cs" Inherits="RP_InfoCardPpid"
	ValidateRequest="false" EnableViewStateMac="false" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<table>
				<tr>
					<td nowrap="nowrap">Source URL:
						<asp:DropDownList ID="sourceDropDown" runat="server" onchange="window.location = this.value" />
					</td>
					<td nowrap="nowrap">Form action:
						<asp:DropDownList ID="actionDropDown" runat="server" onchange="document.getElementById('formActionSpan').innerHTML = document.forms[0].action = this.value;">
							<asp:ListItem Value="https://test-id.org/RP/InfoCardPpid.aspx" Text="Valid Test endpoint GoDaddy.com Class2" />
							<asp:ListItem Value="https://test-id.net/RP/InfoCardPpid.aspx" Text="Revoked Test endpoint GoDaddy.com Class2" />
							<asp:ListItem Value="http://test-id.net/RP/InfoCardPpid.aspx" Text="No SSL Test endpoint" />
							<asp:ListItem Value="https://expired.test-id.net:444/RP/InfoCardPpid.aspx" Text="Expired Test Endpoint VeriSign" />
							<asp:ListItem Value="https://start-class1.test-id.net:8443/RP/InfoCardPpid.aspx"
								Text="Valid Test endpoint StartSSL Class1" />
							<asp:ListItem Value="https://start-class2.test-id.net:8444/RP/InfoCardPpid.aspx"
								Text="Valid Test endpoint StartSSL Class2" />
							<asp:ListItem Value="https://test-id.info/RP/InfoCardPpid.aspx" Text="Valid Test endpoint CAcert Class1" />
							<asp:ListItem Value="https://wild-card.test-id.net:8445/RP/InfoCardPpid.aspx" Text="Valid Wild Card endpoint StartSSL Class2" />
						</asp:DropDownList>
						<span id="formActionSpan"></span></td>
				</tr>
			</table>
			<ic:InfoCardSelector ID="InfoCardSelector1" runat="server" Issuer="" OnReceivedToken="InfoCardSelector1_ReceivedToken"
				OnTokenProcessingError="InfoCardSelector1_TokenProcessingError">
				<ClaimsRequested>
					<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier" />
				</ClaimsRequested>
				<UnsupportedTemplate>
					<p>Your browser does not support Information Cards.</p>
				</UnsupportedTemplate>
			</ic:InfoCardSelector>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<p>Received InfoCard! </p>
			<table>
				<tr>
					<td>Site-specific identifier </td>
					<td><asp:Label ID="siteSpecificIdentifierLabel" runat="server" /> </td>
				</tr>
				<tr>
					<td>Ppid </td>
					<td><asp:Label ID="ppidLabel" runat="server" /> </td>
				</tr>
				<tr>
					<td>Issuer public key hash </td>
					<td><asp:Label ID="issuerPubKeyHashLabel" runat="server" /> </td>
				</tr>
				<tr>
					<td>UniqueID </td>
					<td><asp:Label ID="uniqueIdLabel" runat="server" /> </td>
				</tr>
			</table>
		</asp:View>
		<asp:View ID="View3" runat="server">
			<p>An error occurred while processing the token: <asp:Label ID="processingErrorLabel"
				runat="server" ForeColor="Red" /> </p>
			<p>Unprocessed token: <asp:Label ID="unprocessedTokenLabel" runat="server" /> </p>
		</asp:View>
	</asp:MultiView>
</asp:Content>
