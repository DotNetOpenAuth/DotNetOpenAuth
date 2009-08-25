using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

[ServiceContract]
public interface IDataApi {
	[OperationContract]
    [WebInvoke(Method = "POST")]
	int? GetAge();

	[OperationContract]
    [WebInvoke(Method="POST")]
	string GetName();

	[OperationContract]
    [WebGet()]
	string[] GetFavoriteSites();
}
