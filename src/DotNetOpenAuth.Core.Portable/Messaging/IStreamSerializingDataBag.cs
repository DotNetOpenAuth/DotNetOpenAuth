//-----------------------------------------------------------------------
// <copyright file="IStreamSerializingDataBag.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.IO;

	/// <summary>
	/// An interface implemented by <see cref="DataBag"/>-derived types that support binary serialization.
	/// </summary>
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
}
