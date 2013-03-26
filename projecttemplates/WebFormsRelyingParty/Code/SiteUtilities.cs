//-----------------------------------------------------------------------
// <copyright file="SiteUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	public static class SiteUtilities {
		private const string CsrfCookieName = "CsrfCookie";
		private static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

		public static string SetCsrfCookie() {
			// Generate an unpredictable secret that goes to the user agent and must come back
			// with authorization to guarantee the user interacted with this page rather than
			// being scripted by an evil Consumer.
			byte[] randomData = new byte[8];
			CryptoRandomDataGenerator.GetBytes(randomData);
			string secret = Convert.ToBase64String(randomData);

			// Send the secret down as a cookie...
			var cookie = new HttpCookie(CsrfCookieName, secret) {
				Path = HttpContext.Current.Request.Path,
				HttpOnly = true,
				Expires = DateTime.Now.AddMinutes(30),
			};
			HttpContext.Current.Response.SetCookie(cookie);

			// ...and also return the secret so the caller can save it as a hidden form field.
			return secret;
		}

		public static void VerifyCsrfCookie(string secret) {
			var cookie = HttpContext.Current.Request.Cookies[CsrfCookieName];
			if (cookie != null) {
				if (cookie.Value == secret && !string.IsNullOrEmpty(secret)) {
					// Valid CSRF check.  Clear the cookie and return.
					cookie.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
					cookie.Value = string.Empty;
					if (HttpContext.Current.Request.Browser["supportsEmptyStringInCookieValue"] == "false") {
						cookie.Value = "NoCookie";
					}
					HttpContext.Current.Response.SetCookie(cookie);
					return;
				}
			}

			throw new InvalidOperationException("Invalid CSRF check.");
		}

		internal static Task ToApm(this Task task, AsyncCallback callback, object state) {
			if (task == null) {
				throw new ArgumentNullException("task");
			}

			var tcs = new TaskCompletionSource<object>(state);
			task.ContinueWith(
				t => {
					if (t.IsFaulted) {
						tcs.TrySetException(t.Exception.InnerExceptions);
					} else if (t.IsCanceled) {
						tcs.TrySetCanceled();
					} else {
						tcs.TrySetResult(null);
					}

					if (callback != null) {
						callback(tcs.Task);
					}
				},
				CancellationToken.None,
				TaskContinuationOptions.None,
				TaskScheduler.Default);

			return tcs.Task;
		}
	}
}
