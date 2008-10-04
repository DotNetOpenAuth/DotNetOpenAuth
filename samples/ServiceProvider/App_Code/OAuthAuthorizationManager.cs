using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using DotNetOAuth;

/// <summary>
/// A WCF extension to authenticate incoming messages using OAuth.
/// </summary>
public class OAuthAuthorizationManager : ServiceAuthorizationManager {
	public OAuthAuthorizationManager() {
	}

	protected override bool CheckAccessCore(OperationContext operationContext) {
		if (!base.CheckAccessCore(operationContext)) {
			return false;
		}

		HttpRequestMessageProperty httpDetails = operationContext.RequestContext.RequestMessage.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
		Uri requestUri = operationContext.RequestContext.RequestMessage.Properties["OriginalHttpRequestUri"] as Uri;
		ServiceProvider sp = Constants.CreateServiceProvider();
		var auth = sp.GetProtectedResourceAuthorization(httpDetails, requestUri);
		if (auth != null) {
			string consumer = auth.ConsumerKey;

			//// TODO: pass the consumer along to the operation somehow

			return true;
		}

		return false;
	}
}
