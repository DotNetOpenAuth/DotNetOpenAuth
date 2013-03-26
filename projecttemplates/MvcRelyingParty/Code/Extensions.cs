namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;

	internal static class Extensions {
		internal static Uri ActionFull(this UrlHelper urlHelper, string actionName) {
			return new Uri(HttpContext.Current.Request.Url, urlHelper.Action(actionName));
		}

		internal static Uri ActionFull(this UrlHelper urlHelper, string actionName, string controllerName) {
			return new Uri(HttpContext.Current.Request.Url, urlHelper.Action(actionName, controllerName));
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
