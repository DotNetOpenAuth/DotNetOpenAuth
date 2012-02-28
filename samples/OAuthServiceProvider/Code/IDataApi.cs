namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.ServiceModel;
	using System.Text;

	[ServiceContract]
	public interface IDataApi {
		[OperationContract]
		int? GetAge();

		[OperationContract]
		string GetName();

		[OperationContract]
		string[] GetFavoriteSites();
	}
}