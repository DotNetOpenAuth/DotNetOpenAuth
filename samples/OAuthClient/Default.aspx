<%@ Page Title="DotNetOpenAuth Consumer samples" Language="C#" MasterPageFile="~/MasterPage.master"
	AutoEventWireup="true" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<p>OAuth allows this web site to access your private data with your authorization, but
		without you having to give up your password. </p>
	<p>Select a demo:</p>
	<ul>
		<li><a href="GoogleAddressBook.aspx">Download your Gmail address book</a></li>
		<li><a href="Twitter.aspx">Get your Twitter updates</a></li>
		<li><a href="SignInWithTwitter.aspx">Sign In With Twitter</a></li>
		<li><a href="Facebook.aspx">Sign in with Facebook</a></li>
		<li><a href="WindowsLive.aspx">Sign in with Windows Live</a></li>
		<li><a href="SampleWcf2.aspx">Interop with Authorization Server sample (Authorization code grant) and Resource Server using WCF w/ OAuth 2.0 </a></li>
		<li><a href="SampleWcf2Javascript.html">Interop with Authorization Server sample (implicit grant) and Resource Server using WCF w/ OAuth 2.0 </a></li>
	</ul>
</asp:Content>
