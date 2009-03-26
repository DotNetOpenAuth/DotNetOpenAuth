<%@ Page Title="Gmail address book demo" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="GoogleAddressBook.aspx.cs" Inherits="GoogleAddressBook" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View runat="server" ID="Authorize">
			<table>
				<tr>
					<td>
						Google Consumer Key
					</td>
					<td>
						<asp:TextBox ID="consumerKeyBox" runat="server" Columns="35"></asp:TextBox>
						<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
							ControlToValidate="consumerKeyBox" Display="Dynamic" 
							ErrorMessage="RequiredFieldValidator">*</asp:RequiredFieldValidator>
					</td>
				</tr>
				<tr>
					<td>
						Google Consumer Secret
					</td>
					<td>
						<asp:TextBox ID="consumerSecretBox" runat="server" Columns="35"></asp:TextBox>
						<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" 
							ControlToValidate="consumerSecretBox" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
				</tr>
				<tr>
					<td>
						&nbsp;</td>
					<td>
						Don&#39;t have a Google Consumer Key?&nbsp;
						<a href="https://www.google.com/accounts/ManageDomains">Get one</a>.</td>
				</tr>
			</table>
			<asp:Button ID="authorizeButton" runat="server" Text="Download your Gmail Address Book"
				OnClick="authorizeButton_Click" />
		</asp:View>
		<asp:View runat="server" ID="Results">
			<p>Now displaying the first 25 records from your address book:</p>
			<asp:PlaceHolder runat="server" ID="resultsPlaceholder" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
