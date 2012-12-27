//-----------------------------------------------------------------------
// <copyright file="SecuritySettings.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Security settings that may be applicable to both relying parties and providers.
	/// </summary>
	[Serializable]
	public abstract class SecuritySettings {
		/// <summary>
		/// Gets the default minimum hash bit length.
		/// </summary>
		internal const int MinimumHashBitLengthDefault = 160;

		/// <summary>
		/// Gets the maximum hash bit length default for relying parties.
		/// </summary>
		internal const int MaximumHashBitLengthRPDefault = 256;

		/// <summary>
		/// Gets the maximum hash bit length default for providers.
		/// </summary>
		internal const int MaximumHashBitLengthOPDefault = 512;

		/// <summary>
		/// Initializes a new instance of the <see cref="SecuritySettings"/> class.
		/// </summary>
		/// <param name="isProvider">A value indicating whether this class is being instantiated for a Provider.</param>
		protected SecuritySettings(bool isProvider) {
			this.MaximumHashBitLength = isProvider ? MaximumHashBitLengthOPDefault : MaximumHashBitLengthRPDefault;
			this.MinimumHashBitLength = MinimumHashBitLengthDefault;
		}

		/// <summary>
		/// Gets or sets the minimum hash length (in bits) allowed to be used in an <see cref="Association"/>
		/// with the remote party.  The default is 160.
		/// </summary>
		/// <remarks>
		/// SHA-1 (160 bits) has been broken.  The minimum secure hash length is now 256 bits.
		/// The default is still a 160 bit minimum to allow interop with common remote parties,
		/// such as Yahoo! that only supports 160 bits.  
		/// For sites that require high security such as to store bank account information and 
		/// health records, 256 is the recommended value.
		/// </remarks>
		public int MinimumHashBitLength { get; set; }

		/// <summary>
		/// Gets or sets the maximum hash length (in bits) allowed to be used in an <see cref="Association"/>
		/// with the remote party.  The default is 256 for relying parties and 512 for providers.
		/// </summary>
		/// <remarks>
		/// The longer the bit length, the more secure the identities of your visitors are.
		/// Setting a value higher than 256 on a relying party site may reduce performance
		/// as many association requests will be denied, causing secondary requests or even
		/// authentication failures.
		/// Setting a value higher than 256 on a provider increases security where possible
		/// without these side-effects.
		/// </remarks>
		public int MaximumHashBitLength { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether identifiers that are both OP Identifiers and Claimed Identifiers
		/// should ever be recognized as claimed identifiers.
		/// </summary>
		/// <value>
		/// 	The default value is <c>false</c>, per the OpenID 2.0 spec.
		/// </value>
		/// <remarks>
		/// OpenID 2.0 sections 7.3.2.2 and 11.2 specify that OP Identifiers never be recognized as Claimed Identifiers.
		/// However, for some scenarios it may be desirable for an RP to override this behavior and allow this.
		/// The security ramifications of setting this property to <c>true</c> have not been fully explored and
		/// therefore this setting should only be changed with caution.
		/// </remarks>
		public bool AllowDualPurposeIdentifiers { get; set; }

		/// <summary>
		/// Determines whether a named association fits the security requirements.
		/// </summary>
		/// <param name="protocol">The protocol carrying the association.</param>
		/// <param name="associationType">The value of the openid.assoc_type parameter.</param>
		/// <returns>
		/// 	<c>true</c> if the association is permitted given the security requirements; otherwise, <c>false</c>.
		/// </returns>
		internal bool IsAssociationInPermittedRange(Protocol protocol, string associationType) {
			int lengthInBits = HmacShaAssociation.GetSecretLength(protocol, associationType) * 8;
			return lengthInBits >= this.MinimumHashBitLength && lengthInBits <= this.MaximumHashBitLength;
		}

		/// <summary>
		/// Determines whether a given association fits the security requirements.
		/// </summary>
		/// <param name="association">The association to check.</param>
		/// <returns>
		/// 	<c>true</c> if the association is permitted given the security requirements; otherwise, <c>false</c>.
		/// </returns>
		internal bool IsAssociationInPermittedRange(Association association) {
			Requires.NotNull(association, "association");
			return association.HashBitLength >= this.MinimumHashBitLength && association.HashBitLength <= this.MaximumHashBitLength;
		}
	}
}
