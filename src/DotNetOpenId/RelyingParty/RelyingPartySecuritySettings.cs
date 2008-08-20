using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Security settings that are applicable to relying parties.
	/// </summary>
	public sealed class RelyingPartySecuritySettings : SecuritySettings {
		internal RelyingPartySecuritySettings() : base(false) { }

		private bool requireSsl;
		/// <summary>
		/// Gets/sets whether the entire pipeline from Identifier discovery to Provider redirect
		/// is guaranteed to be encrypted using HTTPS for authentication to succeed.
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
		/// An <see cref="OpenIdException"/> is thrown when a secure pipeline cannot be established.
		/// </para>
		/// </remarks>
		public bool RequireSsl {
			get { return requireSsl; }
			set {
				if (requireSsl == value) return;
				requireSsl = value;
				OnRequireSslChanged();
			}
		}

		internal event EventHandler RequireSslChanged;
		/// <summary>
		/// Fires the <see cref="RequireSslChanged"/> event.
		/// </summary>
		void OnRequireSslChanged() {
			EventHandler requireSslChanged = RequireSslChanged;
			if (requireSslChanged != null) {
				requireSslChanged(this, new EventArgs());
			}
		}

		/// <summary>
		/// Gets/sets the oldest version of OpenID the remote party is allowed to implement.
		/// </summary>
		/// <value>Defaults to <see cref="ProtocolVersion.V10"/></value>
		public ProtocolVersion MinimumRequiredOpenIdVersion { get; set; }
	}
}
