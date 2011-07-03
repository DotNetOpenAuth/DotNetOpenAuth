namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	internal static class OAuthReporting {
		/// <summary>
		/// Records the feature and dependency use.
		/// </summary>
		/// <param name="value">The consumer or service provider.</param>
		/// <param name="service">The service.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="nonceStore">The nonce store.</param>
		internal static void RecordFeatureAndDependencyUse(object value, ServiceProviderDescription service, ITokenManager tokenManager, INonceStore nonceStore) {
			Contract.Requires(value != null);
			Contract.Requires(service != null);
			Contract.Requires(tokenManager != null);

			// In release builds, just quietly return.
			if (value == null || service == null || tokenManager == null) {
				return;
			}

			if (Enabled && Configuration.IncludeFeatureUsage) {
				StringBuilder builder = new StringBuilder();
				builder.Append(value.GetType().Name);
				builder.Append(" ");
				builder.Append(tokenManager.GetType().Name);
				if (nonceStore != null) {
					builder.Append(" ");
					builder.Append(nonceStore.GetType().Name);
				}
				builder.Append(" ");
				builder.Append(service.Version);
				builder.Append(" ");
				builder.Append(service.UserAuthorizationEndpoint);
				observedFeatures.Add(builder.ToString());
				Touch();
			}
		}
	}
}
