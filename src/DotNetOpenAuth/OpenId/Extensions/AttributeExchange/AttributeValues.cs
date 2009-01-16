//-----------------------------------------------------------------------
// <copyright file="AttributeValues.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Collections.ObjectModel;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An individual attribute's value(s) as supplied by an OpenID Provider
	/// in response to a prior request by an OpenID Relying Party as part of
	/// a fetch request, or by a relying party as part of a store request.
	/// </summary>
	[Serializable]
	public class AttributeValues {
		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeValues"/> class.
		/// </summary>
		/// <remarks>
		/// This is internal because web sites should be using the
		/// <see cref="AttributeRequest.Respond"/> method to instantiate.
		/// </remarks>
		internal AttributeValues() {
			this.Values = new List<string>(1);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeValues"/> class.
		/// </summary>
		/// <param name="typeUri">The TypeURI of the attribute whose values are being provided.</param>
		internal AttributeValues(string typeUri) {
			ErrorUtilities.VerifyNonZeroLength(typeUri, "typeUri");

			this.TypeUri = typeUri;
			this.Values = new List<string>(1);
		}

		/// <summary>
		/// Instantiates an <see cref="AttributeValues"/> object.
		/// </summary>
		public AttributeValues(string typeUri, params string[] values) {
			ErrorUtilities.VerifyNonZeroLength(typeUri, "typeUri");

			this.TypeUri = typeUri;
			this.Values = (IList<string>)values ?? EmptyList<string>.Instance;
		}

		/// <summary>
		/// Gets the URI uniquely identifying the attribute whose value is being supplied.
		/// </summary>
		public string TypeUri { get; internal set; }

		/// <summary>
		/// Gets the values supplied by the Provider.
		/// </summary>
		public IList<string> Values { get; private set; }
	}
}
