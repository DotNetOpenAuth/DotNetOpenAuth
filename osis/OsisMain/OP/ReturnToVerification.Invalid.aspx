<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" %>

<script runat="server">

</script>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<p>You should never reach this page. The Provider you are testing should never send
		an assertion to this URL.  It has FAILED this test. </p>
	<asp:HyperLink runat="server" NavigateUrl="~/OP/ReturnToVerification.aspx" Text="Return to test page" />
</asp:Content>
