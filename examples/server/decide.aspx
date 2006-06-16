<%@ Page Language="C#" Inherits="Janrain.OpenId.Server.Asp.OpenIdDecisionPage"%>
<html>
  <head>
    <title>Approve OpenID request?</title>
  </head>
  <body>
      <p>
        A site has asked for your identity.  If you approve, the
        site represented by the trust root below will be told that you
        control identity URL listed below. (If you are using a
        delegated identity, the site will take care of reversing the
        delegation on its own.)
      </p>
      <table>
        <tr><td>Identity:</td><td><%=idrequest.IdentityUrl%></td></tr>
        <tr><td>Trust Root:</td><td><%=idrequest.TrustRoot%></td></tr>
      </table>
      <p>Allow this authentication to proceed?</p>

      <form runat="server">
        <asp:button id=yes_button onclick="Yes_Click" text="  yes  "
                    cssclass="button" runat="Server"/>
        <asp:button id=no_button onclick="No_Click" text="  no  "
                    cssclass="button" runat="Server"/>
      </form>
  </body>
</html>