//-----------------------------------------------------------------------
// <copyright file="StoreRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Linq;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The Attribute Exchange Store message, request leg.
	/// </summary>
	public sealed class StoreRequest : ExtensionBase {
		private const string Mode = "store_request";

		/// <summary>
		/// The list of provided attribute values.  This field will never be null.
		/// </summary>
		private readonly List<AttributeValues> attributesProvided = new List<AttributeValues>();

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreRequest"/> class.
		/// </summary>
		public StoreRequest()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

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
			ErrorUtilities.VerifyArgumentNotNull(attribute, "attribute");
			ErrorUtilities.VerifyArgumentNamed(!ContainsAttribute(attribute.TypeUri), "attribute", OpenIdStrings.AttributeAlreadyAdded, attribute.TypeUri);
			attributesProvided.Add(attribute);
		}

		/// <summary>
		/// Used by the Relying Party to add a given attribute with one or more values 
		/// to the request for storage.
		/// </summary>
		public void AddAttribute(string typeUri, params string[] values) {
			AddAttribute(new AttributeValues(typeUri, values));
		}

		/// <summary>
		/// Used by the Provider to gets the value(s) associated with a given attribute
		/// that should be stored.
		/// </summary>
		public AttributeValues GetAttribute(string attributeTypeUri) {
			return this.attributesProvided.SingleOrDefault(attribute => string.Equals(attribute.TypeUri, attributeTypeUri, StringComparison.Ordinal));
		}

		#region IExtensionRequest Members
		string IExtension.TypeUri { get { return Constants.TypeUri; } }
		IEnumerable<string> IExtension.AdditionalSupportedTypeUris {
			get { return new string[0]; }
		}

		IDictionary<string, string> IExtensionRequest.Serialize(RelyingParty.IAuthenticationRequest authenticationRequest) {
			var fields = new Dictionary<string, string> {
				{ "mode", Mode },
			};

			FetchResponse.SerializeAttributes(fields, attributesProvided);

			return fields;
		}

		bool IExtensionRequest.Deserialize(IDictionary<string, string> fields, DotNetOpenId.Provider.IRequest request, string typeUri) {
			if (fields == null) return false;
			string mode;
			fields.TryGetValue("mode", out mode);
			if (mode != Mode) return false;

			foreach (var att in FetchResponse.DeserializeAttributes(fields))
				AddAttribute(att);

			return true;
		}

		#endregion

		private bool ContainsAttribute(string typeUri) {
			return GetAttribute(typeUri) != null;
		}
	}
}
