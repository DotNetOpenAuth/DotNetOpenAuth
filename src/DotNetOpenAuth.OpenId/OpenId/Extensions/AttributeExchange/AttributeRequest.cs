//-----------------------------------------------------------------------
// <copyright file="AttributeRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Diagnostics;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// An individual attribute to be requested of the OpenID Provider using
	/// the Attribute Exchange extension.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{TypeUri} (required: {IsRequired}) ({Count})")]
	public class AttributeRequest {
		/// <summary>
		/// Backing field for the <see cref="Count"/> property.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int count = 1;

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
			Requires.NotNullOrEmpty(typeUri, "typeUri");
			this.TypeUri = typeUri;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeRequest"/> class
		/// with <see cref="Count"/> = 1.
		/// </summary>
		/// <param name="typeUri">The unique TypeURI for that describes the attribute being sought.</param>
		/// <param name="isRequired">A value indicating whether the Relying Party considers this attribute to be required for registration.</param>
		public AttributeRequest(string typeUri, bool isRequired)
			: this(typeUri) {
			this.IsRequired = isRequired;
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
		/// Gets or sets the URI uniquely identifying the attribute being requested.
		/// </summary>
		public string TypeUri { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the relying party considers this a required field.
		/// Note that even if set to true, the Provider may not provide the value.
		/// </summary>
		public bool IsRequired { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of values for this attribute the 
		/// Relying Party wishes to receive from the OpenID Provider.
		/// A value of int.MaxValue is considered infinity.
		/// </summary>
		public int Count {
			get {
				return this.count;
			}

			set {
				Requires.Range(value > 0, "value");
				this.count = value;
			}
		}

		/// <summary>
		/// Used by a Provider to create a response to a request for an attribute's value(s)
		/// using a given array of strings.
		/// </summary>
		/// <param name="values">The values for the requested attribute.</param>
		/// <returns>
		/// The newly created <see cref="AttributeValues"/> object that should be added to
		/// the <see cref="FetchResponse"/> object.
		/// </returns>
		public AttributeValues Respond(params string[] values) {
			Requires.NotNull(values, "values");
			Requires.That(values.Length <= this.Count, "values", "requires values.Length <= this.Count");
			return new AttributeValues(this.TypeUri, values);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			AttributeRequest other = obj as AttributeRequest;
			if (other == null) {
				return false;
			}

			if (this.TypeUri != other.TypeUri) {
				return false;
			}

			if (this.Count != other.Count) {
				return false;
			}

			if (this.IsRequired != other.IsRequired) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			int hashCode = this.IsRequired ? 1 : 0;
			unchecked {
				hashCode += this.Count;
				if (this.TypeUri != null) {
					hashCode += this.TypeUri.GetHashCode();
				}
			}

			return hashCode;
		}
	}
}
