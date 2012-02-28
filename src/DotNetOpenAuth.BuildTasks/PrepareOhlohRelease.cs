//-----------------------------------------------------------------------
// <copyright file="PrepareOhlohRelease.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Creates an Ohloh.net upload instruct XLM file and bash script that uses it.
	/// </summary>
	public class PrepareOhlohRelease : Task {
		/// <summary>
		/// Initializes a new instance of the <see cref="PrepareOhlohUpload"/> class.
		/// </summary>
		public PrepareOhlohRelease() {
		}

		[Required]
		public string Release { get; set; }

		public string ReleaseNotes { get; set; }

		[Required]
		public string InstructFile { get; set; }

		[Required]
		public string UploadScript { get; set; }

		[Required]
		public string OhlohUser { get; set; }

		[Required]
		public string OhlohProject { get; set; }

		[Required]
		public ITaskItem[] RedistributableFiles { get; set; }

		public override bool Execute() {
			this.WriteInstructFile();
			this.WriteUploadScript();

			return !this.Log.HasLoggedErrors;
		}

		private void WriteInstructFile() {
			var packages = from redist in this.RedistributableFiles
						   let file = new { Path = redist.ItemSpec, Package = redist.GetMetadata("Package"), Platform = redist.GetMetadata("Platform"), Icon = redist.GetMetadata("Icon") }
						   group file by file.Package into package
						   select new XElement(
							   "package",
							   new XAttribute("name", package.Key),
							   new XElement(
								   "releases",
								   new XElement(
									   "release",
									   new XAttribute("name", this.Release),
									   new XAttribute("date", XmlConvert.ToString(DateTime.Now)),
									   new XElement("notes", this.ReleaseNotes),
									   new XElement(
										   "files",
										   package.Select(
											f => new XElement(
												"file",
												new XAttribute("name", Path.GetFileName(f.Path)),
												new XAttribute("date", XmlConvert.ToString(DateTime.Now)),
												new XAttribute("platform", f.Platform),
												new XAttribute("icon", f.Icon)
											)
										   )
									   )
								   )
							   )
						   );

			var instructXml = new XElement("packages", packages);

			var writerSettings = new XmlWriterSettings {
				OmitXmlDeclaration = true,
				Indent = true,
				IndentChars = "\t",
			};
			using (var writer = XmlWriter.Create(this.InstructFile, writerSettings)) {
				instructXml.Save(writer);
			}

			this.Log.LogMessage("Ohloh instruct file written to: \"{0}\"", this.InstructFile);

		}

		private void WriteUploadScript() {
			int longestPath = Math.Max(
				GetLinuxPath(Path.GetFullPath(this.InstructFile)).Length,
				this.RedistributableFiles.Max(f => GetLinuxPath(f.GetMetadata("FullPath")).Length));

			using (StreamWriter writer = new StreamWriter(this.UploadScript)) {
				writer.WriteLine("#!/bin/bash");
				writer.WriteLine();
				foreach (var file in this.RedistributableFiles) {
					writer.WriteLine("scp {0,-" + longestPath + "} {1}@upload.ohloh.net:{2}/files", GetLinuxPath(file.GetMetadata("FullPath")), this.OhlohUser, this.OhlohProject);
				}

				writer.WriteLine();
				writer.WriteLine("scp {0,-" + longestPath + "} {1}@upload.ohloh.net:{2}/instructs", GetLinuxPath(Path.GetFullPath(this.InstructFile)), this.OhlohUser, this.OhlohProject);
				writer.WriteLine();
				writer.WriteLine("# Download the instruct log by executing this command:");
				writer.WriteLine("# scp {0}@upload.ohloh.net:{1}/logs/upload.log .", this.OhlohUser, this.OhlohProject);
			}

			this.Log.LogMessage("Ohloh upload script written to: \"{0}\".", this.UploadScript);
		}

		private static string GetLinuxPath(string windowsPath) {
			if (String.IsNullOrEmpty(windowsPath)) {
				throw new ArgumentNullException("windowsPath");
			}

			string linuxPath = Regex.Replace(
				windowsPath,
				@"^([A-Za-z])\:",
				m => "/" + m.Groups[1].Value)
				.Replace('\\', '/')
				.Replace(" ", "\\ ");
			return linuxPath;
		}
	}
}
