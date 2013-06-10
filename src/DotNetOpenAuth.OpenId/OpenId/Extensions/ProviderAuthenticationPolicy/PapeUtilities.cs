//-----------------------------------------------------------------------
// <copyright file="PapeUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Utility methods for use by the PAPE extension.
	/// </summary>
	internal static class PapeUtilities {
		/// <summary>
		/// Looks at the incoming fields and figures out what the aliases and name spaces for auth level types are.
		/// </summary>
		/// <param name="fields">The incoming message data in which to discover TypeURIs and aliases.</param>
		/// <returns>The <see cref="AliasManager"/> initialized with the given data.</returns>
		internal static AliasManager FindIncomingAliases(IDictionary<string, string> fields) {
			AliasManager aliasManager = new AliasManager();

			foreach (var pair in fields) {
				if (!pair.Key.StartsWith(Constants.AuthLevelNamespaceDeclarationPrefix, StringComparison.Ordinal)) {
					continue;
				}

				string alias = pair.Key.Substring(Constants.AuthLevelNamespaceDeclarationPrefix.Length);
				aliasManager.SetAlias(alias, pair.Value);
			}

			aliasManager.SetPreferredAliasesWhereNotSet(Constants.AssuranceLevels.PreferredTypeUriToAliasMap);

			return aliasManager;
		}

		/// <summary>
		/// Concatenates a sequence of strings using a space as a separator.
		/// </summary>
		/// <param name="values">The elements to concatenate together..</param>
		/// <returns>The concatenated string of elements.</returns>
		/// <exception cref="FormatException">Thrown if any element in the sequence includes a space.</exception>
		internal static string ConcatenateListOfElements(IEnumerable<string> values) {
			Requires.NotNull(values, "values");

			StringBuilder valuesList = new StringBuilder();
			foreach (string value in values.Distinct()) {
				if (value.Contains(" ")) {
					throw new FormatException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidUri, value));
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
