//-----------------------------------------------------------------------
// <copyright file="AttributeRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Globalization;
	using System.Diagnostics;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An individual attribute to be requested of the OpenID Provider using
	/// the Attribute Exchange extension.
	/// </summary>
	public class AttributeRequest {
		/// <summary>
		/// Backing field for the <see cref="Count"/> property.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int count = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeRequest"/> class
		/// with <see cref="IsRequired"/> = false, <see cref="Count"/> = 1.
		/// </summary>
		public AttributeRequest() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeRequest"/> class
		/// with <see cref="IsRequired"/> = false, <see cref="Count"/> = 1.
		/// </summary>
		/// <param name="typeUri">The unique TypeURI for that describes the attribute being sought.</param>
		public AttributeRequest(string typeUri) {
			ErrorUtilities.VerifyNonZeroLength(typeUri, "typeUri");
			TypeUri = typeUri;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeRequest"/> class
		/// with <see cref="Count"/> = 1.
		/// </summary>
		/// <param name="typeUri">The unique TypeURI for that describes the attribute being sought.</param>
		/// <param name="isRequired">A value indicating whether the Relying Party considers this attribute to be required for registration.</param>
		public AttributeRequest(string typeUri, bool isRequired)
			: this(typeUri) {
			IsRequired = isRequired;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeRequest"/> class.
		/// </summary>
		/// <param name="typeUri">The unique TypeURI for that describes the attribute being sought.</param>
		/// <param name="isRequired">A value indicating whether the Relying Party considers this attribute to be required for registration.</param>
		/// <param name="count">The maximum number of values for this attribute the Relying Party is prepared to receive.</param>
		public AttributeRequest(string typeUri, bool isRequired, int count)
			: this(typeUri, isRequired) {
			this.Count = count;
		}

		/// <summary>
		/// The URI uniquely identifying the attribute being requested.
		/// </summary>
		public string TypeUri { get; set; }

		/// <summary>
		/// Whether the relying party considers this a required field.
		/// Note that even if set to true, the Provider may not provide the value.
		/// </summary>
		public bool IsRequired { get; set; }

		/// <summary>
		/// The maximum number of values for this attribute the 
		/// Relying Party wishes to receive from the OpenID Provider.
		/// A value of int.MaxValue is considered infinity.
		/// </summary>
		public int Count {
			get {
				return this.count;
			}

			set {
				ErrorUtilities.VerifyArgumentInRange(value > 0, "value");
				this.count = value;
			}
		}

		/// <summary>
		/// Used by a Provider to create a response to a request for an attribute's value(s)
		/// using a given array of strings.
		/// </summary>
		/// <returns>
		/// The newly created <see cref="AttributeValues"/> object that should be added to
		/// the <see cref="FetchResponse"/> object.
		/// </returns>
		public AttributeValues Respond(params string[] values) {
			ErrorUtilities.VerifyArgumentNotNull(values, "values");
			ErrorUtilities.VerifyArgument(values.Length <= Count, OpenIdStrings.AttributeTooManyValues, Count, TypeUri, values.Length);
			return new AttributeValues(TypeUri, values);
		}
	}
}
