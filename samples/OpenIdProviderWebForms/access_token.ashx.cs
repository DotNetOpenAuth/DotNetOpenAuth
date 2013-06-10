namespace OpenIdProviderWebForms {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Services;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using OpenIdProviderWebForms.Code;

	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class access_token : HttpAsyncHandlerBase {
		public override bool IsReusable {
			get { return true; }
		}

		protected override async Task ProcessRequestAsync(HttpContext context) {
			var request = await OAuthHybrid.ServiceProvider.ReadAccessTokenRequestAsync(
				new HttpRequestWrapper(context.Request),
				context.Response.ClientDisconnectedToken);
			var response = OAuthHybrid.ServiceProvider.PrepareAccessTokenMessage(request);
			var httpResponseMessage = await OAuthHybrid.ServiceProvider.Channel.PrepareResponseAsync(
				response,
				context.Response.ClientDisconnectedToken);
			await httpResponseMessage.SendAsync();
		}
	}
}
