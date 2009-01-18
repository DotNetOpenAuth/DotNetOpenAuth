//-----------------------------------------------------------------------
// <copyright file="PapeUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal static class PapeUtilities {
		/// <summary>
		/// Looks at the incoming fields and figures out what the aliases and name spaces for auth level types are.
		/// </summary>
		internal static AliasManager FindIncomingAliases(IDictionary<string, string> fields) {
			AliasManager aliasManager = new AliasManager();

			foreach (var pair in fields) {
				if (!pair.Key.StartsWith(Constants.AuthLevelNamespaceDeclarationPrefix, StringComparison.Ordinal)) {
					continue;
				}

				string alias = pair.Key.Substring(Constants.AuthLevelNamespaceDeclarationPrefix.Length);
				aliasManager.SetAlias(alias, pair.Value);
			}

			aliasManager.SetPreferredAliasesWhereNotSet(Constants.AuthenticationLevels.PreferredTypeUriToAliasMap);

			return aliasManager;
		}

		internal static string ConcatenateListOfElements(IList<string> values) {
			ErrorUtilities.VerifyArgumentNotNull(values, "values");

			StringBuilder valuesList = new StringBuilder();
			foreach (string value in values.Distinct()) {
				if (value.Contains(" ")) {
					throw new FormatException(string.Format(CultureInfo.CurrentCulture,
						OpenIdStrings.InvalidUri, value));
				}
				valuesList.Append(value);
				valuesList.Append(" ");
			}
			if (valuesList.Length > 0) {
				valuesList.Length -= 1; // remove trailing space
			}
			return valuesList.ToString();
		}
	}
}
