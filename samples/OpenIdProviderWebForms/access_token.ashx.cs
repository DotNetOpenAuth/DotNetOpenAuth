namespace OpenIdProviderWebForms {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Services;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.Messaging;
	using OpenIdProviderWebForms.Code;
	using System.Threading;

	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class access_token : IHttpAsyncHandler {
		public bool IsReusable {
			get { return true; }
		}

		public async Task ProcessRequestAsync(HttpContext context) {
			var request = await OAuthHybrid.ServiceProvider.ReadAccessTokenRequestAsync(
				new HttpRequestWrapper(context.Request),
				context.Response.ClientDisconnectedToken);
			var response = OAuthHybrid.ServiceProvider.PrepareAccessTokenMessage(request);
			var httpResponseMessage = await OAuthHybrid.ServiceProvider.Channel.PrepareResponseAsync(
				response,
				context.Response.ClientDisconnectedToken);
			await httpResponseMessage.SendAsync();
		}

		public void ProcessRequest(HttpContext context) {
			this.ProcessRequestAsync(context).GetAwaiter().GetResult();
		}

		public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
			return this.ProcessRequestAsync(context).ToApm(cb, extraData);
		}

		public void EndProcessRequest(IAsyncResult result) {
			((Task)result).Wait(); // rethrows exceptions
		}
	}
}
