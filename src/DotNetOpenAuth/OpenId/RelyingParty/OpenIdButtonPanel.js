//-----------------------------------------------------------------------
// <copyright file="OpenIdButtonPanel.js" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
//     This file may be used and redistributed under the terms of the
//     Microsoft Public License (Ms-PL) http://opensource.org/licenses/ms-pl.html
// </copyright>
//-----------------------------------------------------------------------

$(function() {
	var hint = $.cookie('openid_identifier') || '';

	var ajaxbox = $('#openid_identifier')[0];
	if (hint != 'infocard') {
		ajaxbox.value = hint;
	}

	if (document.infoCard.isSupported()) {
		$('ul.OpenIdProviders li.infocard')[0].style.display = 'inline-block';
	}

	if (hint.length > 0) {
		var ops = $('ul.OpenIdProviders li');
		ops.addClass('grayedOut');
		var matchFound = false;
		ops.each(function(i, li) {
			if (li.id == hint || (hint == 'infocard' && $(li).hasClass('infocard'))) {
				$(li)
					.removeClass('grayedOut')
					.addClass('focused');
				matchFound = true;
			}
		});
		if (!matchFound) {
			$('#OpenIDButton')
					.removeClass('grayedOut')
					.addClass('focused');
			$('#OpenIDForm').show('slow', function() {
				$('#openid_identifier').focus();
			});
		}
	}

	function showLoginSuccess(userSuppliedIdentifier, hide) {
		var li = document.getElementById(userSuppliedIdentifier);
		if (li) {
			if (hide) {
				$(li).removeClass('loginSuccess');
			} else {
				$(li).addClass('loginSuccess');
			}
		}
	}

	ajaxbox.onStateChanged = function(state) {
		if (state == "authenticated") {
			showLoginSuccess('OpenIDButton');
		} else {
			showLoginSuccess('OpenIDButton', true); // hide checkmark
		}
	};

	function checkidSetup(identifier, timerBased) {
		var openid = new window.OpenIdIdentifier(identifier);
		if (!openid) { throw 'checkidSetup called without an identifier.'; }
		openid.login(function(discoveryResult, respondingEndpoint, extensionResponses) {
			showLoginSuccess(discoveryResult.userSuppliedIdentifier);
			doLogin(respondingEndpoint, discoveryResult);
		});
	}

	// Sends the positive assertion we've collected to the server and actually logs the user into the RP.
	function doLogin(respondingEndpoint, discoveryResult) {
		var retain = true; //!$('#NotMyComputer')[0].selected;
		$.cookie('openid_identifier', retain ? discoveryResult.userSuppliedIdentifier : null, { path: window.aspnetapppath });
		window.postLoginAssertion(respondingEndpoint.response.toString(), window.parent.location.href);
	}

	// This FrameManager will be used for background logins for the OP buttons
	// and the last used identifier.  It is NOT the frame manager used by the
	// OpenIdAjaxTextBox, as it has its own.
	var backgroundTimeout = 3000;

	$(document).ready(function() {
		var ops = $('ul.OpenIdProviders li');
		ops.each(function(i, li) {
			if ($(li).hasClass('OPButton')) {
				li.authenticationIFrames = new window.dnoa_internal.FrameManager(1/*throttle*/);
				var openid = new window.OpenIdIdentifier(li.id);
				openid.loginBackground(li.authenticationIFrames, function(discoveryResult, respondingEndpoint, extensionResponses) {
					showLoginSuccess(li.id);
					//alert('OP button background login as ' + respondingEndpoint.claimedIdentifier + ' was successful!');
				}, null, backgroundTimeout);
			}
		});
	});

	$('ul.OpenIdProviders li').click(function() {
		var lastFocus = $('.focused')[0];
		if (lastFocus != $(this)[0]) {
			$('ul.OpenIdProviders li').removeClass('focused');
			$(this).addClass('focused');
		}

		// Make sure we're not graying out any OPs if the user clicked on a gray button.
		var wasGrayedOut = false;
		if ($(this).hasClass('grayedOut')) {
			wasGrayedOut = true;
			$('ul.OpenIdProviders li').removeClass('grayedOut');
		}

		// Be sure to hide the openid_identifier text box unless the OpenID button is selected.
		if ($(this)[0] != $('#OpenIDButton')[0] && $('#OpenIDForm').is(':visible')) {
			$('#OpenIDForm').hide('slow');
		}

		// If the user clicked on a button that has the "we're ready to log you in immediately",
		// then log them in!
		if ($(this).hasClass('loginSuccess')) {
			var relevantUserSuppliedIdentifier = null;
			// Don't immediately login if the user clicked OpenID and he can't see the identifier box.
			if ($(this)[0].id != 'OpenIDButton') {
				relevantUserSuppliedIdentifier = $(this)[0].id;
			} else if ($('#OpenIDForm').is(':visible')) {
				relevantUserSuppliedIdentifier = ajaxbox.value;
			}

			if (relevantUserSuppliedIdentifier) {
				var discoveryResult = window.dnoa_internal.discoveryResults[relevantUserSuppliedIdentifier];
				var respondingEndpoint = discoveryResult.findSuccessfulRequest();
				doLogin(respondingEndpoint, discoveryResult);
			}
		} else if ($(this).hasClass('OPButton')) {
			checkidSetup($(this)[0].id);
		} else if ($(this).hasClass('infocard') && wasGrayedOut) {
			// we need to forward the click onto the InfoCard image so it is handled, since our
			// gray overlaying div captured the click event.
			$('img', this)[0].click();
		}
	});
	$('#OpenIDButton').click(function() {
		if ($('#OpenIDForm').is(':hidden')) {
			$('#OpenIDForm').show('slow', function() {
				$('#openid_identifier').focus();
			});
		} else {
			$('#openid_identifier').focus();
		}
	});

	// Make popup window close on escape (the dialog style is already taken care of)
	$(document).keydown(function(e) {
		if (e.keyCode == $.ui.keyCode.ESCAPE) {
			window.close();
		}
	});
});