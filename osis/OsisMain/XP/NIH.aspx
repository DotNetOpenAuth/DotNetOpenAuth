<%@ Page Title="Client Selector for NIH" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">

		<object type="application/x-informationcard" name="xmlToken">
			<param name="privacyUrl" value="https://test-id.org/OP/PrivacyPolicy.aspx"/>
			<param Name="protocol" Value="http://specs.openid.net/auth/2.0"/>
			<param name="tokenType" value="http://specs.openid.net/auth/2.0"/>
			<param name="issuer" value="https://me.yahoo.com/ https://www.myopenid.com/ https://www.google.com/accounts/o8/id"/>
			<param name="issuerExclusive" value="false" />
			<param Name="OpenIDAuthParameters" Value="openid.ns:http://specs.openid.net/auth/2.0
openid.return_to:https://nihlogin.nih.gov/InternalIDP/OpenID/RP/Consumer.aspx
openid.realm:https://nihlogin.nih.gov/InternalIDP/OpenID/RP/
openid.ns.sreg:http://openid.net/extensions/sreg/1.1
openid.sreg.policy_url:https://nihlogin.nih.gov/InternalIDP/OpenID/RP/PrivacyPolicy.aspx
openid.sreg.required:email
openid.sreg.optional:nickname,fullname
openid.ns.pape:http://specs.openid.net/extensions/pape/1.0
openid.pape.max_auth_age:0
openid.pape.preferred_auth_policies:http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier http://www.idmanagement.gov/schema/2009/05/icam/openid-trust-level1.pdf
openid.ns.alias3:http://openid.net/srv/ax/1.0
openid.alias3.if_available:alias2,alias3,alias5,alias6
openid.alias3.required:alias1,alias4
openid.alias3.mode:fetch_request
openid.alias3.type.alias1:http://axschema.org/contact/email
openid.alias3.count.alias1:1
openid.alias3.type.alias2:http://axschema.org/namePerson
openid.alias3.count.alias2:1
openid.alias3.type.alias3:http://axschema.org/namePerson/friendly
openid.alias3.count.alias3:1
openid.alias3.type.alias4:http://schema.openid.net/contact/email
openid.alias3.count.alias4:1
openid.alias3.type.alias5:http://schema.openid.net/namePerson
openid.alias3.count.alias5:1
openid.alias3.type.alias6:http://schema.openid.net/namePerson/friendly
openid.alias3.count.alias6:1" />
		</object>

		<asp:Button runat="server" Text="Log in using your OpenID Selector" />

</asp:Content>
