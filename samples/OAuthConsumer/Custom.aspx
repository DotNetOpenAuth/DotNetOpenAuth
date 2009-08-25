<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="Custom.aspx.cs" Inherits="Custom" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<h2>Custom setup</h2>
			<p>This allows you to authorize against a custom MVC provider.</p>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<h2>Get Data</h2>
			<asp:Button ID="getName" runat="server" Text="Get Name" OnClick="getName_Click" />
			<br /><br />
			<asp:Button ID="getAge" runat="server" Text="Get Age" OnClick="getAge_Click" />
			<br /><br />
			<asp:Button ID="getFavoriteSites" runat="server" Text="Get Fav Sites" OnClick="getFavoriteSites_Click" />			
			<h3>Result</h3>
            <asp:PlaceHolder runat="server" ID="resultsPlaceholder" />
            
            <h4>Json Responses</h4>
			<asp:Button ID="Button1" runat="server" Text="Get Name" OnClick="getNameAsJson_Click" />
			<br /><br />
			<asp:Button ID="Button2" runat="server" Text="Get Age" OnClick="getAgeAsJson_Click" />
			<br /><br />
			<asp:Button ID="Button3" runat="server" Text="Get Fav Sites" OnClick="getFavoriteSitesAsJson_Click" />			
			<h3>Result</h3>
           <textarea id="resultsAsJsonPlaceholder" runat="server"></textarea>
		</asp:View>
	</asp:MultiView>
</asp:Content>
