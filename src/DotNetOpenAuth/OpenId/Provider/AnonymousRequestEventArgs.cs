//-----------------------------------------------------------------------
// <copyright file="AnonymousRequestEventArgs.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The event arguments that include details of the incoming request.
	/// </summary>
	public class AnonymousRequestEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="AnonymousRequestEventArgs"/> class.
		/// </summary>
		/// <param name="request">The incoming OpenID request.</param>
		internal AnonymousRequestEventArgs(IAnonymousRequest request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			this.Request = request;
		}

		/// <summary>
		/// Gets the incoming OpenID request.
		/// </summary>
		public IAnonymousRequest Request { get; private set; }
	}
}
