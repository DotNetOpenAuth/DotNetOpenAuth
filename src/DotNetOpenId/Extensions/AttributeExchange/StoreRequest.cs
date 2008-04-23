using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Globalization;

namespace DotNetOpenId.Extensions.AttributeExchange {
	/// <summary>
	/// The Attribute Exchange Store message, request leg.
	/// </summary>
	public sealed class StoreRequest : IExtensionRequest {
		readonly string Mode = "store_request";

		List<AttributeValues> attributesProvided = new List<AttributeValues>();
		/// <summary>
		/// Lists all the attributes that are included in the store request.
		/// </summary>
		public IEnumerable<AttributeValues> Attributes {
			get { return attributesProvided; }
		}
		
		/// <summary>
		/// Used by the Relying Party to add a given attribute with one or more values 
		/// to the request for storage.
		/// </summary>
		public void AddAttribute(AttributeValues attribute) {
			if (attribute == null) throw new ArgumentNullException("attribute");
			if (containsAttribute(attribute.TypeUri)) throw new ArgumentException(
				  string.Format(CultureInfo.CurrentCulture, Strings.AttributeAlreadyAdded, attribute.TypeUri));
			attributesProvided.Add(attribute);
		}
		/// <summary>
		/// Used by the Provider to gets the value(s) associated with a given attribute
		/// that should be stored.
		/// </summary>
		public AttributeValues GetAttribute(string attributeTypeUri) {
			foreach (var att in attributesProvided) {
				if (att.TypeUri == attributeTypeUri)
					return att;
			}
			return null;
		}
		bool containsAttribute(string typeUri) {
			return GetAttribute(typeUri) != null;
		}

		#region IExtensionRequest Members
		string IExtension.TypeUri { get { return Constants.TypeUri; } }

		IDictionary<string, string> IExtensionRequest.Serialize(RelyingParty.IAuthenticationRequest authenticationRequest) {
			var fields = new Dictionary<string, string> {
				{ "mode", Mode },
			};

			FetchResponse.SerializeAttributes(fields, attributesProvided);

			return fields;
		}

		bool IExtensionRequest.Deserialize(IDictionary<string, string> fields, DotNetOpenId.Provider.IRequest request) {
			if (fields == null) return false;
			string mode;
			fields.TryGetValue("mode", out mode);
			if (mode != Mode) return false;

			foreach (var att in FetchResponse.DeserializeAttributes(fields))
				AddAttribute(att);

			return true;
		}

		#endregion
	}
}
