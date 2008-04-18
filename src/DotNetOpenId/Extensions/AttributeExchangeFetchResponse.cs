using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The Attribute Exchange Fetch message, response leg.
	/// </summary>
	public class AttributeExchangeFetchResponse : IExtensionResponse {
		readonly string Mode = "fetch_response";

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
		string IExtensionResponse.TypeUri { get { return Constants.ae.ns; } }

		void IExtensionResponse.AddToResponse(Provider.IRequest authenticationRequest) {
			var fields = new Dictionary<string, string> {
				{ "mode", Mode },
			};

			if (UpdateUrlSupported)
				fields.Add("update_url", UpdateUrl.AbsoluteUri);

			SerializeAttributes(fields, attributesProvided);

			authenticationRequest.AddExtensionArguments(Constants.ae.ns, fields);
		}

		internal static void SerializeAttributes(Dictionary<string, string> fields, IEnumerable<AttributeValues> attributes) {
			AliasManager aliasManager = new AliasManager();
			foreach (var att in attributes) {
				string alias = aliasManager.GetAlias(att.TypeUri);
				fields.Add("type." + alias, att.TypeUri);
				if (att.Values.Length != 1) {
					fields.Add("count." + alias, att.Values.Length.ToString());
					for (int i = 0; i < att.Values.Length; i++) {
						fields.Add(string.Format(CultureInfo.InvariantCulture, "value.{0}.{1}", alias, i + 1), att.Values[i]);
					}
				} else {
					fields.Add("value." + alias, att.Values[0]);
				}
			}
		}

		bool IExtensionResponse.ReadFromResponse(IAuthenticationResponse response) {
			var fields = response.GetExtensionArguments(Constants.ae.ns);
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
				AttributeValues att = new AttributeValues() {
					TypeUri = aliasManager.ResolveAlias(alias),
				};
				int count = 1;
				bool countSent = false;
				string countString;
				if (fields.TryGetValue("count." + alias, out countString)) {
					if (!int.TryParse(countString, out count) || count <= 0) {
						if (TraceUtil.Switch.TraceError)
							Trace.TraceError("Failed to parse count.{0} value to a positive integer.");
						continue;
					}
					countSent = true;
				}
				att.Values = new string[count];
				if (countSent) {
					for (int i = 0; i < att.Values.Length; i++) {
						string value;
						if (fields.TryGetValue(string.Format(CultureInfo.InvariantCulture, "value.{0}.{1}", alias, i + 1), out value)) {
							att.Values[i] = value;
						} else {
							if (TraceUtil.Switch.TraceError)
								Trace.TraceError("Missing value for attribute '{0}'.", att.TypeUri);
							continue;
						}
					}
				} else {
					string value;
					if (fields.TryGetValue("value." + alias, out value))
						att.Values[0] = value;
					else {
						if (TraceUtil.Switch.TraceError)
							Trace.TraceError("Missing value for attribute '{0}'.", att.TypeUri);
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
					if (TraceUtil.Switch.TraceError)
						Trace.TraceError("Illegal characters in alias name '{0}'.", alias);
					continue;
				}
				aliasManager.SetAlias(alias, pair.Value);
			}
			return aliasManager;
		}

		#endregion
	}
}
