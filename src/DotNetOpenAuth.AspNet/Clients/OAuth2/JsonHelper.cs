//-----------------------------------------------------------------------
// <copyright file="JsonHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.IO;
	using System.Runtime.Serialization.Json;

	internal static class JsonHelper {
		public static T Deserialize<T>(Stream stream) where T : class {
			if (stream == null) {
				throw new ArgumentNullException("stream");
			}

			var serializer = new DataContractJsonSerializer(typeof(T));
			return (T)serializer.ReadObject(stream);
		}
	}
}
