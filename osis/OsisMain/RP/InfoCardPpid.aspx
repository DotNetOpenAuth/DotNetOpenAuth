<%@ Page Title="RP Infocard PPID display" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="InfoCardPpid.aspx.cs" Inherits="RP_InfoCardPpid"
	ValidateRequest="false" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<ic:InfoCardSelector ID="InfoCardSelector1" runat="server" OnReceivedToken="InfoCardSelector1_ReceivedToken">
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
	</asp:MultiView>
</asp:Content>
