//-----------------------------------------------------------------------
// <copyright file="Configuration.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A set of adjustable properties that control various aspects of OpenID behavior.
	/// </summary>
	internal static class Configuration {
		/// <summary>
		/// Initializes static members of the <see cref="Configuration"/> class.
		/// </summary>
		static Configuration() {
			MaximumUserAgentAuthenticationTime = TimeSpan.FromMinutes(5);
		}

		/// <summary>
		/// Gets the maximum time a user can be allowed to take to complete authentication.
		/// </summary>
		/// <remarks>
		/// This is used to calculate the length of time that nonces are stored.
		/// This is internal until we can decide whether to leave this static, or make
		/// it an instance member, or put it inside the IConsumerApplicationStore interface.
		/// </remarks>
		internal static TimeSpan MaximumUserAgentAuthenticationTime { get; private set; }
	}
}
