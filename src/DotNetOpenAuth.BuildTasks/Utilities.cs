//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	internal static class Utilities {
		internal static string SuppressCharacters(string fileName, char[] suppress, char replacement) {
			Contract.Requires<ArgumentNullException>(fileName != null);
			Contract.Requires<ArgumentNullException>(suppress != null);

			if (fileName.IndexOfAny(suppress) < 0) {
				return fileName;
			}

			StringBuilder builder = new StringBuilder(fileName);
			foreach (char ch in suppress) {
				builder.Replace(ch, replacement);
			}

			return builder.ToString();
		}
	}
}
