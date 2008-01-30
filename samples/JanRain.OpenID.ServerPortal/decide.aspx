<%@ Page Language="C#" AutoEventWireup="true" CodeFile="decide.aspx.cs" Inherits="decide" Debug="true" %>

<%@ Register Src="ProfileFields.ascx" TagName="ProfileFields" TagPrefix="uc1" %>
<html>
<head>
    <title>Approve OpenID request?</title>
</head>
<body>
<form id="Form1" runat="server">
    <p>
        A site has asked for your identity. If you approve, the site represented by the trust root below will be told that you control identity URL listed below. (If you are using a delegated identity, the site will take care of reversing the delegation on its own.)
    </p>
    <table>
        <tr>
            <td>
                Identity:</td>
            <td>
                <%=State.Session.LastRequest.IdentityUrl%>
            </td>
        </tr>
        <tr>
            <td>
                Trust Root:</td>
            <td>
                <%=State.Session.LastRequest.TrustRoot%>
            </td>
        </tr>
    </table>
    <p>
        Allow this authentication to proceed?</p>
    <p>
        
            <uc1:ProfileFields ID="profileFields" runat="server" Visible="false" />
        &nbsp;</p>
    <asp:Button ID="yes_button" OnClick="Yes_Click" Text="  yes  " CssClass="button" runat="Server" />
    <asp:Button ID="no_button" OnClick="No_Click" Text="  no  " CssClass="button" runat="Server" />
    </form>
</body>
</html>
