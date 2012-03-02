//-----------------------------------------------------------------------
// <copyright file="WindowsLiveUserData.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Runtime.Serialization;
	using System.ComponentModel;

	/// <summary>
	/// Contains data of a Windows Live user.
	/// </summary>
	/// <remarks>
	/// Technically, this class doesn't need to be public, but because we want to make it serializable
	/// in medium trust, it has to be public.
	/// </remarks>
	[DataContract]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class WindowsLiveUserData {
		[DataMember(Name = "id")]
		public string Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "link")]
		public Uri Link { get; set; }

		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		[DataMember(Name = "last_name")]
		public string LastName { get; set; }
	}
}
