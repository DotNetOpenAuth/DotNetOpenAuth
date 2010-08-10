$(function() {
	var loginContent = 'LoginFrame.aspx';
	var popupWindowName = 'openidlogin';
	var popupWidth = 365;
	var popupHeight = 273; // use 205 for 1 row of OP buttons, or 273 for 2 rows
	var iframe;

	{
		var div = window.document.createElement('div');
		div.style.padding = 0;
		div.id = 'loginDialog';

		iframe = window.document.createElement('iframe');
		iframe.src = "about:blank"; // don't set the actual page yet, since FireFox & Chrome will refresh it when the iframe is moved in the DOM anyway.
		iframe.frameBorder = 0;
		iframe.width = popupWidth;
		iframe.height = popupHeight;
		div.appendChild(iframe);

		window.document.body.appendChild(div);
	}

	$(document).ready(function() {
		$("#loginDialog").dialog({
			bgiframe: true,
			modal: true,
			title: 'Login or register',
			resizable: false,
			hide: 'clip',
			width: popupWidth,
			height: popupHeight + 50,
			buttons: {},
			closeOnEscape: true,
			autoOpen: false,
			close: function(event, ui) {
				// Clear the URL so Chrome/Firefox don't refresh the iframe when it's hidden.
				iframe.src = "about:blank";
			},
			open: function(event, ui) {
				iframe.src = loginContent;
			},
			focus: function(event, ui) {
				//				var box = $('#openid_identifier')[0];
				//				if (box.style.display != 'none') {
				//					box.focus();
				//				}
			}
		});

		$('.loginPopupLink').click(function() {
			$("#loginDialog").dialog('open');
		});
		$('.loginWindowLink').click(function() {
			if (window.showModalDialog) {
				window.showModalDialog(loginContent, popupWindowName, 'status:0;resizable:1;scroll:1;center:1;dialogHeight:' + popupHeight + 'px;dialogWidth:' + popupWidth + 'px');
			} else {
				window.open(loginContent, popupWindowName, 'modal=yes,status=0,location=1,toolbar=0,menubar=0,resizable=0,scrollbars=0,height=' + popupHeight + 'px,width=' + popupWidth + 'px');
			}
		});
	});
});
