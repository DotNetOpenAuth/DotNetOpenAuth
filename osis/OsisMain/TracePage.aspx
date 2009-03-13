<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TracePage.aspx.cs" Inherits="TracePage"
	MasterPageFile="~/MasterPage.master" %>

<asp:Content runat="server" ContentPlaceHolderID="Body">
	<p align="right">
		<asp:Button runat="server" Text="Clear log" ID="clearLogButton" OnClick="clearLogButton_Click" />
	</p>
	<pre><asp:PlaceHolder runat="server" ID="placeHolder1" /></pre>
</asp:Content>
