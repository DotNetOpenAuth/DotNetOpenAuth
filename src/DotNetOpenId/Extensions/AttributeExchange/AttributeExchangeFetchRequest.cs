using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.Extensions.AttributeExchange {
	/// <summary>
	/// The Attribute Exchange Fetch message, request leg.
	/// </summary>
	public class AttributeExchangeFetchRequest : IExtensionRequest {
		readonly string Mode = "fetch_request";

		List<AttributeRequest> attributesRequested = new List<AttributeRequest>();
		public IEnumerable<AttributeRequest> Attributes {
			get { return attributesRequested; }
		}
		
		public void AddAttribute(AttributeRequest attribute) {
			if (attribute == null) throw new ArgumentNullException("attribute");
			if (containsAttribute(attribute.TypeUri)) throw new ArgumentException(
				string.Format(CultureInfo.CurrentCulture,
				Strings.AttributeAlreadyAdded, attribute.TypeUri), "attribute");
			attributesRequested.Add(attribute);
		}
		public AttributeRequest GetAttribute(string typeUri) {
			foreach (var attribute in attributesRequested)
				if (string.Equals(attribute.TypeUri, typeUri, StringComparison.Ordinal))
					return attribute;
			return null;
		}
		bool containsAttribute(string typeUri) {
			return GetAttribute(typeUri) != null;
		}

		/// <summary>
		/// If set, the OpenID Provider may re-post the fetch response message to the 
		/// specified URL at some time after the initial response has been sent, using an
		/// OpenID Authentication Positive Assertion to inform the relying party of updates
		/// to the requested fields.
		/// </summary>
		public Uri UpdateUrl { get; set; }

		#region IExtensionRequest Members
		string IExtension.TypeUri { get { return Constants.ns; } }

		IDictionary<string, string> IExtensionRequest.Serialize(RelyingParty.IAuthenticationRequest authenticationRequest) {
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
				fields.Add("count." + alias, att.Count.ToString());
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

		bool IExtensionRequest.Deserialize(IDictionary<string, string> fields, DotNetOpenId.Provider.IRequest request) {
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
				if (TraceUtil.Switch.TraceError)
					Trace.TraceError("Attribute Exchange extension did not provide any aliases in the if_available or required lists.");
				return false;
			}
			AliasManager aliasManager = new AliasManager();
			foreach (var alias in allAliases) {
				string typeUri;
				if (fields.TryGetValue("type." + alias, out typeUri)) {
					aliasManager.SetAlias(alias, typeUri);
					AttributeRequest att = new AttributeRequest {
						TypeUri = typeUri,
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
								if (TraceUtil.Switch.TraceError)
									Trace.TraceError("count." + alias + " could not be parsed into a positive integer.");
							}
						}
					} else {
						att.Count = 1;
					}
					AddAttribute(att);
				} else {
					if (TraceUtil.Switch.TraceError)
						Trace.TraceError("Type URI definition of alias " + alias + " is missing.");
				}
			}

			return true;
		}

		List<string> parseAliasList(string aliasList) {
			List<string> result = new List<string>();
			if (string.IsNullOrEmpty(aliasList)) return result;
			if (aliasList.Contains(".") || aliasList.Contains("\n")) {
				if (TraceUtil.Switch.TraceError)
					Trace.TraceError("Illegal characters found in Attribute Exchange alias list.");
				return result;
			}
			result.AddRange(aliasList.Split(','));
			return result;
		}

		#endregion
	}
}
