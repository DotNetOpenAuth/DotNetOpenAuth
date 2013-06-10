//-----------------------------------------------------------------------
// <copyright file="AttributeValues.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// An individual attribute's value(s) as supplied by an OpenID Provider
	/// in response to a prior request by an OpenID Relying Party as part of
	/// a fetch request, or by a relying party as part of a store request.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{TypeUri}")]
	public class AttributeValues {
		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeValues"/> class.
		/// </summary>
		/// <param name="typeUri">The TypeURI that uniquely identifies the attribute.</param>
		/// <param name="values">The values for the attribute.</param>
		public AttributeValues(string typeUri, params string[] values) {
			Requires.NotNullOrEmpty(typeUri, "typeUri");

			this.TypeUri = typeUri;
			this.Values = (IList<string>)values ?? EmptyList<string>.Instance;
		}

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
			Requires.NotNullOrEmpty(typeUri, "typeUri");

			this.TypeUri = typeUri;
			this.Values = new List<string>(1);
		}

		/// <summary>
		/// Gets the URI uniquely identifying the attribute whose value is being supplied.
		/// </summary>
		public string TypeUri { get; internal set; }

		/// <summary>
		/// Gets the values supplied by the Provider.
		/// </summary>
		public IList<string> Values { get; private set; }

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
			AttributeValues other = obj as AttributeValues;
			if (other == null) {
				return false;
			}

			if (this.TypeUri != other.TypeUri) {
				return false;
			}

			if (!MessagingUtilities.AreEquivalent<string>(this.Values, other.Values)) {
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
			int hashCode = 0;
			unchecked {
				if (this.TypeUri != null) {
					hashCode += this.TypeUri.GetHashCode();
				}

				foreach (string value in this.Values) {
					hashCode += value.GetHashCode();
				}
			}

			return hashCode;
		}
	}
}
