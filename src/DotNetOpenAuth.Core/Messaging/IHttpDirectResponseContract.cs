//-----------------------------------------------------------------------
// <copyright file="IHttpDirectResponseContract.cs" company="Outercurve Foundation">
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
	/// Contract class for the <see cref="IHttpDirectResponse"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IHttpDirectResponse))]
	public abstract class IHttpDirectResponseContract : IHttpDirectResponse {
		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		/// <value></value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get {
				Contract.Ensures(Contract.Result<WebHeaderCollection>() != null);
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
