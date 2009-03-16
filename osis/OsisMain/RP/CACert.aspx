<%@ Page Title="RP accepts CA Cert trust root" Language="C#" MasterPageFile="~/TestMaster.master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<h3>Instructions </h3>
	<ol>
		<li>Log into the RP under test with the identifier: https://test-id.info/RP/AffirmativeIdentity.aspx
		</li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP passes if it completes a successful login for the given identifier. </p>
</asp:Content>
