//-----------------------------------------------------------------------
// <copyright file="ISecurityProfile.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Applies a custom security policy to certain OpenID security settings and behaviors.
	/// </summary>
	[ContractClass(typeof(ISecurityProfileContract))]
	internal interface ISecurityProfile {
		/// <summary>
		/// Applies a well known set of security requirements.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void ApplySecuritySettings(SecuritySettings securitySettings);

		/// <summary>
		/// Checks whether the given security settings comply with security requirements and throws otherwise.
		/// </summary>
		/// <param name="securitySettings">The security settings to check for compliance.</param>
		/// <remarks>
		/// Security settings should <em>not</em> be changed by this method.  Any settings
		/// that do not comply should cause an exception to be thrown.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the given security settings are not compliant with the requirements of this security profile.</exception>
		void EnsureCompliance(SecuritySettings securitySettings);
	}

	/// <summary>
	/// Code contract for the <see cref="ISecurityProfile"/> type.
	/// </summary>
	[ContractClassFor(typeof(ISecurityProfile))]
	internal abstract class ISecurityProfileContract : ISecurityProfile {
		/// <summary>
		/// Prevents a default instance of the <see cref="ISecurityProfileContract"/> class from being created.
		/// </summary>
		private ISecurityProfileContract() {
		}

		#region ISecurityProfile Members

		/// <summary>
		/// Applies a well known set of security requirements.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void ISecurityProfile.ApplySecuritySettings(SecuritySettings securitySettings) {
			Contract.Requires(securitySettings != null);
		}

		/// <summary>
		/// Checks whether the given security settings comply with security requirements and throws otherwise.
		/// </summary>
		/// <param name="securitySettings">The security settings to check for compliance.</param>
		/// <remarks>
		/// Security settings should <em>not</em> be changed by this method.  Any settings
		/// that do not comply should cause an exception to be thrown.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the given security settings are not compliant with the requirements of this security profile.</exception>
		void ISecurityProfile.EnsureCompliance(SecuritySettings securitySettings) {
			Contract.Requires(securitySettings != null);
		}

		#endregion
	}
}
