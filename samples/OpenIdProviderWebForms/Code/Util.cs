//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms.Code {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;

	public static class Util {
		public static string ExtractUserName(Uri url) {
			return url.Segments[url.Segments.Length - 1];
		}

		public static string ExtractUserName(Identifier identifier) {
			return ExtractUserName(new Uri(identifier.ToString()));
		}

		public static Identifier BuildIdentityUrl() {
			return BuildIdentityUrl(HttpContext.Current.User.Identity.Name);
		}

		public static Identifier BuildIdentityUrl(string username) {
			// This sample Provider has a custom policy for normalizing URIs, which is that the whole
			// path of the URI be lowercase except for the first letter of the username.
			username = username.Substring(0, 1).ToUpperInvariant() + username.Substring(1).ToLowerInvariant();
			return new Uri(HttpContext.Current.Request.Url, HttpContext.Current.Response.ApplyAppPathModifier("~/user.aspx/" + username));
		}

		internal static void ProcessAuthenticationChallenge(IAuthenticationRequest idrequest) {
			if (idrequest.Immediate) {
				if (idrequest.IsDirectedIdentity) {
					if (HttpContext.Current.User.Identity.IsAuthenticated) {
						idrequest.LocalIdentifier = Util.BuildIdentityUrl();
						idrequest.IsAuthenticated = true;
					} else {
						idrequest.IsAuthenticated = false;
					}
				} else {
					string userOwningOpenIdUrl = Util.ExtractUserName(idrequest.LocalIdentifier);

					// NOTE: in a production provider site, you may want to only 
					// respond affirmatively if the user has already authorized this consumer
					// to know the answer.
					idrequest.IsAuthenticated = userOwningOpenIdUrl == HttpContext.Current.User.Identity.Name;
				}

				if (idrequest.IsAuthenticated.Value) {
					// add extension responses here.
				}
			} else {
				HttpContext.Current.Response.Redirect("~/decide.aspx", false);
			}
		}

		internal static void ProcessAnonymousRequest(IAnonymousRequest request) {
			if (request.Immediate) {
				// NOTE: in a production provider site, you may want to only
				// respond affirmatively if the user has already authorized this consumer
				// to know the answer.
				request.IsApproved = HttpContext.Current.User.Identity.IsAuthenticated;

				if (request.IsApproved.Value) {
					// Add extension responses here.
					// These would typically be filled in from a user database
				}
			} else {
				HttpContext.Current.Response.Redirect("~/decide.aspx", false);
			}
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