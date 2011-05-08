//-----------------------------------------------------------------------
// <copyright file="AssociationDataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class AssociationDataBag : DataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociationDataBag"/> class.
		/// </summary>
		public AssociationDataBag() {
		}

		[MessagePart(IsRequired = true)]
		internal byte[] Secret { get; set; }

		[MessagePart(IsRequired = true)]
		internal DateTime ExpiresUtc { get; set; }

		[MessagePart(IsRequired = true)]
		internal bool IsPrivateAssociation {
			get { return this.AssociationType == AssociationRelyingPartyType.Dumb; }
			set { this.AssociationType = value ? AssociationRelyingPartyType.Dumb : AssociationRelyingPartyType.Smart; }
		}

		internal AssociationRelyingPartyType AssociationType { get; set; }

		internal static IDataBagFormatter<AssociationDataBag> CreateFormatter(byte[] symmetricSecret) {
			return new UriStyleMessageFormatter<AssociationDataBag>(symmetricSecret, signed: true, encrypted: true);
		}
	}
}
