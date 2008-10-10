using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.Extensions.ProviderAuthenticationPolicy {
	/// <summary>
	/// The PAPE request part of an OpenID Authentication request message.
	/// </summary>
	public sealed class PolicyRequest : IExtensionRequest {
		/// <summary>
		/// Instantiates a new <see cref="PolicyRequest"/>.
		/// </summary>
		public PolicyRequest() {
			PreferredPolicies = new List<string>(1);
			PreferredAuthLevelTypes = new List<string>(1);
		}

		/// <summary>
		/// Optional. If the End User has not actively authenticated to the OP within the number of seconds specified in a manner fitting the requested policies, the OP SHOULD authenticate the End User for this request.
		/// </summary>
		/// <remarks>
		/// The OP should realize that not adhering to the request for re-authentication most likely means that the End User will not be allowed access to the services provided by the RP. If this parameter is absent in the request, the OP should authenticate the user at its own discretion.
		/// </remarks>
		public TimeSpan? MaximumAuthenticationAge { get; set; }
		/// <summary>
		/// Zero or more authentication policy URIs that the OP SHOULD conform to when authenticating the user. If multiple policies are requested, the OP SHOULD satisfy as many as it can.
		/// </summary>
		/// <value>List of authentication policy URIs obtainable from the <see cref="AuthenticationPolicies"/> class or from a custom list.</value>
		/// <remarks>
		/// If no policies are requested, the RP may be interested in other information such as the authentication age.
		/// </remarks>
		public IList<string> PreferredPolicies { get; private set; }

		/// <summary>
		/// Zero or more name spaces of the custom Assurance Level the RP requests, in the order of its preference.
		/// </summary>
		public IList<string> PreferredAuthLevelTypes { get; private set; }

		/// <summary>
		/// Tests equality between two <see cref="PolicyRequest"/> instances.
		/// </summary>
		public override bool Equals(object obj) {
			PolicyRequest other = obj as PolicyRequest;
			if (other == null) return false;
			if (MaximumAuthenticationAge != other.MaximumAuthenticationAge) return false;
			if (PreferredPolicies.Count != other.PreferredPolicies.Count) return false;
			foreach(string policy in PreferredPolicies) {
				if (!other.PreferredPolicies.Contains(policy)) return false;
			}
			if (PreferredAuthLevelTypes.Count != other.PreferredAuthLevelTypes.Count) return false;
			foreach (string authLevel in PreferredAuthLevelTypes) {
				if (!other.PreferredAuthLevelTypes.Contains(authLevel)) return false;
			}
			return true;
		}

		/// <summary>
		/// Gets a hash code for this object.
		/// </summary>
		public override int GetHashCode() {
			return PreferredPolicies.GetHashCode();
		}

		#region IExtensionRequest Members

		IDictionary<string, string> IExtensionRequest.Serialize(DotNetOpenId.RelyingParty.IAuthenticationRequest authenticationRequest) {
			var fields = new Dictionary<string, string>();

			if (MaximumAuthenticationAge.HasValue) {
				fields.Add(Constants.RequestParameters.MaxAuthAge,
					MaximumAuthenticationAge.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			}

			// Even if empty, this parameter is required as part of the request message.
			fields.Add(Constants.RequestParameters.PreferredAuthPolicies, SerializePolicies(PreferredPolicies));

			if (PreferredAuthLevelTypes.Count > 0) {
				AliasManager authLevelAliases = new AliasManager();
				authLevelAliases.AssignAliases(PreferredAuthLevelTypes, Constants.AuthenticationLevels.PreferredTypeUriToAliasMap);

				// Add a definition for each Auth Level Type alias.
				foreach (string alias in authLevelAliases.Aliases) {
					fields.Add(Constants.AuthLevelNamespaceDeclarationPrefix + alias, authLevelAliases.ResolveAlias(alias));
				}

				// Now use the aliases for those type URIs to list a preferred order.
				fields.Add(Constants.RequestParameters.PreferredAuthLevelTypes, SerializeAuthLevels(PreferredAuthLevelTypes, authLevelAliases));
			}

			return fields;
		}

		bool IExtensionRequest.Deserialize(IDictionary<string, string> fields, DotNetOpenId.Provider.IRequest request, string typeUri) {
			if (fields == null) return false;
			if (!fields.ContainsKey(Constants.RequestParameters.PreferredAuthPolicies)) return false;

			string maxAuthAge;
			MaximumAuthenticationAge = fields.TryGetValue(Constants.RequestParameters.MaxAuthAge, out maxAuthAge) ?
				TimeSpan.FromSeconds(double.Parse(maxAuthAge, CultureInfo.InvariantCulture)) : (TimeSpan?)null;

			PreferredPolicies.Clear();
			string[] preferredPolicies = fields[Constants.RequestParameters.PreferredAuthPolicies].Split(' ');
			foreach (string policy in preferredPolicies) {
				if (policy.Length > 0)
					PreferredPolicies.Add(policy);
			}

			PreferredAuthLevelTypes.Clear();
			AliasManager authLevelAliases = FindIncomingAliases(fields);
			string preferredAuthLevelAliases;
			if (fields.TryGetValue(Constants.RequestParameters.PreferredAuthLevelTypes, out preferredAuthLevelAliases)) {
				foreach (string authLevelAlias in preferredAuthLevelAliases.Split(' ')) {
					if (authLevelAlias.Length == 0) continue;
					PreferredAuthLevelTypes.Add(authLevelAliases.ResolveAlias(authLevelAlias));
				}
			}

			return true;
		}

		#endregion

		#region IExtension Members

		string IExtension.TypeUri {
			get { return Constants.TypeUri; }
		}

		IEnumerable<string> IExtension.AdditionalSupportedTypeUris {
			get { return new string[0]; }
		}

		#endregion

		static internal string SerializePolicies(IList<string> policies) {
			return ConcatenateListOfElements(policies);
		}

		private static string SerializeAuthLevels(IList<string> preferredAuthLevelTypes, AliasManager aliases) {
			var aliasList = new List<string>();
			foreach (string typeUri in preferredAuthLevelTypes) {
				aliasList.Add(aliases.GetAlias(typeUri));
			}
			
			return ConcatenateListOfElements(aliasList);
		}

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

		static string ConcatenateListOfElements(IList<string> values) {
			Debug.Assert(values != null);
			StringBuilder valuesList = new StringBuilder();
			foreach (string value in GetUniqueItems(values)) {
				if (value.Contains(" ")) {
					throw new FormatException(string.Format(CultureInfo.CurrentCulture,
						Strings.InvalidUri, value));
				}
				valuesList.Append(value);
				valuesList.Append(" ");
			}
			if (valuesList.Length > 0)
				valuesList.Length -= 1; // remove trailing space
			return valuesList.ToString();
		}

		static internal IEnumerable<T> GetUniqueItems<T>(IList<T> list) {
			List<T> itemsSeen = new List<T>(list.Count);
			foreach (T item in list) {
				if (itemsSeen.Contains(item)) continue;
				itemsSeen.Add(item);
				yield return item;
			}
		}
	}
}
