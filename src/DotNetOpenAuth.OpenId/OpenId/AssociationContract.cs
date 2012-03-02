//-----------------------------------------------------------------------
// <copyright file="AssociationContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Code contract for the <see cref="Association"/> class.
	/// </summary>
	[ContractClassFor(typeof(Association))]
	internal abstract class AssociationContract : Association {
		/// <summary>
		/// Prevents a default instance of the <see cref="AssociationContract"/> class from being created.
		/// </summary>
		private AssociationContract()
			: base(null, null, TimeSpan.Zero, DateTime.Now) {
		}

		/// <summary>
		/// Gets the length (in bits) of the hash this association creates when signing.
		/// </summary>
		public override int HashBitLength {
			get {
				Contract.Ensures(Contract.Result<int>() > 0);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// The string to pass as the assoc_type value in the OpenID protocol.
		/// </summary>
		/// <param name="protocol">The protocol version of the message that the assoc_type value will be included in.</param>
		/// <returns>
		/// The value that should be used for  the openid.assoc_type parameter.
		/// </returns>
		[Pure]
		internal override string GetAssociationType(Protocol protocol) {
			Requires.NotNull(protocol, "protocol");
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the specific hash algorithm used for message signing.
		/// </summary>
		/// <returns>
		/// The hash algorithm used for message signing.
		/// </returns>
		[Pure]
		protected override HashAlgorithm CreateHasher() {
			Contract.Ensures(Contract.Result<HashAlgorithm>() != null);
			throw new NotImplementedException();
		}
	}
}
