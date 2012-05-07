//-----------------------------------------------------------------------
// <copyright file="WindowsLiveClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;

	/// <summary>
	/// The WindowsLive client.
	/// </summary>
	/// <remarks>
	/// The WindowsLive brand is being replaced by Microsoft account brand.
	/// We keep this class for backward compatibility only.
	/// </remarks>
	[Obsolete("Use the MicrosoftClient class.")]
	public sealed class WindowsLiveClient : MicrosoftClient {
		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsLiveClient"/> class.
		/// </summary>
		/// <param name="appId">The app id.</param>
		/// <param name="appSecret">The app secret.</param>
		public WindowsLiveClient(string appId, string appSecret) :
			base("windowslive", appId, appSecret) {
		}
	}
}