//-----------------------------------------------------------------------
// <copyright file="RequestContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Code contract for the <see cref="Request"/> class.
	/// </summary>
	[ContractClassFor(typeof(Request))]
	internal abstract class RequestContract : Request {
		/// <summary>
		/// Prevents a default instance of the <see cref="RequestContract"/> class from being created.
		/// </summary>
		private RequestContract() : base((Version)null, null) {
		}

		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		public override bool IsResponseReady {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the response message, once <see cref="IsResponseReady"/> is <c>true</c>.
		/// </summary>
		protected override IProtocolMessage ResponseMessage {
			get {
				Requires.ValidState(this.IsResponseReady);
				Contract.Ensures(Contract.Result<IProtocolMessage>() != null);
				throw new NotImplementedException();
			}
		}
	}
}
