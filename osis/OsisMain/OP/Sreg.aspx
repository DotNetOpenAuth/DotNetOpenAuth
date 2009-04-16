<%@ Page Title="OP Simple Registration" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="Sreg.aspx.cs" Inherits="OP_Sreg" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<rp:OpenIdLogin ID="OpenIdBox" runat="server" ButtonText="Begin" ExamplePrefix=""
				EnableRequestProfile="false" ExampleUrl="" LabelText="OpenID Identifier:" RegisterVisible="False"
				OnLoggedIn="OpenIdBox_LoggedIn" OnLoggingIn="OpenIdBox_LoggingIn" />
			<table id="Table1" runat="server">
			<tr>
				<td>Include privacy policy </td>
				<td>
					<asp:CheckBox ID="privacyPolicyCheckbox" Checked="true" runat="server" />
				</td>
			</tr>
				<tr>
					<td>Nickname </td>
					<td>
						<asp:DropDownList ID="nickNameList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>Email </td>
					<td>
						<asp:DropDownList ID="emailList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>FullName </td>
					<td>
						<asp:DropDownList ID="fullNameList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>Date of Birth </td>
					<td>
						<asp:DropDownList ID="dateOfBirthList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>Gender </td>
					<td>
						<asp:DropDownList ID="genderList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>Postal Code </td>
					<td>
						<asp:DropDownList ID="postalCodeList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>Country </td>
					<td>
						<asp:DropDownList ID="countryList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>Language </td>
					<td>
						<asp:DropDownList ID="languageList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td>Timezone </td>
					<td>
						<asp:DropDownList ID="timeZoneList" runat="server">
							<asp:ListItem>Not requested</asp:ListItem>
							<asp:ListItem Selected="true">Optional</asp:ListItem>
							<asp:ListItem>Required</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
			</table>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<% if (SregResponse != null) { %>
			<p>In addition to authenticating you, your OpenID Provider may have told us something
				about you using the Simple Registration extension: </p>
			<table id="profileFieldsTable" runat="server">
				<tr>
					<td>Nickname </td>
					<td>
						<%=SregResponse.Nickname %>
					</td>
				</tr>
				<tr>
					<td>Email </td>
					<td>
						<%=SregResponse.Email%>
					</td>
				</tr>
				<tr>
					<td>FullName </td>
					<td>
						<%=SregResponse.FullName%>
					</td>
				</tr>
				<tr>
					<td>Date of Birth </td>
					<td>
						<%=SregResponse.BirthDate.ToString()%>
					</td>
				</tr>
				<tr>
					<td>Gender </td>
					<td>
						<%=SregResponse.Gender.ToString()%>
					</td>
				</tr>
				<tr>
					<td>Postal Code </td>
					<td>
						<%=SregResponse.PostalCode%>
					</td>
				</tr>
				<tr>
					<td>Country </td>
					<td>
						<%=SregResponse.Country%>
					</td>
				</tr>
				<tr>
					<td>Language </td>
					<td>
						<%=SregResponse.Language%>
					</td>
				</tr>
				<tr>
					<td>Timezone </td>
					<td>
						<%=SregResponse.TimeZone%>
					</td>
				</tr>
			</table>
			<% } %>
		</asp:View>
	</asp:MultiView>
	<h3>Instructions </h3>
	<ol>
		<li>Customize the Simple Registration request if desired.</li>
		<li>Enter an OpenID Identifier associated with the Provider to test.</li>
		<li>Complete authentication at the Provider. Note whether the Provider asks for permission
			to send personal information with the authentication.</li>
		<li>Check that the registration values show up back here.</li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The Provider must recognize the standard attribute Type URIs above and successfully
		transfer the values to this page. </p>
</asp:Content>
