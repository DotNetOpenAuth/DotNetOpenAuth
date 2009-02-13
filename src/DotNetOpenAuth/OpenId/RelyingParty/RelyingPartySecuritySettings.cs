//-----------------------------------------------------------------------
// <copyright file="RelyingPartySecuritySettings.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Security settings that are applicable to relying parties.
	/// </summary>
	public sealed class RelyingPartySecuritySettings : SecuritySettings {
		/// <summary>
		/// Backing field for the <see cref="RequireSsl"/> property.
		/// </summary>
		private bool requireSsl;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartySecuritySettings"/> class.
		/// </summary>
		internal RelyingPartySecuritySettings()
			: base(false) {
			this.PrivateSecretMaximumAge = TimeSpan.FromDays(7);
		}

		/// <summary>
		/// Fired when the <see cref="RequireSsl"/> property is changed.
		/// </summary>
		internal event EventHandler RequireSslChanged;

		/// <summary>
		/// Gets or sets a value indicating whether the entire pipeline from Identifier discovery to 
		/// Provider redirect is guaranteed to be encrypted using HTTPS for authentication to succeed.
		/// </summary>
		/// <remarks>
		/// <para>Setting this property to true is appropriate for RPs with highly sensitive 
		/// personal information behind the authentication (money management, health records, etc.)</para>
		/// <para>When set to true, some behavioral changes and additional restrictions are placed:</para>
		/// <list>
		/// <item>User-supplied identifiers lacking a scheme are prepended with
		/// HTTPS:// rather than the standard HTTP:// automatically.</item>
		/// <item>User-supplied identifiers are not allowed to use HTTP for the scheme.</item>
		/// <item>All redirects during discovery on the user-supplied identifier must be HTTPS.</item>
		/// <item>Any XRDS file found by discovery on the User-supplied identifier must be protected using HTTPS.</item>
		/// <item>Only Provider endpoints found at HTTPS URLs will be considered.</item>
		/// <item>If the discovered identifier is an OP Identifier (directed identity), the 
		/// Claimed Identifier eventually asserted by the Provider must be an HTTPS identifier.</item>
		/// <item>In the case of an unsolicited assertion, the asserted Identifier, discovery on it and 
		/// the asserting provider endpoint must all be secured by HTTPS.</item>
		/// </list>
		/// <para>Although the first redirect from this relying party to the Provider is required
		/// to use HTTPS, any additional redirects within the Provider cannot be protected and MAY
		/// revert the user's connection to HTTP, based on individual Provider implementation.
		/// There is nothing that the RP can do to detect or prevent this.</para>
		/// <para>
		/// A <see cref="ProtocolException"/> is thrown during discovery or authentication when a secure pipeline cannot be established.
		/// </para>
		/// </remarks>
		public bool RequireSsl {
			get {
				return this.requireSsl;
			}

			set {
				if (this.requireSsl == value) {
					return;
				}
				this.requireSsl = value;
				this.OnRequireSslChanged();
			}
		}

		/// <summary>
		/// Gets or sets the oldest version of OpenID the remote party is allowed to implement.
		/// </summary>
		/// <value>Defaults to <see cref="ProtocolVersion.V10"/></value>
		public ProtocolVersion MinimumRequiredOpenIdVersion { get; set; }

		/// <summary>
		/// Gets or sets the maximum allowable age of the secret a Relying Party
		/// uses to its return_to URLs and nonces with 1.0 Providers.
		/// </summary>
		/// <value>The default value is 7 days.</value>
		public TimeSpan PrivateSecretMaximumAge { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether unsigned extension responses will be deserialized.
		/// </summary>
		/// <value>
		/// 	<c>false</c> to ignore unsigned extension responses; <c>true</c> to accept them.
		/// 	Default is <c>false</c>.
		/// </value>
		/// <remarks>
		/// This is an internal-only property because not requiring signed extensions is
		/// potentially dangerous.  It is included here as an internal option primarily
		/// to enable testing.
		/// </remarks>
		internal bool AllowUnsignedIncomingExtensions { get; set; }

		/// <summary>
		/// Fires the <see cref="RequireSslChanged"/> event.
		/// </summary>
		private void OnRequireSslChanged() {
			EventHandler requireSslChanged = this.RequireSslChanged;
			if (requireSslChanged != null) {
				requireSslChanged(this, new EventArgs());
			}
		}
	}
}
