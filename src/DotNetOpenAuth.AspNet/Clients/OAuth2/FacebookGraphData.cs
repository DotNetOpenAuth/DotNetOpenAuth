//-----------------------------------------------------------------------
// <copyright file="FacebookGraphData.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.ComponentModel;
	using System.Runtime.Serialization;

	/// <summary>
	/// Contains data of a Facebook user.
	/// </summary>
	/// <remarks>
	/// Technically, this class doesn't need to be public, but because we want to make it serializable in medium trust, it has to be public.
	/// </remarks>
	[DataContract]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class FacebookGraphData {
		#region Public Properties

		/// <summary>
		/// Gets or sets the birthday.
		/// </summary>
		/// <value> The birthday. </value>
		[DataMember(Name = "birthday")]
		public string Birthday { get; set; }

		/// <summary>
		/// Gets or sets the email.
		/// </summary>
		/// <value> The email. </value>
		[DataMember(Name = "email")]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the gender.
		/// </summary>
		/// <value> The gender. </value>
		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value> The id. </value>
		[DataMember(Name = "id")]
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the link.
		/// </summary>
		/// <value> The link. </value>
		[DataMember(Name = "link")]
		public Uri Link { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value> The name. </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }
		#endregion
	}
}
