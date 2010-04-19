<%@ Page Title="DotNetOpenAuth Service Provider Sample" Language="C#" MasterPageFile="~/MasterPage.master" CodeBehind="~/Default.aspx.cs" Inherits="OAuthServiceProvider._Default" AutoEventWireup="True" %>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data.SqlClient" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:Button ID="createDatabaseButton" runat="server" Text="(Re)create Database" OnClick="createDatabaseButton_Click" />
	<asp:Label runat="server" ID="databaseStatus" EnableViewState="false" Text="Database recreated!"
		Visible="false" />
	<p>Note that to be useful, you really need to either modify the database to add an
		account with data that will be accessed by this sample, or modify this very page
		to inject that data into the database. </p>
</asp:Content>
