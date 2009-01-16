//-----------------------------------------------------------------------
// <copyright file="FetchRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Linq;
	using System.Globalization;
	using System.Diagnostics;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The Attribute Exchange Fetch message, request leg.
	/// </summary>
	public sealed class FetchRequest : ExtensionBase {
		private const string Mode = "fetch_request";

		/// <summary>
		/// The list of requested attributes.  This field will never be null.
		/// </summary>
		private readonly List<AttributeRequest> attributesRequested = new List<AttributeRequest>();

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchRequest"/> class.
		/// </summary>
		public FetchRequest()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets a sequence of the attributes whose values are requested by the Relying Party.
		/// </summary>
		public IEnumerable<AttributeRequest> Attributes {
			get { return attributesRequested; }
		}

		/// <summary>
		/// If set, the OpenID Provider may re-post the fetch response message to the 
		/// specified URL at some time after the initial response has been sent, using an
		/// OpenID Authentication Positive Assertion to inform the relying party of updates
		/// to the requested fields.
		/// </summary>
		public Uri UpdateUrl { get; set; }

		/// <summary>
		/// Used by the Relying Party to add a request for the values of a given attribute.
		/// </summary>
		public void AddAttribute(AttributeRequest attribute) {
			ErrorUtilities.VerifyArgumentNotNull(attribute, "attribute");
			ErrorUtilities.VerifyArgumentNamed(!ContainsAttribute(attribute.TypeUri), "attribute", OpenIdStrings.AttributeAlreadyAdded, attribute.TypeUri);
			attributesRequested.Add(attribute);
		}

		/// <summary>
		/// Used by the Provider to find out whether the value(s) of a given attribute is requested.
		/// </summary>
		/// <returns>Null if the Relying Party did not ask for the values of the given attribute.</returns>
		public AttributeRequest GetAttribute(string attributeTypeUri) {
			return this.attributesRequested.SingleOrDefault(attribute => string.Equals(attribute.TypeUri, attributeTypeUri, StringComparison.Ordinal));
		}

		#region IExtensionRequest Members

		IDictionary<string, string> IExtensionRequest.Serialize(RelyingParty.IAuthenticationRequest authenticationRequest){
			var fields = new Dictionary<string, string> {
				{ "mode", Mode },
			};
			if (UpdateUrl != null)
				fields.Add("update_url", UpdateUrl.AbsoluteUri);

			List<string> requiredAliases = new List<string>(), optionalAliases = new List<string>();
			AliasManager aliasManager = new AliasManager();
			foreach (var att in attributesRequested) {
				string alias = aliasManager.GetAlias(att.TypeUri);
				// define the alias<->typeUri mapping
				fields.Add("type." + alias, att.TypeUri);
				// set how many values the relying party wants max
				fields.Add("count." + alias, att.Count.ToString(CultureInfo.InvariantCulture));
				if (att.IsRequired)
					requiredAliases.Add(alias);
				else
					optionalAliases.Add(alias);
			}

			// Set optional/required lists
			if (optionalAliases.Count > 0)
				fields.Add("if_available", string.Join(",", optionalAliases.ToArray()));
			if (requiredAliases.Count > 0)
				fields.Add("required", string.Join(",", requiredAliases.ToArray()));

			return fields;
		}

		bool IExtensionRequest.Deserialize(IDictionary<string, string> fields, DotNetOpenId.Provider.IRequest request, string typeUri) {
			if (fields == null) return false;
			string mode;
			fields.TryGetValue("mode", out mode);
			if (mode != Mode) return false;

			string updateUrl;
			fields.TryGetValue("update_url", out updateUrl);
			Uri updateUri;
			if (Uri.TryCreate(updateUrl, UriKind.Absolute, out updateUri))
				UpdateUrl = updateUri;

			string requiredAliasString, optionalAliasString;
			fields.TryGetValue("if_available", out optionalAliasString);
			fields.TryGetValue("required", out requiredAliasString);
			var requiredAliases = parseAliasList(requiredAliasString);
			var optionalAliases = parseAliasList(optionalAliasString);
			// if an alias shows up in both lists, an exception will result implicitly.
			var allAliases = new List<string>(requiredAliases.Count + optionalAliases.Count);
			allAliases.AddRange(requiredAliases);
			allAliases.AddRange(optionalAliases);
			if (allAliases.Count == 0) {
				Logger.Error("Attribute Exchange extension did not provide any aliases in the if_available or required lists.");
				return false;
			}
			AliasManager aliasManager = new AliasManager();
			foreach (var alias in allAliases) {
				string attributeTypeUri;
				if (fields.TryGetValue("type." + alias, out attributeTypeUri)) {
					aliasManager.SetAlias(alias, attributeTypeUri);
					AttributeRequest att = new AttributeRequest {
						TypeUri = attributeTypeUri,
						IsRequired = requiredAliases.Contains(alias),
					};
					string countString;
					if (fields.TryGetValue("count." + alias, out countString)) {
						if (countString == "unlimited")
							att.Count = int.MaxValue;
						else {
							int count;
							if (int.TryParse(countString, out count) && count > 0) {
								att.Count = count;
							} else {
								Logger.Error("count." + alias + " could not be parsed into a positive integer.");
							}
						}
					} else {
						att.Count = 1;
					}
					AddAttribute(att);
				} else {
					Logger.Error("Type URI definition of alias " + alias + " is missing.");
				}
			}

			return true;
		}

		static List<string> parseAliasList(string aliasList) {
			List<string> result = new List<string>();
			if (string.IsNullOrEmpty(aliasList)) return result;
			if (aliasList.Contains(".") || aliasList.Contains("\n")) {
				Logger.ErrorFormat("Illegal characters found in Attribute Exchange alias list.");
				return result;
			}
			result.AddRange(aliasList.Split(','));
			return result;
		}

		#endregion

		private bool ContainsAttribute(string typeUri) {
			return GetAttribute(typeUri) != null;
		}
	}
}
