<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>DotNetOpenId Classic ASP sample</title>
	<link href="styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<div>
		<a href="http://dotnetopenid.googlecode.com">
			<img runat="server" src="images/dotnetopenid_tiny.gif" title="Jump to the project web site."
				alt="DotNetOpenId" border='0' /></a>
	</div>
	<h2>Classic ASP Relying Party</h2>
	<p>Visit the <a href="MembersOnly.asp">Members Only</a> area. (This will trigger
		a login demo). </p>
	<h3>Required steps for this sample to work on your own machine:</h3>
	<p>Although classic ASP cannot access .NET assemblies directly, it does know how to
		call COM components.&nbsp; DotNetOpenId exposes a COM server to allow classic ASP
		and other COM clients to utilize it for easy OpenID support. The DotNetOpenId.dll
		assembly must be registered as a COM server on each development box and web server
		in order for COM clients such as classic ASP to find it.</p>
	<p>To register DotNetOpenId as a COM server, complete these steps.</p>
	<ol>
		<li>At an administrator command prompt, navigate to a directory where the DotNetOpenId
			assembly is found.</li>
		<li>Register DotNetOpenId as a COM server:<br />
			<span class="command">%windir%\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe 
			/tlb DotNetOpenId.dll</span></li>
		<li>Install DotNetOpenId into the GAC.&nbsp; The gacutil.exe tool may be in an SDK
			directory, which will be in your path if you opened a Visual Studio Command Prompt.<br />
			<span class="command">gacutil.exe /i DotNetOpenId.dll</span></li>
	</ol>
	<p>Another thing to be aware of is that with classic ASP there is no Web.config 
		file in which to customize DotNetOpenId behavior.&nbsp; And the COM interfaces 
		that DotNetOpenId exposes are a very limited subset of full functionality 
		available to .NET clients.&nbsp; Please send feature requests to
		<a href="mailto:dotnetopenid@googlegroups.com">dotnetopenid@googlegroups.com</a>.</p>
</body>
</html>
