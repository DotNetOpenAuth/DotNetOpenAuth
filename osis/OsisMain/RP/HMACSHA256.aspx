<%@ Page Title="RP supports HMAC-SHA256 associations" Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true"
	CodeFile="HMACSHA256.aspx.cs" Inherits="RP_HMACSHA256" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<h3>Instructions</h3>
			<ol>
				<li>Visit the RP and log in with this OpenID Identifier:
					<%=new Uri(Request.Url, Request.Url.AbsolutePath) %>
				</li>
			</ol>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
