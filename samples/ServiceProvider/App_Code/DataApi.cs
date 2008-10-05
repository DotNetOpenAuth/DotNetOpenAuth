using System.Globalization;
using System.ServiceModel;

public class DataApi : IDataApi {
	public int GetAge() {
		return 5;
	}

	public string GetName() {
		string consumerKey = OperationContext.Current.IncomingMessageProperties["OAuthConsumerKey"] as string;
		string accessToken = OperationContext.Current.IncomingMessageProperties["OAuthAccessToken"] as string;
		return string.Format(CultureInfo.InvariantCulture, "Andrew_{0}_{1}", consumerKey.Substring(0, 1), accessToken.Substring(0, 1));
	}
}
