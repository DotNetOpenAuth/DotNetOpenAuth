//-----------------------------------------------------------------------
// <copyright file="Reporting.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Configuration;
	using System.IO;
	using System.Reflection;
	using System.Diagnostics;
	using System.IO.IsolatedStorage;

	internal static class Reporting {
		private static StreamWriter writer;

		static Reporting() {
			Enabled = DotNetOpenAuthSection.Configuration.Reporting.Enabled;
			if (Enabled) {
				try {
					IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForDomain();
					var assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
					var fileStream = new IsolatedStorageFileStream("reporting.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
					writer = new StreamWriter(fileStream, Encoding.UTF8);
					writer.AutoFlush = true;
					writer.WriteLine();
					writer.WriteLine(Util.LibraryVersion);
				} catch {
					// This is supposed to be as low-risk as possible, so if it fails, just disable reporting.
					Enabled = false;
				}
			}
		}

		internal static bool Enabled { get; set; }

		internal static void OnAuthenticated() {
			if (!Enabled) {
				return;
			}

			writer.Write("L");
		}
	}
}
