//-----------------------------------------------------------------------
// <copyright file="IHttpDirectRequestContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Text;

	/// <summary>
	/// Contract class for the <see cref="IHttpDirectRequest"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IHttpDirectRequest))]
	public abstract class IHttpDirectRequestContract : IHttpDirectRequest {
		#region IHttpDirectRequest Members

		/// <summary>
		/// Gets the HTTP headers of the request.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectRequest.Headers {
			get {
				Contract.Ensures(Contract.Result<WebHeaderCollection>() != null);
				throw new NotImplementedException();
			}
		}

		#endregion

		#region IMessage Members

		Version IMessage.Version {
			get { throw new NotImplementedException(); }
		}

		IDictionary<string, string> IMessage.ExtraData {
			get { throw new NotImplementedException(); }
		}

		void IMessage.EnsureValidMessage() {
			throw new NotImplementedException();
		}

		#endregion
	}
}
