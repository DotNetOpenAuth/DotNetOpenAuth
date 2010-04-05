<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccountInfo.aspx.cs" Inherits="WebFormsRelyingParty.Members.AccountInfo"
	MasterPageFile="~/Site.Master" ValidateRequest="false" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<%@ Register Assembly="System.Web.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
	Namespace="System.Web.UI.WebControls" TagPrefix="asp" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
<% if (Request.Url.IsLoopback) { %>
	<script type="text/javascript" src="../scripts/jquery-1.3.1.js"></script>
	<script type="text/javascript" src="../scripts/jquery-ui-personalized-1.6rc6.js"></script>
<% } else { %>
	<script type="text/javascript" language="javascript" src="http://www.google.com/jsapi"></script>
	<script type="text/javascript" language="javascript">
		google.load("jquery", "1.3.2");
		google.load("jqueryui", "1.7.2");
	</script>
<% } %>
	<script type="text/javascript" src="../scripts/jquery.cookie.js"></script>
</asp:Content>
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
							ValidationGroup="Profile" Text="invalid" Display="Dynamic" />
						<asp:Label runat="server" ID="emailVerifiedLabel" Text="verified" Visible="false" />
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
	<asp:UpdatePanel runat="server" ID="authorizedClientsPanel" ChildrenAsTriggers="true">
		<ContentTemplate>
			<h3>
				Authorized clients
			</h3>
			<asp:Panel runat="server" ID="noAuthorizedClientsPanel" Visible="false">
				You have not authorized any clients to access your data.
			</asp:Panel>
			<asp:Repeater runat="server" ID="tokenListRepeater">
				<HeaderTemplate>
					<ul>
				</HeaderTemplate>
				<ItemTemplate>
					<li>
						<asp:Label runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("Consumer.Name").ToString()) %>' />
						-
						<asp:Label ID="Label1" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("CreatedOn").ToString()) %>' ForeColor="Gray" />
						-
						<asp:LinkButton ID="revokeLink" runat="server" Text="revoke" OnCommand="revokeToken_Command"
							CommandName="revokeToken" CommandArgument='<%# Eval("Token") %>' />
					</li>
				</ItemTemplate>
				<FooterTemplate>
					</ul>
				</FooterTemplate>
			</asp:Repeater>
		</ContentTemplate>
	</asp:UpdatePanel>
	<h3>
		OpenIDs &amp; InfoCards
	</h3>
	<asp:Repeater ID="Repeater1" runat="server">
		<HeaderTemplate>
			<ul class="AuthTokens">
		</HeaderTemplate>
		<ItemTemplate>
			<li class='<%# ((bool)Eval("IsInfoCard")) ? "InfoCard" : "OpenID" %>'>
				<asp:Label ID="OpenIdClaimedIdentifierLabel" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("FriendlyIdentifier").ToString()) %>'
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
	<div>
		<p>
			Add a way to log into your account:
		</p>
		<rp:OpenIdSelector runat="server" ID="openIdSelector" OnLoggedIn="openIdBox_LoggedIn"
			OnReceivedToken="InfoCardSelector1_ReceivedToken">
			<Buttons>
				<rp:SelectorProviderButton OPIdentifier="https://me.yahoo.com/" Image="~/images/yahoo.gif" />
				<rp:SelectorProviderButton OPIdentifier="https://www.google.com/accounts/o8/id" Image="~/images/google.gif" />
				<rp:SelectorInfoCardButton>
					<InfoCardSelector Issuer="" />
				</rp:SelectorInfoCardButton>
				<rp:SelectorOpenIdButton Image="~/images/openid.png" />
			</Buttons>
		</rp:OpenIdSelector>
	</div>
	<asp:Label ID="differentAccountLabel" runat="server" EnableViewState="False" ForeColor="Red"
		Text="This identifier already belongs to a different user account." Visible="False" />
	<asp:Label ID="alreadyLinkedLabel" runat="server" EnableViewState="False" ForeColor="Red"
		Text="This identifier is already linked to your account." Visible="False" />
</asp:Content>
