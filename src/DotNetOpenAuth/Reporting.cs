//-----------------------------------------------------------------------
// <copyright file="Reporting.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.IO.IsolatedStorage;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using DotNetOpenAuth.Configuration;

	/// <summary>
	/// The statistical reporting mechanism used so this library's project authors
	/// know what versions and features are in use.
	/// </summary>
	internal static class Reporting {
		/// <summary>
		/// The writer to use to log statistics.
		/// </summary>
		private static StreamWriter writer;

		/// <summary>
		/// Initializes static members of the <see cref="Reporting"/> class.
		/// </summary>
		static Reporting() {
			Enabled = DotNetOpenAuthSection.Configuration.Reporting.Enabled;
			if (Enabled) {
				try {
					writer = OpenReport();
					writer.WriteLine();
					writer.WriteLine(Util.LibraryVersion);
				} catch {
					// This is supposed to be as low-risk as possible, so if it fails, just disable reporting.
					Enabled = false;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this reporting is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		private static bool Enabled { get; set; }

		/// <summary>
		/// Called when an OpenID RP successfully authenticates someone.
		/// </summary>
		internal static void OnAuthenticated() {
			if (!Enabled) {
				return;
			}

			writer.Write("L");
		}

		/// <summary>
		/// Opens the report file for append.
		/// </summary>
		/// <returns>The writer to use.</returns>
		private static StreamWriter OpenReport() {
			IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForDomain();
			var assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
			var fileStream = new IsolatedStorageFileStream("reporting.txt", FileMode.Append, FileAccess.Write, FileShare.Read);
			var writer = new StreamWriter(fileStream, Encoding.UTF8);
			writer.AutoFlush = true;
			return writer;
		}
	}
}
