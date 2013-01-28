<%@ WebHandler Language="C#" Class="OpenIdProviderEndpoint" %>
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DotNetOpenAuth.OpenId.Provider;

using DotNetOpenAuth.Messaging;

public class OpenIdProviderEndpoint : IHttpAsyncHandler {
	public bool IsReusable {
		get { return true; }
	}

	public IAsyncResult BeginProcessRequest(HttpContext context, System.AsyncCallback cb, object extraData) {
		return ToApm(this.ProcessRequestAsync(context), cb, extraData);
	}

	public void EndProcessRequest(IAsyncResult result) {
		((Task)result).Wait(); // rethrows exceptions
	}

	public void ProcessRequest(HttpContext context) {
		this.ProcessRequestAsync(context).GetAwaiter().GetResult();
	}

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

	private async Task ProcessRequestAsync(HttpContext context) {
		OpenIdProvider provider = new OpenIdProvider();
		IRequest request = await provider.GetRequestAsync(new HttpRequestWrapper(context.Request), context.Response.ClientDisconnectedToken);
		if (request != null) {
			if (!request.IsResponseReady) {
				IAuthenticationRequest authRequest = (IAuthenticationRequest)request;
				authRequest.IsAuthenticated = true;
			}

			var response = await provider.PrepareResponseAsync(request, context.Response.ClientDisconnectedToken);
			await response.SendAsync(new HttpResponseWrapper(context.Response), context.Response.ClientDisconnectedToken);
		}
	}
}