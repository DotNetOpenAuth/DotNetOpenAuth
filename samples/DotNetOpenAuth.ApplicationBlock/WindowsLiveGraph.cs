//-----------------------------------------------------------------------
// <copyright file="WindowsLiveGraph.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Json;
	using System.Text;

	[DataContract]
	public class WindowsLiveGraph {
		private static DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(WindowsLiveGraph));

		[DataMember(Name = "id")]
		public string Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		[DataMember(Name = "last_name")]
		public string LastName { get; set; }

		[DataMember(Name = "link")]
		public Uri Link { get; set; }

		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		[DataMember(Name = "updated_time")]
		public string UpdatedTime { get; set; }

		[DataMember(Name = "locale")]
		public string Locale { get; set; }

		public static WindowsLiveGraph Deserialize(string json) {
			if (string.IsNullOrEmpty(json)) {
				throw new ArgumentNullException("json");
			}

			return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
		}

		public static WindowsLiveGraph Deserialize(Stream jsonStream) {
			if (jsonStream == null) {
				throw new ArgumentNullException("jsonStream");
			}

			return (WindowsLiveGraph)jsonSerializer.ReadObject(jsonStream);
		}
	}
}
