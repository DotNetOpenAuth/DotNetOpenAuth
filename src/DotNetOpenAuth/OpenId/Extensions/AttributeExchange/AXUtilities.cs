//-----------------------------------------------------------------------
// <copyright file="AXUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Helper methods shared by multiple messages in the Attribute Exchange extension.
	/// </summary>
	internal static class AXUtilities {
		/// <summary>
		/// Serializes a set of attribute values to a dictionary of fields to send in the message.
		/// </summary>
		/// <param name="fields">The dictionary to fill with serialized attributes.</param>
		/// <param name="attributes">The attributes.</param>
		internal static void SerializeAttributes(IDictionary<string, string> fields, IEnumerable<AttributeValues> attributes) {
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");
			ErrorUtilities.VerifyArgumentNotNull(attributes, "attributes");

			AliasManager aliasManager = new AliasManager();
			foreach (var att in attributes) {
				string alias = aliasManager.GetAlias(att.TypeUri);
				fields.Add("type." + alias, att.TypeUri);
				if (att.Values == null) {
					continue;
				}
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

		/// <summary>
		/// Deserializes attribute values from an incoming set of message data.
		/// </summary>
		/// <param name="fields">The data coming in with the message.</param>
		/// <returns>The attribute values found in the message.</returns>
		internal static IEnumerable<AttributeValues> DeserializeAttributes(IDictionary<string, string> fields) {
			AliasManager aliasManager = ParseAliases(fields);
			foreach (string alias in aliasManager.Aliases) {
				AttributeValues att = new AttributeValues(aliasManager.ResolveAlias(alias));
				int count = 1;
				bool countSent = false;
				string countString;
				if (fields.TryGetValue("count." + alias, out countString)) {
					if (!int.TryParse(countString, out count) || count <= 0) {
						Logger.ErrorFormat("Failed to parse count.{0} value to a positive integer.", alias);
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
					if (fields.TryGetValue("value." + alias, out value)) {
						att.Values.Add(value);
					} else {
						Logger.ErrorFormat("Missing value for attribute '{0}'.", att.TypeUri);
						continue;
					}
				}
				yield return att;
			}
		}

		/// <summary>
		/// Reads through the attributes included in the response to discover
		/// the alias-TypeURI relationships.
		/// </summary>
		/// <param name="fields">The data included in the extension message.</param>
		/// <returns>The alias manager that provides lookup between aliases and type URIs.</returns>
		private static AliasManager ParseAliases(IDictionary<string, string> fields) {
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			AliasManager aliasManager = new AliasManager();
			const string TypePrefix = "type.";
			foreach (var pair in fields) {
				if (!pair.Key.StartsWith(TypePrefix, StringComparison.Ordinal)) {
					continue;
				}
				string alias = pair.Key.Substring(TypePrefix.Length);
				if (alias.IndexOfAny(FetchRequest.IllegalAliasCharacters) >= 0) {
					Logger.ErrorFormat("Illegal characters in alias name '{0}'.", alias);
					continue;
				}
				aliasManager.SetAlias(alias, pair.Value);
			}
			return aliasManager;
		}
	}
}
