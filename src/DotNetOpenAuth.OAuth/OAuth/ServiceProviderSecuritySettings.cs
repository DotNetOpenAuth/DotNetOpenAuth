//-----------------------------------------------------------------------
// <copyright file="ServiceProviderSecuritySettings.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;

	/// <summary>
	/// Security settings that are applicable to service providers.
	/// </summary>
	public class ServiceProviderSecuritySettings : SecuritySettings {
		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderSecuritySettings"/> class.
		/// </summary>
		internal ServiceProviderSecuritySettings() {
		}

		/// <summary>
		/// Gets or sets the minimum required version of OAuth that must be implemented by a Consumer.
		/// </summary>
		public ProtocolVersion MinimumRequiredOAuthVersion { get; set; }

		/// <summary>
		/// Gets or sets the maximum time a user can take to complete authorization.
		/// </summary>
		/// <remarks>
		/// This time limit serves as a security mitigation against brute force attacks to
		/// compromise (unauthorized or authorized) request tokens.
		/// Longer time limits is more friendly to slow users or consumers, while shorter
		/// time limits provide better security.
		/// </remarks>
		public TimeSpan MaximumRequestTokenTimeToLive { get; set; }
	}
}
