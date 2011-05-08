//-----------------------------------------------------------------------
// <copyright file="AssociationDataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class AssociationDataBag : DataBag, IStreamSerializingMessage {
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
			return new BinaryDataBagFormatter<AssociationDataBag>(symmetricSecret, signed: true, encrypted: true);
		}

		public void Serialize(Stream stream) {
			var writer = new BinaryWriter(stream);
			writer.Write(this.IsPrivateAssociation);
			writer.WriteBuffer(this.Secret);
			writer.Write((int)(this.ExpiresUtc - TimestampEncoder.Epoch).TotalSeconds);
			writer.Flush();
		}

		public void Deserialize(Stream stream) {
			var reader = new BinaryReader(stream);
			this.IsPrivateAssociation = reader.ReadBoolean();
			this.Secret = reader.ReadBuffer();
			this.ExpiresUtc = TimestampEncoder.Epoch + TimeSpan.FromSeconds(reader.ReadInt32());
		}
	}
}
