//-----------------------------------------------------------------------
// <copyright file="PolicyResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// The PAPE response part of an OpenID Authentication response message.
	/// </summary>
	[Serializable]
	public sealed class PolicyResponse : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUri && !isProviderRole) {
				return new PolicyResponse();
			}

			return null;
		};

		/// <summary>
		/// The first part of a parameter name that gives the custom string value for
		/// the assurance level.  The second part of the parameter name is the alias for
		/// that assurance level.
		/// </summary>
		private const string AuthLevelAliasPrefix = "auth_level.";

		/// <summary>
		/// One or more authentication policy URIs that the OP conformed to when authenticating the End User.
		/// </summary>
		/// <value>Space separated list of authentication policy URIs.</value>
		/// <remarks>
		/// If no policies were met though the OP wishes to convey other information in the response, this parameter MUST be included with the value of "none".
		/// </remarks>
		[MessagePart("auth_policies", IsRequired = true)]
		private string actualPoliciesString;

		/// <summary>
		/// Backing field for the <see cref="AuthenticationTimeUtc"/> property.
		/// </summary>
		private DateTime? authenticationTimeUtc;

		/// <summary>
		/// Initializes a new instance of the <see cref="PolicyResponse"/> class.
		/// </summary>
		public PolicyResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
			this.ActualPolicies = new List<string>(1);
			this.AssuranceLevels = new Dictionary<string, string>(1);
		}

		/// <summary>
		/// Gets a list of authentication policy URIs that the 
		/// OP conformed to when authenticating the End User.
		/// </summary>
		public IList<string> ActualPolicies { get; private set; }

		/// <summary>
		/// Gets or sets the most recent timestamp when the End User has 
		/// actively authenticated to the OP in a manner fitting the asserted policies.
		/// </summary>
		/// <remarks>
		/// If the RP's request included the "openid.pape.max_auth_age" parameter 
		/// then the OP MUST include "openid.pape.auth_time" in its response. 
		/// If "openid.pape.max_auth_age" was not requested, the OP MAY choose to include 
		/// "openid.pape.auth_time" in its response.
		/// </remarks>
		[MessagePart("auth_time", Encoder = typeof(DateTimeEncoder))]
		public DateTime? AuthenticationTimeUtc {
			get {
				return this.authenticationTimeUtc;
			}

			set {
				Requires.That(!value.HasValue || value.Value.Kind != DateTimeKind.Unspecified, "value", OpenIdStrings.UnspecifiedDateTimeKindNotAllowed);

				// Make sure that whatever is set here, it becomes UTC time.
				if (value.HasValue) {
					// Convert to UTC and cut to the second, since the protocol only allows for
					// that level of precision.
					this.authenticationTimeUtc = OpenIdUtilities.CutToSecond(value.Value.ToUniversalTimeSafe());
				} else {
					this.authenticationTimeUtc = null;
				}
			}
		}

		/// <summary>
		/// Gets or sets the Assurance Level as defined by the National 
		/// Institute of Standards and Technology (NIST) in Special Publication 
		/// 800-63 (Burr, W., Dodson, D., and W. Polk, Ed., “Electronic 
		/// Authentication Guideline,” April 2006.) [NIST_SP800‑63] corresponding 
		/// to the authentication method and policies employed by the OP when 
		/// authenticating the End User.
		/// </summary>
		/// <remarks>
		/// See PAPE spec Appendix A.1.2 (NIST Assurance Levels) for high-level 
		/// example classifications of authentication methods within the defined 
		/// levels.
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nist", Justification = "Acronym")]
		public NistAssuranceLevel? NistAssuranceLevel {
			get {
				string levelString;
				if (this.AssuranceLevels.TryGetValue(Constants.AssuranceLevels.NistTypeUri, out levelString)) {
					return (NistAssuranceLevel)Enum.Parse(typeof(NistAssuranceLevel), levelString);
				} else {
					return null;
				}
			}

			set {
				if (value != null) {
					this.AssuranceLevels[Constants.AssuranceLevels.NistTypeUri] = ((int)value).ToString(CultureInfo.InvariantCulture);
				} else {
					this.AssuranceLevels.Remove(Constants.AssuranceLevels.NistTypeUri);
				}
			}
		}

		/// <summary>
		/// Gets a dictionary where keys are the authentication level type URIs and
		/// the values are the per authentication level defined custom value.
		/// </summary>
		/// <remarks>
		/// A very common key is <see cref="Constants.AssuranceLevels.NistTypeUri"/>
		/// and values for this key are available in <see cref="NistAssuranceLevel"/>.
		/// </remarks>
		public IDictionary<string, string> AssuranceLevels { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this extension is signed by the Provider.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the Provider; otherwise, <c>false</c>.
		/// </value>
		public bool IsSignedByProvider {
			get { return this.IsSignedByRemoteParty; }
		}

		#region IMessageWithEvents Members

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnSending() {
			var extraData = ((IMessage)this).ExtraData;
			extraData.Clear();

			this.actualPoliciesString = SerializePolicies(this.ActualPolicies);

			if (this.AssuranceLevels.Count > 0) {
				AliasManager aliases = new AliasManager();
				aliases.AssignAliases(this.AssuranceLevels.Keys, Constants.AssuranceLevels.PreferredTypeUriToAliasMap);

				// Add a definition for each Auth Level Type alias.
				foreach (string alias in aliases.Aliases) {
					extraData.Add(Constants.AuthLevelNamespaceDeclarationPrefix + alias, aliases.ResolveAlias(alias));
				}

				// Now use the aliases for those type URIs to list the individual values.
				foreach (var pair in this.AssuranceLevels) {
					extraData.Add(AuthLevelAliasPrefix + aliases.GetAlias(pair.Key), pair.Value);
				}
			}
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			var extraData = ((IMessage)this).ExtraData;

			this.ActualPolicies.Clear();
			string[] actualPolicies = this.actualPoliciesString.Split(' ');
			foreach (string policy in actualPolicies) {
				if (policy.Length > 0 && policy != AuthenticationPolicies.None) {
					this.ActualPolicies.Add(policy);
				}
			}

			this.AssuranceLevels.Clear();
			AliasManager authLevelAliases = PapeUtilities.FindIncomingAliases(extraData);
			foreach (string authLevelAlias in authLevelAliases.Aliases) {
				string authValue;
				if (extraData.TryGetValue(AuthLevelAliasPrefix + authLevelAlias, out authValue)) {
					string authLevelType = authLevelAliases.ResolveAlias(authLevelAlias);
					this.AssuranceLevels[authLevelType] = authValue;
				}
			}
		}

		#endregion

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
			PolicyResponse other = obj as PolicyResponse;
			if (other == null) {
				return false;
			}

			if (this.AuthenticationTimeUtc != other.AuthenticationTimeUtc) {
				return false;
			}

			if (this.AssuranceLevels.Count != other.AssuranceLevels.Count) {
				return false;
			}

			foreach (var pair in this.AssuranceLevels) {
				if (!other.AssuranceLevels.Contains(pair)) {
					return false;
				}
			}

			if (this.ActualPolicies.Count != other.ActualPolicies.Count) {
				return false;
			}

			foreach (string policy in this.ActualPolicies) {
				if (!other.ActualPolicies.Contains(policy)) {
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
			// This is a poor hash function, but an site that cares will likely have a bunch
			// of look-alike instances anyway, so a good hash function would still bunch
			// all the instances into the same hash code.
			if (this.AuthenticationTimeUtc.HasValue) {
				return this.AuthenticationTimeUtc.Value.GetHashCode();
			} else {
				return 1;
			}
		}

		/// <summary>
		/// Serializes the applied policies for transmission from the Provider
		/// to the Relying Party.
		/// </summary>
		/// <param name="policies">The applied policies.</param>
		/// <returns>A space-delimited list of applied policies.</returns>
		private static string SerializePolicies(IList<string> policies) {
			if (policies.Count == 0) {
				return AuthenticationPolicies.None;
			} else {
				return PapeUtilities.ConcatenateListOfElements(policies);
			}
		}
	}
}
