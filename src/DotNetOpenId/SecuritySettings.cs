using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId {
	/// <summary>
	/// Security settings that may be applicable to both relying parties and providers.
	/// </summary>
	public class SecuritySettings {
		internal SecuritySettings(bool isProvider) {
			if (isProvider) {
				maximumHashBitLength = 512;
			}
		}

		int minimumHashBitLength = 120;
		/// <summary>
		/// Gets/sets the minimum hash length (in bits) allowed to be used in an <see cref="Association"/>
		/// with the remote party.  The default is 120.
		/// </summary>
		/// <remarks>
		/// SHA-1 (120 bits) has been broken.  The minimum secure hash length is now 256 bits.
		/// The default is still a 120 bit minimum to allow interop with common remote parties,
		/// such as Yahoo! that only supports 120 bits.  
		/// For sites that require high security such as to store bank account information and 
		/// health records, 256 is the recommended value.
		/// </remarks>
		public int MinimumHashBitLength {
			get { return minimumHashBitLength; }
			set { minimumHashBitLength = value; }
		}
		int maximumHashBitLength = 256;
		/// <summary>
		/// Gets/sets the maximum hash length (in bits) allowed to be used in an <see cref="Association"/>
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
		public int MaximumHashBitLength {
			get { return maximumHashBitLength; }
			set { maximumHashBitLength = value; }
		}

		internal bool IsAssociationInPermittedRange(Protocol protocol, string associationType) {
			int length = HmacShaAssociation.GetSecretLength(protocol, associationType);
			return length >= MinimumHashBitLength && length <= MaximumHashBitLength;
		}

		/// <summary>
		/// Gets/sets the oldest version of OpenID the remote party is allowed to implement.
		/// </summary>
		/// <value>Defaults to <see cref="ProtocolVersion.V10"/></value>
		public ProtocolVersion MinimumRequiredOpenIDVersion { get; set; }
	}
}
