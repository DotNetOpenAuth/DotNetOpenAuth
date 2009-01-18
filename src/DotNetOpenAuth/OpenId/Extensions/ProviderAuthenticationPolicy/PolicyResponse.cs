//-----------------------------------------------------------------------
// <copyright file="PolicyResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The PAPE response part of an OpenID Authentication response message.
	/// </summary>
	public sealed class PolicyResponse : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly OpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage) => {
			if (typeUri == Constants.TypeUri && baseMessage is IndirectSignedResponse) {
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

		private DateTime? authenticationTimeUtc;

		/// <summary>
		/// Instantiates a <see cref="PolicyResponse"/>.
		/// </summary>
		public PolicyResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
			ActualPolicies = new List<string>(1);
			AssuranceLevels = new Dictionary<string, string>(1);
		}

		/// <summary>
		/// One or more authentication policy URIs that the OP conformed to when authenticating the End User.
		/// </summary>
		/// <remarks>
		/// If no policies were met though the OP wishes to convey other information in the response, this parameter MUST be included with the value of "none".
		/// </remarks>
		public IList<string> ActualPolicies { get; private set; }

		/// <summary>
		/// Optional. The most recent timestamp when the End User has actively authenticated to the OP in a manner fitting the asserted policies.
		/// </summary>
		/// <remarks>
		/// If the RP's request included the "openid.max_auth_age" parameter then the OP MUST include "openid.auth_time" in its response. If "openid.max_auth_age" was not requested, the OP MAY choose to include "openid.auth_time" in its response.
		/// </remarks>
		[MessagePart("auth_time", Encoder = typeof(DateTimeEncoder))]
		public DateTime? AuthenticationTimeUtc {
			get { return authenticationTimeUtc; }
			set {
				// Make sure that whatever is set here, it becomes UTC time.
				if (value.HasValue) {
					if (value.Value.Kind == DateTimeKind.Unspecified) {
						throw new ArgumentException(OpenIdStrings.UnspecifiedDateTimeKindNotAllowed, "value");
					}

					// Convert to UTC and cut to the second, since the protocol only allows for
					// that level of precision.
					authenticationTimeUtc = OpenIdUtilities.CutToSecond(value.Value.ToUniversalTime());
				} else {
					authenticationTimeUtc = null;
				}
			}
		}

		/// <summary>
		/// Optional. The Assurance Level as defined by the National Institute of Standards and Technology (NIST) in Special Publication 800-63 (Burr, W., Dodson, D., and W. Polk, Ed., “Electronic Authentication Guideline,” April 2006.) [NIST_SP800‑63] corresponding to the authentication method and policies employed by the OP when authenticating the End User.
		/// </summary>
		/// <remarks>
		/// See PAPE spec Appendix A.1.2 (NIST Assurance Levels) for high-level example classifications of authentication methods within the defined levels.
		/// </remarks>
		public NistAssuranceLevel? NistAssuranceLevel {
			get {
				string levelString;
				if (AssuranceLevels.TryGetValue(Constants.AuthenticationLevels.NistTypeUri, out levelString)) {
					return (NistAssuranceLevel)Enum.Parse(typeof(NistAssuranceLevel), levelString);
				} else {
					return null;
				}
			}
			set {
				if (value != null) {
					AssuranceLevels[Constants.AuthenticationLevels.NistTypeUri] = ((int)value).ToString(CultureInfo.InvariantCulture);
				} else {
					AssuranceLevels.Remove(Constants.AuthenticationLevels.NistTypeUri);
				}
			}
		}

		/// <summary>
		/// Gets a dictionary where keys are the authentication level type URIs and
		/// the values are the per authentication level defined custom value.
		/// </summary>
		/// <remarks>
		/// A very common key is <see cref="Constants.AuthenticationLevels.NistTypeUri"/>
		/// and values for this key are available in <see cref="NistAssuranceLevel"/>.
		/// </remarks>
		public IDictionary<string, string> AssuranceLevels { get; private set; }

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

			if (AuthenticationTimeUtc != other.AuthenticationTimeUtc) {
				return false;
			}

			if (AssuranceLevels.Count != other.AssuranceLevels.Count) {
				return false;
			}

			foreach (var pair in AssuranceLevels) {
				if (!other.AssuranceLevels.Contains(pair)) {
					return false;
				}
			}

			if (ActualPolicies.Count != other.ActualPolicies.Count) {
				return false;
			}

			foreach (string policy in ActualPolicies) {
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
			// TODO: fix this to match Equals
			return ActualPolicies.GetHashCode();
		}

		private static string SerializePolicies(IList<string> policies) {
			if (policies.Count == 0) {
				return AuthenticationPolicies.None;
			} else {
				return PapeUtilities.ConcatenateListOfElements(policies);
			}
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

			if (AssuranceLevels.Count > 0) {
				AliasManager aliases = new AliasManager();
				aliases.AssignAliases(AssuranceLevels.Keys, Constants.AuthenticationLevels.PreferredTypeUriToAliasMap);

				// Add a definition for each Auth Level Type alias.
				foreach (string alias in aliases.Aliases) {
					extraData.Add(Constants.AuthLevelNamespaceDeclarationPrefix + alias, aliases.ResolveAlias(alias));
				}

				// Now use the aliases for those type URIs to list the individual values.
				foreach (var pair in AssuranceLevels) {
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
			string[] actualPolicies = actualPoliciesString.Split(' ');
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
	}
}
