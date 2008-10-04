using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

public class DataApi : IDataApi {
	public int GetAge() {
		return 5;
	}

	public string GetName() {
		return "Andrew";
	}
}
