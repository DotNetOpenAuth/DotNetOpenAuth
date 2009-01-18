//-----------------------------------------------------------------------
// <copyright file="PolicyRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The PAPE request part of an OpenID Authentication request message.
	/// </summary>
	public sealed class PolicyRequest : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly OpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage) => {
			if (typeUri == Constants.TypeUri && baseMessage is SignedResponseRequest) {
				return new PolicyRequest();
			}

			return null;
		};
		
		[MessagePart("preferred_auth_policies", IsRequired = true)]
		private string preferredPoliciesString;

		/// <summary>
		/// Initializes a new instance of the <see cref="PolicyRequest"/> class.
		/// </summary>
		public PolicyRequest()
			: base(new Version(1, 0), Constants.TypeUri, null) {
			PreferredPolicies = new List<string>(1);
			PreferredAuthLevelTypes = new List<string>(1);
		}

		/// <summary>
		/// Optional. If the End User has not actively authenticated to the OP within 
		/// the number of seconds specified in a manner fitting the requested policies, 
		/// the OP SHOULD authenticate the End User for this request.
		/// </summary>
		/// <remarks>
		/// The OP should realize that not adhering to the request for re-authentication
		/// most likely means that the End User will not be allowed access to the 
		/// services provided by the RP. If this parameter is absent in the request, 
		/// the OP should authenticate the user at its own discretion.
		/// </remarks>
		[MessagePart("max_auth_age", IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
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
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			PolicyRequest other = obj as PolicyRequest;
			if (other == null) {
				return false;
			}

			if (MaximumAuthenticationAge != other.MaximumAuthenticationAge) {
				return false;
			}

			if (PreferredPolicies.Count != other.PreferredPolicies.Count) {
				return false;
			}

			foreach (string policy in PreferredPolicies) {
				if (!other.PreferredPolicies.Contains(policy)) {
					return false;
				}
			}

			if (PreferredAuthLevelTypes.Count != other.PreferredAuthLevelTypes.Count) {
				return false;
			}

			foreach (string authLevel in PreferredAuthLevelTypes) {
				if (!other.PreferredAuthLevelTypes.Contains(authLevel)) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			// TODO: fix this to match Equals
			return PreferredPolicies.GetHashCode();
		}

		private static string SerializePolicies(IList<string> policies) {
			return PapeUtilities.ConcatenateListOfElements(policies);
		}

		private static string SerializeAuthLevels(IList<string> preferredAuthLevelTypes, AliasManager aliases) {
			var aliasList = new List<string>();
			foreach (string typeUri in preferredAuthLevelTypes) {
				aliasList.Add(aliases.GetAlias(typeUri));
			}

			return PapeUtilities.ConcatenateListOfElements(aliasList);
		}

		#region IMessageWithEvents Members

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnSending() {
			var extraData = ((IMessage)this).ExtraData;
			extraData.Clear();

			this.preferredPoliciesString = SerializePolicies(this.PreferredPolicies);

			if (PreferredAuthLevelTypes.Count > 0) {
				AliasManager authLevelAliases = new AliasManager();
				authLevelAliases.AssignAliases(PreferredAuthLevelTypes, Constants.AuthenticationLevels.PreferredTypeUriToAliasMap);

				// Add a definition for each Auth Level Type alias.
				foreach (string alias in authLevelAliases.Aliases) {
					extraData.Add(Constants.AuthLevelNamespaceDeclarationPrefix + alias, authLevelAliases.ResolveAlias(alias));
				}

				// Now use the aliases for those type URIs to list a preferred order.
				extraData.Add(Constants.RequestParameters.PreferredAuthLevelTypes, SerializeAuthLevels(PreferredAuthLevelTypes, authLevelAliases));
			}
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			var extraData = ((IMessage)this).ExtraData;

			PreferredPolicies.Clear();
			string[] preferredPolicies = extraData[Constants.RequestParameters.PreferredAuthPolicies].Split(' ');
			foreach (string policy in preferredPolicies) {
				if (policy.Length > 0)
					PreferredPolicies.Add(policy);
			}

			PreferredAuthLevelTypes.Clear();
			AliasManager authLevelAliases = PapeUtilities.FindIncomingAliases(extraData);
			string preferredAuthLevelAliases;
			if (extraData.TryGetValue(Constants.RequestParameters.PreferredAuthLevelTypes, out preferredAuthLevelAliases)) {
				foreach (string authLevelAlias in preferredAuthLevelAliases.Split(' ')) {
					if (authLevelAlias.Length == 0) continue;
					PreferredAuthLevelTypes.Add(authLevelAliases.ResolveAlias(authLevelAlias));
				}
			}
		}

		#endregion
	}
}
