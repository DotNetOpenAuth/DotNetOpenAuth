//-----------------------------------------------------------------------
// <copyright file="IncomingWebResponseContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;

	/// <summary>
	/// Code contract for the <see cref="IncomingWebResponse"/> class.
	/// </summary>
	[ContractClassFor(typeof(IncomingWebResponse))]
	internal abstract class IncomingWebResponseContract : IncomingWebResponse {
		/// <summary>
		/// Gets the body of the HTTP response.
		/// </summary>
		/// <value></value>
		public override Stream ResponseStream {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Creates a text reader for the response stream.
		/// </summary>
		/// <returns>
		/// The text reader, initialized for the proper encoding.
		/// </returns>
		public override StreamReader GetResponseReader() {
			Contract.Ensures(Contract.Result<StreamReader>() != null);
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets an offline snapshot version of this instance.
		/// </summary>
		/// <param name="maximumBytesToCache">The maximum bytes from the response stream to cache.</param>
		/// <returns>A snapshot version of this instance.</returns>
		/// <remarks>
		/// If this instance is a <see cref="NetworkDirectWebResponse"/> creating a snapshot
		/// will automatically close and dispose of the underlying response stream.
		/// If this instance is a <see cref="CachedDirectWebResponse"/>, the result will
		/// be the self same instance.
		/// </remarks>
		internal override CachedDirectWebResponse GetSnapshot(int maximumBytesToCache) {
			Requires.InRange(maximumBytesToCache >= 0, "maximumBytesToCache");
			Requires.ValidState(this.RequestUri != null);
			Contract.Ensures(Contract.Result<CachedDirectWebResponse>() != null);
			throw new NotImplementedException();
		}
	}
}
