<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebFormsRelyingParty.Login"
	MasterPageFile="~/Site.Master" ValidateRequest="false" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<asp:Content runat="server" ContentPlaceHolderID="Body">
	<h2>Login</h2>
	<iframe src="LoginFrame.aspx" frameborder="0" width="355" height="270"></iframe>
</asp:Content>
