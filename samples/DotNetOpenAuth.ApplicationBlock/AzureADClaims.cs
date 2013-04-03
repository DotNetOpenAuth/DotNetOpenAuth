//-----------------------------------------------------------------------
// <copyright file="AzureADClaims.cs" company="Microsoft">
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
	/// Contains clains of a AzureAD token.
	/// </summary>
	/// <remarks>
	/// Technically, this class doesn't need to be public, but because we want to make it serializable in medium trust, it has to be public.
	/// </remarks>
	[DataContract]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "AzureAD", Justification = "Brand name")]
	public class AzureADClaims
	{
		#region Public Properties

		/// <summary>
		/// Gets or sets the audience.
		/// </summary>
		/// <value> The audience token is valid for. </value>
		[DataMember(Name = "aud")]
		public string Aud { get; set; }

		/// <summary>
		/// Gets or sets the issuer.
		/// </summary>
		/// <value> The issuer. </value>
		[DataMember(Name = "iss")]
		public string Iss { get; set; }

		/// <summary>
		/// Gets or sets the early expiry time.
		/// </summary>
		/// <value> The early expiry time. </value>
		[DataMember(Name = "nbf")]
		public string Nbf { get; set; }

		/// <summary>
		/// Gets or sets the expiry time.
		/// </summary>
		/// <value> The expiry time. </value>
		[DataMember(Name = "exp")]
		public string Exp { get; set; }

		/// <summary>
		/// Gets or sets the id of the user.
		/// </summary>
		/// <value> The id of the user. </value>
		[DataMember(Name = "oid")]
		public string Oid { get; set; }

		/// <summary>
		/// Gets or sets the id of the tenant.
		/// </summary>
		/// <value> The tenant . </value>
		[DataMember(Name = "tid")]
		public string Tid { get; set; }

		/// <summary>
		/// Gets or sets the appid of application.
		/// </summary>
		/// <value> The id of the application. </value>
		[DataMember(Name = "appid")]
		public string Appid { get; set; }
		#endregion
	}
}
