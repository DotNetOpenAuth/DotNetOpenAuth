namespace OAuthResourceServer.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using System.Text;

	[ServiceContract]
	public interface IDataApi {
		[OperationContract, WebGet(UriTemplate = "/age", ResponseFormat = WebMessageFormat.Json)]
		int? GetAge();

		[OperationContract, WebGet(UriTemplate = "/name", ResponseFormat = WebMessageFormat.Json)]
		string GetName();

		[OperationContract, WebGet(UriTemplate = "/favoritesites", ResponseFormat = WebMessageFormat.Json)]
		string[] GetFavoriteSites();
	}
}