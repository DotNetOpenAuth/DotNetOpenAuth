//-----------------------------------------------------------------------
// <copyright file="IStreamSerializingDataBag.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;

	/// <summary>
	/// An interface implemented by <see cref="DataBag"/>-derived types that support binary serialization.
	/// </summary>
	[ContractClass(typeof(IStreamSerializingDataBaContract))]
	internal interface IStreamSerializingDataBag {
		/// <summary>
		/// Serializes the instance to the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		void Serialize(Stream stream);

		/// <summary>
		/// Initializes the fields on this instance from the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		void Deserialize(Stream stream);
	}

	/// <summary>
	/// Code Contract for the <see cref="IStreamSerializingDataBag"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IStreamSerializingDataBag))]
	internal abstract class IStreamSerializingDataBaContract : IStreamSerializingDataBag {
		/// <summary>
		/// Serializes the instance to the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		void IStreamSerializingDataBag.Serialize(Stream stream) {
			Contract.Requires(stream != null);
			Contract.Requires(stream.CanWrite);
			throw new NotImplementedException();
		}

		/// <summary>
		/// Initializes the fields on this instance from the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		void IStreamSerializingDataBag.Deserialize(Stream stream) {
			Contract.Requires(stream != null);
			Contract.Requires(stream.CanRead);
			throw new NotImplementedException();
		}
	}
}
