//-----------------------------------------------------------------------
// <copyright file="AzureADHeader.cs" company="Microsoft">
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
	/// Contains header of AzureAD JWT token.
	/// </summary>
	/// <remarks>
	/// Technically, this class doesn't need to be public, but because we want to make it serializable in medium trust, it has to be public.
	/// </remarks>
	[DataContract]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "AzureAD", Justification = "Brand name")]

	public class AzureADHeader
	{
		#region Public Properties

		/// <summary>
		/// Gets or sets the type of token. Will always be JWT
		/// </summary>
		/// <value> The type of token. </value>
		[DataMember(Name = "typ")]
		public string Typ { get; set; }

		/// <summary>
		/// Gets or sets the algo of the header.
		/// </summary>
		/// <value> The algo of encoding. </value>
		[DataMember(Name = "alg")]
		public string Alg { get; set; }

		/// <summary>
		/// Gets or sets the thumbprint of the header.
		/// </summary>
		/// <value> The thumbprint of the cert used to encode. </value>
		[DataMember(Name = "x5t")]
		public string X5t { get; set; }

		#endregion
	}
}
