//-----------------------------------------------------------------------
// <copyright file="SuggestedStringsConverterContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.Collections;
	using System.ComponentModel.Design.Serialization;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Contract class for the <see cref="SuggestedStringsConverter"/> class.
	/// </summary>
	[ContractClassFor(typeof(SuggestedStringsConverter))]
	internal abstract class SuggestedStringsConverterContract : SuggestedStringsConverter {
		/// <summary>
		/// Gets the type to reflect over for the well known values.
		/// </summary>
		protected override Type WellKnownValuesType {
			get {
				Contract.Ensures(Contract.Result<Type>() != null);
				throw new NotImplementedException();
			}
		}
	}
}
