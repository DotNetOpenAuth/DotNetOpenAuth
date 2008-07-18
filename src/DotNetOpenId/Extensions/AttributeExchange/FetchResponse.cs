using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.Extensions.AttributeExchange {
	/// <summary>
	/// The Attribute Exchange Fetch message, response leg.
	/// </summary>
	public sealed class FetchResponse : IExtensionResponse {
		readonly string Mode = "fetch_response";

		List<AttributeValues> attributesProvided = new List<AttributeValues>();
		/// <summary>
		/// Enumerates over all the attributes included by the Provider.
		/// </summary>
		public IEnumerable<AttributeValues> Attributes {
			get { return attributesProvided; }
		}
		/// <summary>
		/// Used by the Provider to add attributes to the response for the relying party.
		/// </summary>
		public void AddAttribute(AttributeValues attribute) {
			if (attribute == null) throw new ArgumentNullException("attribute");
			if (containsAttribute(attribute.TypeUri)) throw new ArgumentException(
				  string.Format(CultureInfo.CurrentCulture, Strings.AttributeAlreadyAdded, attribute.TypeUri));
			attributesProvided.Add(attribute);
		}
		/// <summary>
		/// Used by the Relying Party to get the value(s) returned by the OpenID Provider 
		/// for a given attribute, or null if that attribute was not provided.
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

		/// <summary>
		/// Whether the OpenID Provider intends to honor the request for updates.
		/// </summary>
		public bool UpdateUrlSupported { get { return UpdateUrl != null; } }
		/// <summary>
		/// The URL the OpenID Provider will post updates to.  Must be set if the Provider
		/// supports and will use this feature.
		/// </summary>
		public Uri UpdateUrl { get; set; }

		#region IExtensionResponse Members
		string IExtension.TypeUri { get { return Constants.TypeUri; } }

		IDictionary<string, string> IExtensionResponse.Serialize(Provider.IRequest authenticationRequest) {
			var fields = new Dictionary<string, string> {
				{ "mode", Mode },
			};

			if (UpdateUrlSupported)
				fields.Add("update_url", UpdateUrl.AbsoluteUri);

			SerializeAttributes(fields, attributesProvided);

			return fields;
		}

		internal static void SerializeAttributes(Dictionary<string, string> fields, IEnumerable<AttributeValues> attributes) {
			Debug.Assert(fields != null && attributes != null);
			AliasManager aliasManager = new AliasManager();
			foreach (var att in attributes) {
				string alias = aliasManager.GetAlias(att.TypeUri);
				fields.Add("type." + alias, att.TypeUri);
				if (att.Values == null) continue;
				if (att.Values.Count != 1) {
					fields.Add("count." + alias, att.Values.Count.ToString(CultureInfo.InvariantCulture));
					for (int i = 0; i < att.Values.Count; i++) {
						fields.Add(string.Format(CultureInfo.InvariantCulture, "value.{0}.{1}", alias, i + 1), att.Values[i]);
					}
				} else {
					fields.Add("value." + alias, att.Values[0]);
				}
			}
		}

		bool IExtensionResponse.Deserialize(IDictionary<string, string> fields, IAuthenticationResponse response) {
			if (fields == null) return false;
			string mode;
			fields.TryGetValue("mode", out mode);
			if (mode != Mode) return false;

			string updateUrl;
			fields.TryGetValue("update_url", out updateUrl);
			Uri updateUri;
			if (Uri.TryCreate(updateUrl, UriKind.Absolute, out updateUri))
				UpdateUrl = updateUri;

			foreach (var att in DeserializeAttributes(fields))
				AddAttribute(att);

			return true;
		}

		internal static IEnumerable<AttributeValues> DeserializeAttributes(IDictionary<string, string> fields) {
			AliasManager aliasManager = parseAliases(fields);
			foreach (string alias in aliasManager.Aliases) {
				AttributeValues att = new AttributeValues(aliasManager.ResolveAlias(alias));
				int count = 1;
				bool countSent = false;
				string countString;
				if (fields.TryGetValue("count." + alias, out countString)) {
					if (!int.TryParse(countString, out count) || count <= 0) {
						Logger.Error("Failed to parse count.{0} value to a positive integer.");
						continue;
					}
					countSent = true;
				}
				if (countSent) {
					for (int i = 1; i <= count; i++) {
						string value;
						if (fields.TryGetValue(string.Format(CultureInfo.InvariantCulture, "value.{0}.{1}", alias, i), out value)) {
							att.Values.Add(value);
						} else {
							Logger.ErrorFormat("Missing value for attribute '{0}'.", att.TypeUri);
							continue;
						}
					}
				} else {
					string value;
					if (fields.TryGetValue("value." + alias, out value))
						att.Values.Add(value);
					else {
						Logger.ErrorFormat("Missing value for attribute '{0}'.", att.TypeUri);
						continue;
					}
				}
				yield return att;
			}
		}

		static AliasManager parseAliases(IDictionary<string, string> fields) {
			Debug.Assert(fields != null);
			AliasManager aliasManager = new AliasManager();
			foreach (var pair in fields) {
				if (!pair.Key.StartsWith("type.", StringComparison.Ordinal)) continue;
				string alias = pair.Key.Substring(5);
				if (alias.IndexOfAny(new[] { '.', ',', ':' }) >= 0) {
					Logger.ErrorFormat("Illegal characters in alias name '{0}'.", alias);
					continue;
				}
				aliasManager.SetAlias(alias, pair.Value);
			}
			return aliasManager;
		}

		#endregion
	}
}
