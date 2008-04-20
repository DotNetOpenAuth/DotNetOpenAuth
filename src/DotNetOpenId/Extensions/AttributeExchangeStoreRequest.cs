using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Globalization;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The Attribute Exchange Store message, request leg.
	/// </summary>
	public class AttributeExchangeStoreRequest : IExtensionRequest {
		readonly string Mode = "store_request";

		List<AttributeValues> attributesProvided = new List<AttributeValues>();
		public IEnumerable<AttributeValues> Attributes {
			get { return attributesProvided; }
		}
		
		public void AddAttribute(AttributeValues attribute) {
			if (attribute == null) throw new ArgumentNullException("attribute");
			if (containsAttribute(attribute.TypeUri)) throw new ArgumentException(
				  string.Format(CultureInfo.CurrentCulture, Strings.AttributeAlreadyAdded, attribute.TypeUri));
			attributesProvided.Add(attribute);
		}
		public AttributeValues GetAttribute(string typeUri) {
			foreach (var att in attributesProvided) {
				if (att.TypeUri == typeUri)
					return att;
			}
			return null;
		}
		bool containsAttribute(string typeUri) {
			return GetAttribute(typeUri) != null;
		}

		#region IExtensionRequest Members
		string IExtensionRequest.TypeUri { get { return Constants.ae.ns; } }

		IDictionary<string, string> IExtensionRequest.GetFields(RelyingParty.IAuthenticationRequest authenticationRequest) {
			var fields = new Dictionary<string, string> {
				{ "mode", Mode },
			};

			AttributeExchangeFetchResponse.SerializeAttributes(fields, attributesProvided);

			return fields;
		}

		bool IExtensionRequest.SetFields(IDictionary<string, string> fields, DotNetOpenId.Provider.IRequest request) {
			if (fields == null) return false;
			string mode;
			fields.TryGetValue("mode", out mode);
			if (mode != Mode) return false;

			foreach (var att in AttributeExchangeFetchResponse.DeserializeAttributes(fields))
				AddAttribute(att);

			return true;
		}

		#endregion
	}
}
