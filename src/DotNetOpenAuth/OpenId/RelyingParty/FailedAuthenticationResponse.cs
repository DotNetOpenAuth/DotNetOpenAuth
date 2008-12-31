//-----------------------------------------------------------------------
// <copyright file="FailedAuthenticationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;

	[DebuggerDisplay("{Exception.Message}")]
	internal class FailedAuthenticationResponse : IAuthenticationResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="FailedAuthenticationResponse"/> class.
		/// </summary>
		/// <param name="exception">The exception that resulted in the failed authentication.</param>
		internal FailedAuthenticationResponse(Exception exception) {
			this.Exception = exception;
		}

		#region IAuthenticationResponse Members

		public Identifier ClaimedIdentifier {
			get { return null; }
		}

		public string FriendlyIdentifierForDisplay {
			get { return null; }
		}

		public AuthenticationStatus Status {
			get { return AuthenticationStatus.Failed; }
		}

		public Exception Exception { get; private set; }

		public IDictionary<string, string> GetCallbackArguments() {
			return new Dictionary<string, string>();
		}

		public string GetCallbackArgument(string key) {
			return null;
		}

		public T GetExtension<T>() where T : IOpenIdMessageExtension, new() {
			return default(T);
		}

		public IOpenIdMessageExtension GetExtension(Type extensionType) {
			return null;
		}

		#endregion
	}
}
