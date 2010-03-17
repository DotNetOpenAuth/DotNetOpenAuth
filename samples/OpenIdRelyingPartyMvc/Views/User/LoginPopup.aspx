<%@ Page Title="Popup Login sample" Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!-- COPYRIGHT (C) 2009 Andrew Arnott.  All rights reserved. -->
<!-- LICENSE: Microsoft Public License available at http://opensource.org/licenses/ms-pl.html -->

<html>
<head>
	<title>OpenID login demo</title>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
	<link type="text/css" href='<%= Url.Content("~/Content/theme/ui.all.css") %>' rel="Stylesheet" />
	<link type="text/css" href='<%= Url.Content("~/Content/css/openidlogin.css") %>' rel="stylesheet" />
	<script type="text/javascript" src='<%= Url.Content("~/Content/scripts/jquery-1.3.1.js") %>'></script>
	<script type="text/javascript" src='<%= Url.Content("~/Content/scripts/jquery-ui-personalized-1.6rc6.js") %>'></script>
	<script>
	$(function() {
		$('#openidlogin').dialog({
			bgiframe: true,
//			autoOpen: true,
			modal: true,
			title: 'Login or Create new account',
			resizable: false,
			hide: 'clip',
			width: '420px',
			buttons: { },
			closeOnEscape: true,
			focus: function(event, ui) { 
				var box = $('#openid_identifier')[0];
				if (box.style.display != 'none') {
					box.focus();
				}
			}
		});
		
		$('#loggedOut').dialog({
			bgiframe: true,
			autoOpen: false,
			title: 'Logged out',
			resizable: false,
			closeOnEscape: true,
			buttons: {
				"Ok": function() { $(this).dialog('close'); }
			}
		});

		$('#loginAction').click(function() {
			$('#openidlogin').dialog('open');
			return false;
		});

		$('#logoutAction').click(function() {
			// TODO: asynchronously log out.
			document.setClaimedIdentifier();
			//$('#loggedOut').dialog('open');
			return false;
		});

		//hover states on the static widgets
		$('.ui-button, ul#icons li').hover(
			function() { $(this).addClass('ui-state-hover'); }, 
			function() { $(this).removeClass('ui-state-hover'); }
		);
		
		document.usernamePlaceholder = "{username}";
		
		function isCompleteIdentifier(identifier) {
			return identifier && identifier != '' && identifier != 'http://' && identifier.indexOf(document.usernamePlaceholder) < 0;
		};
		
		function setSelection() {
			var box = $('#openid_identifier')[0];
			var usernamePlaceholderIndex = box.value.indexOf(document.usernamePlaceholder);
			if (usernamePlaceholderIndex >= 0) {
				box.setSelectionRange(usernamePlaceholderIndex + document.usernamePlaceholder.length);
				box.setSelectionRange(usernamePlaceholderIndex, usernamePlaceholderIndex + document.usernamePlaceholder.length);
			}
		};
		
		function completeLogin() {
			var box = $('#openid_identifier')[0];
			if (box.value.indexOf(document.usernamePlaceholder) >= 0) {
				alert('You need to type in your username first.');
				box.focus();
				setSelection();
				return;
			}
			
			if (!isCompleteIdentifier(box.value)) {
				alert(box.value + ' is not a valid identifier.');
				return;
			}
			
			var box = $('#openid_identifier')[0];
			$('#openidlogin').dialog('close');
			document.setClaimedIdentifier(box.value);
			$('#loginForm').submit();
			return box.value;
		};
		
		document.selectProvider = function(button, identifierTemplate) {
			var box = $('#openid_identifier')[0];
			$('#openidlogin .provider').removeClass('highlight');
			if (isCompleteIdentifier(identifierTemplate)) {
				box.value = identifierTemplate;
				$('#openidlogin .inputbox').slideUp();
				completeLogin();
			} else {			
				if (this.lastIdentifierTemplate == identifierTemplate) {
					$('#openidlogin .inputbox').slideToggle();
				} else {
					$(button).addClass('highlight').show();
					$('#openidlogin .inputbox').slideDown();
					box.value = identifierTemplate;
					if (box.value == null || box.value == '') {
						box.value = 'http://';
					}
					
					setSelection();
				}
				
				box.focus();
			}
			this.lastIdentifierTemplate = identifierTemplate;
		};

		$('#loginButton').click(function() {
			completeLogin();
			return true;
		});
		
		document.openid_identifier_keydown = function(e) {
			if (window.event && window.event.keyCode == 13) {
				$('#loginButton').effect('highlight');
				completeLogin();
			}
		};
		
		document.setClaimedIdentifier = function(identifier) {
			if (identifier) {
				// Apply login
				$('#loginAction').hide();
				$('#logoutAction').show();
			} else {
				// Apply logout
				$('#loginAction').show();
				$('#logoutAction').hide();
			}
			$('#claimedIdentifierLabel')[0].innerText = identifier ? identifier : '';
		};
		
		$('#logoutAction').hide();
	});
	</script>
	
	<style>
		body{ font: 62.5% "Trebuchet MS", sans-serif;}
		.ui-button {padding: .4em .5em .4em 20px;text-decoration: none;position: relative;}
		.ui-button span.ui-icon {margin: 0 5px 0 0;position: absolute;left: .2em;top: 50%;margin-top: -8px;}
		#loginButton {padding: 0.1em 0.4em 0.1em 20px}
	</style>
</head>
<body>

<div style="margin-top: 10px">
	<p style="float: right; margin-top: 0px">
		<a href="#" id="loginAction"  class="ui-button ui-state-default ui-corner-all"><span class="ui-icon ui-icon-locked"></span>Login / New user</a> 
		<a href="#" id="logoutAction" class="ui-button ui-state-default ui-corner-all"><span class="ui-icon ui-icon-unlocked"></span>Logout</a>
	</p>
	<p style="text-align: center; margin-top: 3px; font-family: Arial" id="claimedIdentifierLabel"/>
</div>

<div id="openidlogin" class="ui-widget-content">
	<p>Log in with an account you already use:</p>
	<div class="large buttons">
		<div class="provider" onclick="document.selectProvider(this, 'https://www.google.com/accounts/o8/id')"><div><img src='<%= Url.Content("~/Content/images/google.gif") %>'/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'https://me.yahoo.com/')"><div><img src='<%= Url.Content("~/Content/images/yahoo.gif") %>'/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'http://openid.aol.com/{username}')"><div><img src='<%= Url.Content("~/Content/images/aol.gif") %>'/></div></div>
		<div class="provider" onclick="document.selectProvider(this, '')"><div><img src='<%= Url.Content("~/Content/images/openid.gif") %>'/></div></div>
	</div>
	<div class="small buttons">
		<div class="provider" onclick="document.selectProvider(this, 'http://www.flickr.com/photos/{username}')"><div><img src="http://flickr.com/favicon.ico"/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'https://www.myopenid.com/')"><div><img src="http://myopenid.com/favicon.ico"/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'http://{username}.livejournal.com/')"><div><img src="http://www.livejournal.com/favicon.ico"/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'https://technorati.com/people/technorati/{username}/')"><div><img src="http://technorati.com/favicon.ico"/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'https://{username}.wordpress.com/')"><div><img src="http://www.wordpress.com/favicon.ico"/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'http://{username}.blogspot.com/')"><div><img src="http://blogspot.com/favicon.ico"/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'https://myvidoop.com/')"><div><img src="http://www.myvidoop.com/favicon.ico"/></div></div>
		<div class="provider" onclick="document.selectProvider(this, 'https://pip.verisignlabs.com/')"><div><img src="http://pip.verisignlabs.com/favicon.ico"/></div></div>
	</div>
	<% Html.BeginForm("Authenticate", "User", FormMethod.Post, new { id = "loginForm" }); %>
	<div class="inputbox">
		<input type="text" id="openid_identifier" name="openid_identifier" onKeyDown="document.openid_identifier_keydown(this)" onFocus="$('#loginButton').addClass('ui-state-hover')" onBlur="$('#loginButton').removeClass('ui-state-hover')" />
		<a href="#" id="loginButton" class="ui-button ui-state-default ui-corner-all" style="color: white; font-size: 10pt"><span class="ui-icon ui-icon-key"></span>Login</a>
	</div>
	<% Html.EndForm(); %>
	<p><a href="javascript:$('#openidlogin .help').slideToggle()">Get help logging in</a></p>
	<div class="help">
		<p>If you don't have an account with any of these services, you can
			<a href="https://www.myopenid.com/signup" target="OpenIdProvider">create one</a>.
		<p>If you have logged into this site previously, click the same button you did last time.</p>
	</div>
</div>

<div id="loggedOut" class="ui-widget-content">
	<p>You have been logged out.</p>
</div>
</body>
</html>