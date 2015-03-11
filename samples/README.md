<h1>
    DotNetOpenAuth samples
</h1>
<h3>
    Prerequisites:</h3>
<ul>
    <li>Microsoft .NET 3.5</li>
    <li>Visual Studio 2010 or IIS</li>
    <li>Microsoft Windows (XP or Vista, or 2003 Server or later)</li>
    <li>See the tools section further below for some helpful software </li>
</ul>
<h2>
    Getting the samples running</h2>
<h3>
    Testing the relying party/provider samples with each other</h3>
<p>
    In this scenario you can use the Personal Web Server (PWS) that is included in Visual
    Studio 2010.</p>
<ol>
    <li>Open the DotNetOpenAuth.sln or Samples.sln file in VS2010.</li>
    <li>Right-click on each web project under the Samples folder and click "View in Browser"
        to start PWS for each web site.</li>
    <li>Each web project will be dynamicly assigned a port number.&nbsp; Find the port number
        on the URL of the browser window for the Provider.&nbsp; </li>
    <li>Now log into the Relying Party sample web site with this OpenID: http://localhost:<i>providerport</i>/user/bob.
    </li>
    <li>When the provider prompts you for a password, type in &#39;test&#39;.</li>
</ol>
<h3>
    Testing with other relying party/provider sites on the Internet</h3>
<ul>
    <li>You need to have a public IP address to test the Provider sample with other Relying
        Party web sites out on the Internet so they can find your Provider.&nbsp; </li>
    <li>You might need to configure your firewall and/or router to forward traffic to your
        computer.</li>
    <li>Note that some OpenID-enabled sites block URLs that use just IP addresses.&nbsp;
        You may need to get a DNS name to point at your public IP address in order for your
        scenario to work.</li>
    <li>Ensure your firewall is configured to allow inbound and outbound TCP port 80 connections.</li>
    <li>Since VS2010 Personal Web Server (PWS) does not allow web requests from other servers
        (as required by OpenID relying parties trying to log into your server), testing
        with external relying parties requires you to use IIS to host your server.</li>
</ul>
<h3>
    Setting up the IIS Applications</h3>
<ul>
    <li>Create an IIS web application for each sample.&nbsp; </li>
    <li>Check that IIS is responding to requests on the port that your router will be forwarding
        requests to you on, if applicable.</li>
    <li>Enable anonymous access to each site.</li>
</ul>
<p>
    Configure VS2010 to use IIS rather than PWS</p>
<ol>
    <li>Right-click on one of the web projects within Solution Explorer.</li>
    <li>Select Property Pages.</li>
    <li>Select Start Options on the left.</li>
    <li>Under the Server section on the right, select Use Custom Server and fill in the
        Base URL.</li>
</ol>
<h2>
    The demos</h2>
<p>
    These will illustrate OpenID in action. You can debug the code to get a good idea
    of what's going on. The implementations are built on top of ASP.NET's forms authentication.
    So basically if you're unauthenticated and get to page requiring authentication,
    it takes you through the OpenID identity provider, tracks in session that you've
    left and then recognizes the user when they return to the relying party and only
    then logs them into FormsAuth and redirects them to their originally requested page.
</p>
<h3>
    The Relying Party Demo
</h3>
<ol>
    <li>Kill all session cookies</li>
    <li>Create an OpenID account with one of the Open Servers listed below OR use the demo
        Server as the identity provider - using http://[EXTERNAL IP]/OpenIdProviderWebForms/user/bob
        with the password 'test'</li>
    <li>Go to http://[EXTERNAL IP]/OpenIdRelyingPartyWebForms/default.aspx and enter the
        OpenIDURL</li>
    <li>You are required to authenticate with the provider. Some fields (eg Name, DoB, Country
        etc.) are requested, some required and some omitted. Your OpenID provider should
        prompt you for the relevant fields, or at least make you aware which fields its
        passing back. The exact page flow and auhentication mechanism will be implemented
        differently by different identity providers.</li>
    <li>After providing the required info and loggin in, you are taken back to the http://[EXTERNAL
        IP]/OpenIdRelyingPartyWebForms/default.aspx and the available profile information
        is displayed</li>
</ol>
<h3>
    The Provider Demo
</h3>
<ol>
    <li>Kill all session cookies</li>
    <li>Get the full openID url for a user based on whats in web.config. By default you
        can use http://[EXTERNAL IP]/OpenIdProviderWebForms/user/bob with the password 'test'</li>
    <li>Go to http://[EXTERNAL IP]/OpenIdRelyingPartyWebForms/default.aspx and enter the
        OpenIDURL of the local server</li>
    <li>The user is prompted for their password. The username field is propulated from the
        openid url and grayed out.</li>
    <li>The user is presentend with their identity url, a trust root (the site requiring
        authentication) and set of fields to complete. Only the requested or required fields
        are presented. Fields with * means the consumer requires it. </li>
    <li>The user completes the fields and clicks Yes and are taken to http://[EXTERNAL IP]/OpenIdRelyingPartyWebForms/default.aspx
        with their available profile information.</li>
</ol>
<h3>
    Interesting classes and methods</h3>
<h4>
    Relying party</h4>
<ul>
    <li>DotNetOpenAuth.OpenId.RelyingParty.<b>OpenIdRelyingParty</b> - programmatic access
        to everything a relying party web site needs.</li>
    <li>DotNetOpenAuth.OpenId.RelyingParty.<b>OpenIdTextBox</b> - An ASP.NET control that
        is a bare-bones text input box with a LogOn method that automatically does all the
        OpenId stuff for you.</li>
    <li>DotNetOpenAuth.OpenId.RelyingParty.<b>OpenIdLogin</b> - Like the OpenIdTextBox,
        but has a Login button and some other end user-friendly UI built-in.&nbsp; Drop
        this onto your web form and you&#39;re all done!</li>
</ul>
<h4>
    Provider</h4>
<ul>
    <li>DotNetOpenAuth.OpenId.Provider.<b>OpenIdProvider</b> - programmatic access to everything
        a provider web site needs.</li>
    <li>DotNetOpenAuth.OpenId.Provider.<b>ProviderEndpoint</b> - An ASP.NET control that
        you can drop in and have an instant provider endpoint on your page.</li>
    <li>DotNetOpenAuth.OpenId.Provider.<b>IdentityEndpoint</b> - An ASP.NET control that
        you can drop onto the page for your own or your customers&#39; individual identity
        pages for discovery by Relying Parties.</li>
</ul>
<h3>
    Development tips / Issues I found:</h3>
<p>
    Here is a growing list of <a href="http://openiddirectory.com/allcats.html">OpenID enabled
        sites</a> to test with.
</p>
<p>
    Good sites to test with if you're developing a relying party:
</p>
<ul>
    <li><a href="http://www.myopenid.com/">http://www.myopenid.com/</a></li>
    <li><a href="http://claimid.com/">http://claimid.com/</a> (supports registration extensions)</li>
    <li><a href="http://www.freeyourid.com/">http://www.freeyourid.com/</a> (supports registration
        extensions)</li>
</ul>
<p>
    Good sites to test with if you're developing a server:
</p>
<ul>
    <li><a href="http://beta.zooomr.com/home">http://beta.zooomr.com/home</a>&nbsp; *</li>
    <li><a href="http://cr.unchy.com/">http://cr.unchy.com/</a>&nbsp; (supports registration
        extensions)</li>
    <li><a href="http://blog.identity20.eu">http://blog.identity20.eu</a>&nbsp; *</li>
    <li><a href="http://openiddirectory.com">http://openiddirectory.com</a>&nbsp; *</li>
    <li><a href="http://www.centernetworks.com/">http://www.centernetworks.com/</a>&nbsp;
        (supports registration extensions)</li>
    <li><a href="http://www.loudisrelative.com">http://www.loudisrelative.com</a>&nbsp;
        (supports registration extensions)</li>
    <li><a href="http://rssarchive.com/index.html">http://rssarchive.com/index.html</a>&nbsp;
    </li>
    <li><a href="http://www.jyte.com">http://www.jyte.com</a>&nbsp; (supports registration
        extensions)</li>
    <li><a href="http://dis.covr.us/">http://dis.covr.us/</a>&nbsp; </li>
</ul>
* These sites seem to block outgoing traffic that is not on a non standard HTTP
port like 80 and 443. Therefore you'll need to host on a proper internet domain
before doing any testing with them.
<p>
    Useful tools:</p>
<ul>
    <li><a href="http://www.fiddlertool.com/fiddler/">Fiddler</a> - this will allow you
        to monitor HTTP traffic when using IE</li>
    <li><a href="http://www.bayden.com/Other/">TamperIE</a> - allows you to change form
        data before posting it</li>
    <li><a href="http://www.microsoft.com/downloads/details.aspx?familyid=E59C3964-672D-4511-BB3E-2D5E1DB91038&amp;displaylang=en">
        IE Developer toolbar</a> - good tool for general IE UI development. Has some neat
        features for quickly clearing cookies etc.</li>
    <li><a href="http://www.iopus.com/download/">iMacros</a> - good for automating web testing</li>
</ul>
