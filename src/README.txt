THE OPENID ASP.NET DEMO
------------------------------------------------ 
This file was created by Willem Muller (willem.muller@netidme.com) on 28/03/2007.
This file was updated by Andrew Arnott (andrewarnott@gmail.com) on 04/04/2007.

Introduction
------------------------------------------------ 
Before getting started, make sure you understand the OpenID Authentication 1.1 spec (http://openid.net/specs/openid-authentication-1_1.html) and the OpenID Simple Registration
Extension 1.0 (http://openid.net/specs/openid-simple-registration-extension-1_0.html).  Presentations and other explanatory resources are available from http://openid.net/.

The current code base comes from a variety of original contributors:
 - Most of the original code around the Consumer and Server comes from a Boo (originally from the Python version) port of JanRain's libraries (http://www.openidenabled.com/openid/libraries/csharp). 
 - A C# port (and at the same time .Net 1.1 -> .Net 2.0) is currently under development at http://code.google.com/p/dotnetopenid/. The following people are main contributors:
  - Scott Hanselman (http://hanselman.com/blog) - Consumer 
  - Jason Alexander (http://jasona.net/) - Server
  - Andrew Arnott (http://blog.nerdbank.net) - ASP.NET Controls (OpenIDLogin and OpenIDTextBox classes)
  - I think the DiffeHelman crypto stuff was all lifted from mono, but don't know by who
  - Willem Muller (willem.muller@netidme.com) - the sample web projects, bit of work on server side implementation of Registration Extensions, this document, and few bug bugfixes  
  - Anyone else deserve a mention...?
  
These have been my experiences in getting OpenID working in a .Net Windows environment. If anyone else discovers things, please contribute.

Setting up up the environment 
------------------------------------------------ 

This code was built using the following prerequisite software:
 * .NET 2.0
 * Visual Studio 2005
 * IIS / PWS
 * Windows 2003 Server
 * NUnit 2.2.9 for .NET 2.0 (get the MSI version from http://www.nunit.org/index.php?p=download)
 * See the tools section further below for some helpful software 

Testing the consumer/server included projects with each other
-------------------------------------------------------------
In this scenario you can use the Personal Web Server (PWS) that is included in
Visual Studio 2008.
1. Open the DotNetOpenId.sln file in VS2008.
2. Right-click on each web project and click "View in Browser" to start PWS
   for each web site.
3. Discover the dynamic port assigned to the server and use it to formulate the
   OpenIDs of the predefined users on the server project's web.config file.
   For example, if the server project is running on port 51957, the OpenID of 
   the first user would be:
   http://localhost:51957/ServerPortal/user/bob
4. Activate the consumer web page and try logging in.

Testing with other consumers/servers on the Internet
---------------------------------------------------
 - Your development machine ideally needs to be internet facing so that you 
   can test with other online server/consumers
 - I reconfigured my network so that all incoming traffic to our external IP 
   on port 79 is forwarded to my local development machine
 - Ensure your firewall is configured to allow outbound calls to web ports
 - Since VS2005 Personal Web Server (PWS) does not allow web requests from 
   other servers (as required by OpenID consumers trying to log into your 
   server), testing with external consumers requires you to use IIS to host 
   your server.

Setting up the IIS Applications 
 * I have set up all my applications on port 79 due to local network config issues
 * Set up http://127.0.0.1:79/ConsumerPortal as an IIS Application and allow anonymous access
 * Set up http://127.0.0.1:79/ServerPortal as an IIS Application and allow anonymous access
 * Your openid server users are set up in ServerPortal\web.config. There are 5 default users already set up.

You need to do something extra for URL rewriting in IIS to work. 

This is the process of url conversion like: user/john ->user.aspx?username=john
 * In IIS, go properties on the website (not the virtual directory)
 * Go the Home Directory Tab and click Configuration
 * Insert a wildcard extension 
 * Enter 'c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll' for the executable
 * Uncheck the 'Verify that file exists button'
 * OK your way out of everything
 * If you navigate to 'http://localhost:79/ServerPortal/user/bob' you should see the text: 'OpenID identity page for bob'

Note: These instructions work on IIS 6 with Windows 2003 Server. Other version of IIS (such as the one with windows XP - IIS 5.1) will vary. For IIS 5.1 , try follow instructions documented toward the end of this article: http://www.codeproject.com/aspnet/URLRewriter.asp. If you still have issues (particularly if you get 404 when trying the demos or experience something like http://groups.google.co.uk/group/microsoft.public.inetserver.iis/browse_thread/thread/386efa0bf596234b/ee1fab525c129071?lnk=st&q=URLRewriter+IIS+XP+404&rnum=2&hl=en#ee1fab525c129071) try this: 
 * create a .openid extension that maps to asp.net (c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll)
 * browse to .openid eg: http://IP/ServerPortal/user/bob.openid

Configure VS2005 to use IIS rather than PWS
1. Right-click on one of the web projects within Solution Explorer.
2. Select Property Pages.
3. Select Start Options on the left.
4. Under the Server section on the right, select Use Custom Server and 
   fill in the Base URL.

The demos
------------------------------------------------ 
These will illustrate OpenID in action. You can debug the code to get a good idea of what's going on.
The implementations are built on top of ASP.Net's forms authentication. So basically if you're unauthenticated and get to page requiring authentication, it takes you through the
Open-ID identity provider, tracks in session that you've left and then recognizes the user when they return to the consumer and only then logs them into FormsAuth and redirects them to their 
orignally requested page. 

The Consumer Demo 
1) Kill all session cookies
2) Create an OpenID account with one of the Open Servers listed below OR use the demo Server as the identity provider - using http://[EXTERNAL IP]/ServerPortal/user/bob with the password 'test'
3) Go to http://[EXTERNAL IP]/ConsumerPortal/default.aspx and enter the OpenIDURL
4) You are required to authenticate with the provider. Some fields (eg Name, DoB, Country etc.) are requested, some required and some omitted. 
Your OpenID provider should prompt you for the relevant fields, or at least make you aware which fields its passing back. The exact page flow and auhentication mechanism will be implemented differently by different identity providers.
5) After providing the required info and loggin in, you are taken back to the http://[EXTERNAL IP]/ConsumerPortal/default.aspx and the available profile information is displayed

The Server Demo 
1) Kill all session cookies
2) Get the full openID url for a user based on whats in web.config. By default you can use http://[EXTERNAL IP]/ServerPortal/user/bob with the password 'test'
3) Go to http://[EXTERNAL IP]/ConsumerPortal/default.aspx and enter the OpenIDURL of the local server
4) The user is prompted for their password. The username field is propulated from the openid url and grayed out.
5) The user is presentend with their  identity url, a trust root (the site requiring authentication) and set of fields to complete. 
Only the requested or required fields are presented. Fields with * means the consumer requires it. 
6) The user completes the fields and clicks Yes and are taken to http://[EXTERNAL IP]/ConsumerPortal/default.aspx with their available profile information.
 
 
Interesting classes and methods
------------------------------------------------ 
I believe a lot of this code originally came from a Python port for which good documentation is available: http://www.openidenabled.com/resources/docs/openid/python/1.1.0/.
There will certainly be additions and refactorings made to the .Net version, I guess this will soon become out of date. Here is a short list of the import entrypoint objects and methods.

Consumer 
- To construct a consumer class you need to pass in (HttpSessionState and IAssociationStore).  The session is needed so that the consumer can track who's doing what and who's who's 
when the browser leaves the consumer site and returns later on. The IAssociationStore is used to generate, store and pair up shared secrets.
- Consumer.Begin(Url): This is the starting point for a consumer. Once a user has entered their url, you should call Consumer.Begin(Url). This initiates the discovery phase,
obtains the shared secret and redirects the user's browser to their openid identity provider.
- Consumer.Complete(NameValueCollection) - This should be called when it's suspected that the user is returning from the identity provider. It performs some structural validation 
on the return message and does some general cleanup. After this method has executed successfully, the consumer can assume a successfull authentication and log the user in locally.

Server
-  Decoder.Decode should be called intially to obtain a request object. The following types are possibly returned (all derriving from Request):
   - CheckIdRequest (in immediate mode - for AJAX style behaviour)
   - CheckIdRequest (in setup mode)
   - CheckAuthRequest (the call for DUMB mode revalidation of the signature)
   - AssociateRequest (the request for pre-obtaining the shared secret upfront, or SMART mode)
  
 - call Server.HandleRequest, passing in the appropriate request. This is an overloaded method that actually ends up doing the physical work and retuning a Response object.

 - once you have response, call Util.GenerateHttpResponse. This will do any necessary digital signing and  return the response through the appropriate mechanism (redirect or server side)
  

Development tips / Issues I found:
------------------------------------------------ 



 - Uncomment  //System.Diagnostics.Debugger.Launch(); in global.asax to force the debugger to start up if its not working
 - I would recommend against setting up multiple websites on your pc using host headers and entries in you hosts file
 - Always access the test sites via their external IP's, not localhost or 127.0.0. I started testing like this, but stopped because I kept getting an exception
'Unable to read data from the transport connection: The connection was closed.' when trying to read from a webresponse stream in the OpenID discovery phase. It turned out the problem was the Cassini file based web 
server. Thats why I created the Consumer.Util.ReadAndClose function. It's horrible code and needs to be improved, but ensures that things works if u need to test things locally.
 - I have not done any HTTPS testing!

For a complete and growing list of OpenID enabled sites, go to: http://openiddirectory.com/allcats.html

Good sites to test with if you're developing a consumer:
 - http://www.myopenid.com/
 - http://claimid.com/ (supports registration extensions)
 - http://www.freeyourid.com/  (supports registration extensions)
 
Good sites to test with if you're developing a server:
 - http://beta.zooomr.com/home *
 - http://cr.unchy.com/  (supports registration extensions)
 - http://blog.identity20.eu *
 - http://openiddirectory.com *
 - http://www.centernetworks.com/ (supports registration extensions)
 - http://www.loudisrelative.com (supports registration extensions)
 - http://rssarchive.com/index.html 
 - http://www.jyte.com (supports registration extensions)
 - http://dis.covr.us/ 
* These sites seem to block outgoing traffic that is not on a non standard HTTP port like 80 and 443. Therefore you'll need to host on a proper internet domain before doing any testing with them.

Useful tools:
 - Fidler (http://www.fiddlertool.com/fiddler/) - this will allow you to monitor HTTP traffic when using IE
 - TamperIE (http://www.bayden.com/Other/) - allows you to change form data before posting it
 - IE Developer toolbar (http://www.microsoft.com/downloads/details.aspx?familyid=E59C3964-672D-4511-BB3E-2D5E1DB91038&displaylang=en) - good tool for general IE UI development. Has some neat features for quickly clearing cookies etc.
 - iMacros (http://www.iopus.com/download/) - good for automating web testing






