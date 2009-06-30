<%@ Page Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true"
	CodeFile="GSAInfoCard.aspx.cs" Inherits="OP_GSAInfoCard" Title="Managed Card issuer presents GSA-compliant Card" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<ic:InfoCardSelector ID="InfoCardLevel1" runat="server" OnReceivedToken="InfoCardLevel1_ReceivedToken"
				Issuer="" PrivacyUrl="https://test-id.org/OP/PrivacyPolicy.aspx" PrivacyVersion="1">
				<ClaimsRequested>
					<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier" />
					<ic:ClaimType Name="http://schemas.informationcard.net/@ics/icam-assurance-level-1/2009-06" />
				</ClaimsRequested>
				<UnsupportedTemplate>
					<p>This test includes an InfoCard component, but it doesn't look like your browser
						has an InfoCard Selector.</p>
				</UnsupportedTemplate>
			</ic:InfoCardSelector>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<p>We have received your InfoCard.</p>
			<table>
				<tr>
					<td>Site specific ID </td>
					<td><asp:Label ID="siteSpecificIdLabel" runat="server" Font-Bold="true" /> </td>
				</tr>
			</table>
		</asp:View>
	</asp:MultiView>
	<h3>Instructions</h3>
	<ol>
		<li>Click the InfoCard button and select a managed card that is issued by the IdP
			you are testing. </li>
	</ol>
	<p>One possible InfoCard issuer to try is <a href="https://cardpress.azigo.net/azigo-cardpress/register.do">
		Azigo</a></p>
</asp:Content>
