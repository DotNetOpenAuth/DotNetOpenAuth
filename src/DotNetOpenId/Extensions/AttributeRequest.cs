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

		public AttributeResponse Respond(params string[] values) {
			if (values == null) throw new ArgumentNullException("values");
			if (values.Length > Count) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
			   Strings.AttributeTooManyValues, Count, TypeUri, values.Length));
			return new AttributeResponse {
				TypeUri = this.TypeUri,
				Values = values,
			};
		}
	}
}
