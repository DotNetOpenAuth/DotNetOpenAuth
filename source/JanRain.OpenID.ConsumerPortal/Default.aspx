<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Open-ID Consumer</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            Welcome: <b>
                <%=User.Identity.Name %>
            </b>to this open-id consumer<br />
            <br />
            <table id="profileFieldsTable" runat="server">
                <tr>
                    <td style="width: 131px; height: 26px">
                        Nickname
                    </td>
                    <td style="width: 300px; height: 26px">
                        <%=State.ProfileFields.Nickname %>
                    </td>
                </tr>
                <tr>
                    <td style="width: 131px">
                        Email
                    </td>
                    <td style="width: 300px">
                        <%=State.ProfileFields.Email%>
                    </td>
                </tr>
                <tr>
                    <td style="width: 131px">
                        Fullname
                    </td>
                    <td style="width: 300px">
                        <%=State.ProfileFields.Fullname%>
                    </td>
                </tr>
                <tr>
                    <td style="width: 131px; height: 24px">
                        Date of Birth
                    </td>
                    <td style="width: 300px; height: 24px">
                        <%=State.ProfileFields.Birthdate.ToString()%>
                    </td>
                </tr>
                <tr>
                    <td style="width: 131px; height: 24px">
                        Gender
                    </td>
                    <td style="width: 300px; height: 24px">
                        <%=State.ProfileFields.Gender.ToString()%>
                    </td>
                </tr>
                <tr>
                    <td style="width: 131px; height: 26px">
                        Post Code
                    </td>
                    <td style="width: 300px; height: 26px">
                        <%=State.ProfileFields.PostalCode%>
                        &nbsp;</td>
                </tr>
                <tr>
                    <td style="width: 131px">
                        Country
                    </td>
                    <td style="width: 300px">
                        <%=State.ProfileFields.Country%>
                    </td>
                </tr>
                <tr>
                    <td style="width: 131px">
                        Language
                    </td>
                    <td style="width: 300px">
                        <%=State.ProfileFields.Language%>
                    </td>
                </tr>
                <tr>
                    <td style="width: 131px">
                        Timezone&nbsp;
                    </td>
                    <td style="width: 300px">
                        <%=State.ProfileFields.TimeZone%>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
