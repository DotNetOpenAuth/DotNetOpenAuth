//-----------------------------------------------------------------------
// <copyright file="ServiceProviderSecuritySettings.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	}
}
