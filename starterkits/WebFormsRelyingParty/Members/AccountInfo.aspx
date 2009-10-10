<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccountInfo.aspx.cs" Inherits="WebFormsOpenIdRelyingParty.Members.AccountInfo"
	MasterPageFile="~/Site.Master" ValidateRequest="false" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<%@ Register Assembly="System.Web.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
	Namespace="System.Web.UI.WebControls" TagPrefix="asp" %>
<asp:Content runat="server" ContentPlaceHolderID="Body">
	<asp:ScriptManager ID="ScriptManager1" runat="server" />
	<h3>
		Personal information
	</h3>
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<table>
				<tr>
					<td>
						First name
					</td>
					<td>
						<asp:TextBox ID="firstNameBox" runat="server" />
					</td>
				</tr>
				<tr>
					<td>
						Last name
					</td>
					<td>
						<asp:TextBox ID="lastNameBox" runat="server" />
					</td>
				</tr>
				<tr>
					<td>
						Email
					</td>
					<td>
						<asp:TextBox ID="emailBox" runat="server" Columns="40" ValidationGroup="Profile" />
						<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="emailBox"
							ErrorMessage="Invalid email address" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
							ValidationGroup="Profile">invalid</asp:RegularExpressionValidator>
					</td>
				</tr>
				<tr>
					<td>
					</td>
					<td>
						<asp:Button ID="saveChanges" runat="server" Text="Save profile changes" OnClick="saveChanges_Click"
							ValidationGroup="Profile" />
						<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1"
							DynamicLayout="true">
							<ProgressTemplate>
								Saving...
							</ProgressTemplate>
						</asp:UpdateProgress>
					</td>
				</tr>
			</table>
		</ContentTemplate>
		<Triggers>
			<asp:AsyncPostBackTrigger ControlID="saveChanges" EventName="Click" />
		</Triggers>
	</asp:UpdatePanel>
	<h3>
		OpenIDs &amp; InfoCards</h3>
	<asp:Repeater ID="Repeater1" runat="server">
		<HeaderTemplate>
			<ul class="AuthTokens">
		</HeaderTemplate>
		<ItemTemplate>
			<li class='<%# ((bool)Eval("IsInfoCard")) ? "InfoCard" : "OpenID" %>'>
				<asp:Label ID="OpenIdClaimedIdentifierLabel" runat="server" Text='<%# Eval("FriendlyIdentifier") %>'
					ToolTip='<%# Eval("ClaimedIdentifier") %>' />
				<asp:Label runat="server" ForeColor="Gray" Text="(current login token)" ToolTip="To delete this token, you must log in using some other token."
					Visible='<%# String.Equals((string)Eval("ClaimedIdentifier"), Page.User.Identity.Name, StringComparison.Ordinal) %>' />
				<asp:LinkButton runat="server" Text="remove" CommandName="delete" CommandArgument='<%# Eval("ClaimedIdentifier") %>'
					ID="deleteOpenId" OnCommand="deleteOpenId_Command" Visible='<%# !String.Equals((string)Eval("ClaimedIdentifier"), Page.User.Identity.Name, StringComparison.Ordinal) %>' />
			</li>
		</ItemTemplate>
		<FooterTemplate>
			</ul>
		</FooterTemplate>
	</asp:Repeater>
	<asp:Panel ID="Panel1" runat="server" DefaultButton="addOpenId">
		<rp:OpenIdAjaxTextBox runat="server" ID="openIdBox" OnLoggedIn="openIdBox_LoggedIn"
			OnLoggingIn="openIdBox_LoggingIn" />
		<asp:Button ID="addOpenId" runat="server" Text="Add Identifier" OnClick="addOpenId_Click" />
		<asp:Label ID="differentAccountLabel" runat="server" EnableViewState="False" ForeColor="Red"
			Text="This identifier already belongs to a different user account." Visible="False" />
		<asp:Label ID="alreadyLinkedLabel" runat="server" EnableViewState="False" ForeColor="Red"
			Text="This identifier is already linked to your account." Visible="False" />
	</asp:Panel>
	<asp:Panel ID="Panel2" runat="server">
		<ic:InfoCardSelector ID="InfoCardSelector1" runat="server" ImageSize="Size92x64"
			ToolTip="Log in with your Information Card" OnReceivedToken="InfoCardSelector1_ReceivedToken">
			<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier" />
		</ic:InfoCardSelector>
	</asp:Panel>
</asp:Content>
