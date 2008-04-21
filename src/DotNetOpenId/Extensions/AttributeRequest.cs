using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// An individual attribute to be requested of the OpenID Provider using
	/// the Attribute Exchange extension.
	/// </summary>
	public class AttributeRequest {
		public AttributeRequest() { }
		/// <summary>
		/// Instantiates a new <see cref="AttributeRequest"/> with IsRequired = false, Count = 1.
		/// </summary>
		public AttributeRequest(string typeUri) {
			if (string.IsNullOrEmpty(typeUri)) throw new ArgumentNullException("typeUri");
			TypeUri = typeUri;
		}
		/// <summary>
		/// Instantiates a new <see cref="AttributeRequest"/> with Count = 1.
		/// </summary>
		public AttributeRequest(string typeUri, bool isRequired)
			: this(typeUri) {
			IsRequired = isRequired;
		}
		/// <summary>
		/// Instantiates a new <see cref="AttributeRequest"/>.
		/// </summary>
		public AttributeRequest(string typeUri, bool isRequired, int count) : this(typeUri, isRequired) {
			Count = count;
		}
		/// <summary>
		/// The URI uniquely identifying the attribute being requested.
		/// </summary>
		public string TypeUri;
		/// <summary>
		/// Whether the relying party considers this a required field.
		/// Note that even if set to true, the Provider may not provide the value.
		/// </summary>
		public bool IsRequired;
		int count = 1;
		/// <summary>
		/// The maximum number of values for this attribute the 
		/// Relying Party wishes to receive from the OpenID Provider.
		/// A value of int.MaxValue is considered infinity.
		/// </summary>
		public int Count {
			get { return count; }
			set {
				if (value <= 0) throw new ArgumentOutOfRangeException("value");
				count = value;
			}
		}

		public AttributeValues Respond(params string[] values) {
			if (values == null) throw new ArgumentNullException("values");
			if (values.Length > Count) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
			   Strings.AttributeTooManyValues, Count, TypeUri, values.Length));
			return new AttributeValues {
				TypeUri = this.TypeUri,
				Values = values,
			};
		}
	}
}
