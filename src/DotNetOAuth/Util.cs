//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Reflection;

	/// <summary>
	/// A grab-bag utility class.
	/// </summary>
	internal static class Util {
		/// <summary>
		/// Gets a human-readable description of the library name and version, including
		/// whether the build is an official or private one.
		/// </summary>
		public static string LibraryVersion {
			get {
				string assemblyFullName = Assembly.GetExecutingAssembly().FullName;
				bool official = assemblyFullName.Contains("PublicKeyToken=2780ccd10d57b246");

				// We use InvariantCulture since this is used for logging.
				return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", assemblyFullName, official ? "official" : "private");
			}
		}

		/// <summary>
		/// Enumerates through the individual set bits in a flag enum.
		/// </summary>
		/// <param name="flags">The flags enum value.</param>
		/// <returns>An enumeration of just the <i>set</i> bits in the flags enum.</returns>
		internal static IEnumerable<long> GetIndividualFlags(Enum flags) {
			long flagsLong = Convert.ToInt64(flags);
			for (int i = 0; i < sizeof(long) * 8; i++) { // long is the type behind the largest enum
				// Select an individual application from the scopes.
				long individualFlagPosition = (long)Math.Pow(2, i);
				long individualFlag = flagsLong & individualFlagPosition;
				if (individualFlag == individualFlagPosition) {
					yield return individualFlag;
				}
			}
		}
	}
}
