//-----------------------------------------------------------------------
// <copyright file="Consumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using DotNetOAuth.Messages;

	/// <summary>
	/// A website or application that uses OAuth to access the Service Provider on behalf of the User.
	/// </summary>
	internal class Consumer {
		/// <summary>
		/// Initializes a new instance of the <see cref="Consumer"/> class.
		/// </summary>
		internal Consumer() {
		}

		/// <summary>
		/// Gets or sets the Consumer Key used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the Consumer Secret used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the Service Provider that will be accessed.
		/// </summary>
		public ServiceProvider ServiceProvider { get; set; }

		/// <summary>
		/// Begins an OAuth authorization request and redirects the user to the Service Provider
		/// to provide that authorization.
		/// </summary>
		public void RequestUserAuthorization() {
			RequestTokenMessage requestToken = new RequestTokenMessage(ServiceProvider.RequestTokenEndpoint);
		}
	}
}
