<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="SampleWcf.aspx.cs" Inherits="SampleWcf" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	
	<asp:Button ID="getAuthorizationButton" runat="server" Text="Get Authorization" 
		onclick="getAuthorizationButton_Click" />
	<br />
	<asp:Button ID="getNameButton" runat="server" Text="Get Name" 
		onclick="getNameButton_Click" />
	<br />
	<asp:Button ID="getAgeButton" runat="server" Text="Get Age" 
		onclick="getAgeButton_Click" />
	
</asp:Content>
