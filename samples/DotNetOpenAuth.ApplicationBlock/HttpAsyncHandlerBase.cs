//-----------------------------------------------------------------------
// <copyright file="HttpAsyncHandlerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	public abstract class HttpAsyncHandlerBase : IHttpAsyncHandler {
		public abstract bool IsReusable { get; }

		public IAsyncResult BeginProcessRequest(HttpContext context, System.AsyncCallback cb, object extraData) {
			return ToApm(this.ProcessRequestAsync(context), cb, extraData);
		}

		public void EndProcessRequest(IAsyncResult result) {
			((Task)result).Wait(); // rethrows exceptions
		}

		public void ProcessRequest(HttpContext context) {
			this.ProcessRequestAsync(context).GetAwaiter().GetResult();
		}

		protected abstract Task ProcessRequestAsync(HttpContext context);

		private static Task ToApm(Task task, AsyncCallback callback, object state) {
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
