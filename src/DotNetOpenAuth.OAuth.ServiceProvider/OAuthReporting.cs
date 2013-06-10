//-----------------------------------------------------------------------
// <copyright file="OAuthReporting.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using Validation;

	/// <summary>
	/// Utility methods specific to OAuth feature reporting.
	/// </summary>
	internal class OAuthReporting : Reporting {
		/// <summary>
		/// Records the feature and dependency use.
		/// </summary>
		/// <param name="value">The consumer or service provider.</param>
		/// <param name="service">The service.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="nonceStore">The nonce store.</param>
		internal static void RecordFeatureAndDependencyUse(object value, ServiceProviderHostDescription service, ITokenManager tokenManager, INonceStore nonceStore) {
			Requires.NotNull(value, "value");
			Requires.NotNull(service, "service");
			Requires.NotNull(tokenManager, "tokenManager");

			// In release builds, just quietly return.
			if (value == null || service == null || tokenManager == null) {
				return;
			}

			if (Reporting.Enabled && Reporting.Configuration.IncludeFeatureUsage) {
				StringBuilder builder = new StringBuilder();
				builder.Append(value.GetType().Name);
				builder.Append(" ");
				builder.Append(tokenManager.GetType().Name);
				if (nonceStore != null) {
					builder.Append(" ");
					builder.Append(nonceStore.GetType().Name);
				}
				builder.Append(" ");
				builder.Append(service.UserAuthorizationEndpoint != null ? service.UserAuthorizationEndpoint.Location.AbsoluteUri : string.Empty);
				Reporting.ObservedFeatures.Add(builder.ToString());
				Reporting.Touch();
			}
		}
	}
}
