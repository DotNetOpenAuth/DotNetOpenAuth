//-----------------------------------------------------------------------
// <copyright file="AzureADGraph.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.Serialization;

	/// <summary>
	/// Contains data of a AzureAD user.
	/// </summary>
	/// <remarks>
	/// Technically, this class doesn't need to be public, but because we want to make it serializable in medium trust, it has to be public.
	/// </remarks>
	[DataContract]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "AzureAD", Justification = "Brand name")]
	public class AzureADGraph {
		#region Public Properties

		/// <summary>
		/// Gets or sets the firstname.
		/// </summary>
		/// <value> The first name. </value>
		[DataMember(Name = "givenName")]
		public string GivenName { get; set; }

		/// <summary>
		/// Gets or sets the lastname.
		/// </summary>
		/// <value> The last name. </value>
		[DataMember(Name = "surname")]
		public string Surname { get; set; }

		/// <summary>
		/// Gets or sets the email.
		/// </summary>
		/// <value> The email. </value>
		[DataMember(Name = "userPrincipalName")]
		public string UserPrincipalName { get; set; }

		/// <summary>
		/// Gets or sets the fullname.
		/// </summary>
		/// <value> The fullname. </value>
		[DataMember(Name = "displayName")]
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value> The id. </value>
		[DataMember(Name = "objectId")]
		public string ObjectId { get; set; }
		#endregion
	}
}
