//-----------------------------------------------------------------------
// <copyright file="HardLinkCopy.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class HardLinkCopy : Task {
		[Required]
		public ITaskItem[] SourceFiles { get; set; }

		[Required]
		public ITaskItem[] DestinationFiles { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			if (this.SourceFiles.Length != this.DestinationFiles.Length) {
				this.Log.LogError("SourceFiles has {0} elements and DestinationFiles has {1} elements.", this.SourceFiles.Length, this.DestinationFiles.Length);
				return false;
			}

			for (int i = 0; i < this.SourceFiles.Length; i++) {
				bool hardLink;
				bool.TryParse(this.DestinationFiles[i].GetMetadata("HardLink"), out hardLink);
				string sourceFile = this.SourceFiles[i].ItemSpec;
				string destinationFile = this.DestinationFiles[i].ItemSpec;
				this.Log.LogMessage(
					MessageImportance.Low,
					"Copying {0} -> {1}{2}.",
					sourceFile,
					destinationFile,
					hardLink ? " as hard link" : string.Empty);

				if (!Directory.Exists(Path.GetDirectoryName(destinationFile))) {
					Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
				}

				if (hardLink) {
					if (File.Exists(destinationFile)) {
						File.Delete(destinationFile);
					}
					NativeMethods.CreateHardLink(sourceFile, destinationFile);
				} else {
					File.Copy(sourceFile, destinationFile, true);
				}
			}

			return !this.Log.HasLoggedErrors;
		}
	}
}
