//-----------------------------------------------------------------------
// <copyright file="AssociationDataBag.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using Validation;

	/// <summary>
	/// A signed and encrypted serialization of an association.
	/// </summary>
	internal class AssociationDataBag : DataBag, IStreamSerializingDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociationDataBag"/> class.
		/// </summary>
		public AssociationDataBag() {
		}

		/// <summary>
		/// Gets or sets the association secret.
		/// </summary>
		[MessagePart(IsRequired = true)]
		internal byte[] Secret { get; set; }

		/// <summary>
		/// Gets or sets the UTC time that this association expires.
		/// </summary>
		[MessagePart(IsRequired = true)]
		internal DateTime ExpiresUtc { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is for "dumb" mode RPs.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is private association; otherwise, <c>false</c>.
		/// </value>
		[MessagePart(IsRequired = true)]
		internal bool IsPrivateAssociation {
			get { return this.AssociationType == AssociationRelyingPartyType.Dumb; }
			set { this.AssociationType = value ? AssociationRelyingPartyType.Dumb : AssociationRelyingPartyType.Smart; }
		}

		/// <summary>
		/// Gets or sets the type of the association (shared or private, a.k.a. smart or dumb).
		/// </summary>
		internal AssociationRelyingPartyType AssociationType { get; set; }

		/// <summary>
		/// Serializes the instance to the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public void Serialize(Stream stream) {
			Requires.NotNull(stream, "stream");
			Requires.That(stream.CanWrite, "stream", "requires stream.CanWrite");

			var writer = new BinaryWriter(stream);
			writer.Write(this.IsPrivateAssociation);
			writer.WriteBuffer(this.Secret);
			writer.Write((int)(this.ExpiresUtc - TimestampEncoder.Epoch).TotalSeconds);
			writer.Flush();
		}

		/// <summary>
		/// Initializes the fields on this instance from the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public void Deserialize(Stream stream) {
			Requires.NotNull(stream, "stream");
			Requires.That(stream.CanRead, "stream", "requires stream.CanRead");

			var reader = new BinaryReader(stream);
			this.IsPrivateAssociation = reader.ReadBoolean();
			this.Secret = reader.ReadBuffer(256);
			this.ExpiresUtc = TimestampEncoder.Epoch + TimeSpan.FromSeconds(reader.ReadInt32());
		}

		/// <summary>
		/// Creates the formatter used for serialization of this type.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store used when signing or encrypting.</param>
		/// <param name="bucket">The bucket in which symmetric keys are stored for signing/encrypting data.</param>
		/// <param name="minimumAge">The minimum age.</param>
		/// <returns>
		/// A formatter for serialization.
		/// </returns>
		internal static IDataBagFormatter<AssociationDataBag> CreateFormatter(ICryptoKeyStore cryptoKeyStore, string bucket, TimeSpan? minimumAge = null) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
			Requires.NotNullOrEmpty(bucket, "bucket");
			return new BinaryDataBagFormatter<AssociationDataBag>(cryptoKeyStore, bucket, signed: true, encrypted: true, minimumAge: minimumAge);
		}
	}
}
