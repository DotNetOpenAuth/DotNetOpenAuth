//-----------------------------------------------------------------------
// <copyright file="CopyWithTokenSubstitution.cs" company="Outercurve Foundation">
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

	/// <summary>
	/// Copies files and performs a search and replace for given tokens in their contents during the process.
	/// </summary>
	public class CopyWithTokenSubstitution : Task {
		/// <summary>
		/// Gets or sets the files to copy.
		/// </summary>
		/// <value>The files to copy.</value>
		[Required]
		public ITaskItem[] SourceFiles { get; set; }

		/// <summary>
		/// Gets or sets a list of files to copy the source files to.
		/// </summary>
		/// <value>The list of files to copy the source files to.</value>
		[Required]
		public ITaskItem[] DestinationFiles { get; set; }

		/// <summary>
		/// Gets or sets the destination files actually copied to.
		/// </summary>
		/// <remarks>
		/// In the case of error partway through, or files not copied due to already being up to date,
		/// this can be a subset of the <see cref="DestinationFiles"/> array.
		/// </remarks>
		[Output]
		public ITaskItem[] CopiedFiles { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		/// <returns><c>true</c> if the operation was successful.</returns>
		public override bool Execute() {
			if (this.SourceFiles.Length != this.DestinationFiles.Length) {
				Log.LogError("{0} inputs and {1} outputs given.", this.SourceFiles.Length, this.DestinationFiles.Length);
				return false;
			}

			var copiedFiles = new List<ITaskItem>(this.DestinationFiles.Length);

			for (int i = 0; i < this.SourceFiles.Length; i++) {
				string sourcePath = this.SourceFiles[i].ItemSpec;
				string destPath = this.DestinationFiles[i].ItemSpec;
				bool skipUnchangedFiles;
				bool.TryParse(this.SourceFiles[i].GetMetadata("SkipUnchangedFiles"), out skipUnchangedFiles);

				if (!Directory.Exists(Path.GetDirectoryName(destPath))) {
					Directory.CreateDirectory(Path.GetDirectoryName(destPath));
				}

				if (string.IsNullOrEmpty(this.SourceFiles[i].GetMetadata("BeforeTokens"))) {
					// this is just a standard copy without token substitution
					if (skipUnchangedFiles && File.GetLastWriteTimeUtc(sourcePath) == File.GetLastWriteTimeUtc(destPath)) {
						Log.LogMessage(MessageImportance.Low, "Skipping \"{0}\" -> \"{1}\" because the destination is up to date.", sourcePath, destPath);
						continue;
					}

					Log.LogMessage(MessageImportance.Normal, "Copying file from \"{0}\" to \"{1}\"", sourcePath, destPath);
					File.Copy(sourcePath, destPath, true);
				} else {
					// We deliberably consider newer destination files to be up-to-date rather than
					// requiring equality because this task modifies the destination file while copying.
					if (skipUnchangedFiles && File.GetLastWriteTimeUtc(sourcePath) < File.GetLastWriteTimeUtc(destPath)) {
						Log.LogMessage(MessageImportance.Low, "Skipping \"{0}\" -> \"{1}\" because the destination is up to date.", sourcePath, destPath);
						continue;
					}

					Log.LogMessage(MessageImportance.Normal, "Transforming \"{0}\" -> \"{1}\"", sourcePath, destPath);

					string[] beforeTokens = this.SourceFiles[i].GetMetadata("BeforeTokens").Split(';');
					string[] afterTokens = this.SourceFiles[i].GetMetadata("AfterTokens").Split(';');
					if (beforeTokens.Length != afterTokens.Length) {
						Log.LogError("Unequal number of before and after tokens.  Before: \"{0}\". After \"{1}\".", beforeTokens, afterTokens);
						return false;
					}

					using (StreamReader sr = File.OpenText(sourcePath)) {
						using (StreamWriter sw = File.CreateText(destPath)) {
							StringBuilder line = new StringBuilder();
							while (!sr.EndOfStream) {
								line.Length = 0;
								line.Append(sr.ReadLine());
								for (int j = 0; j < beforeTokens.Length; j++) {
									line.Replace(beforeTokens[j], afterTokens[j]);
								}

								sw.WriteLine(line);
							}
						}
					}
				}

				copiedFiles.Add(this.DestinationFiles[i]);
			}

			this.CopiedFiles = copiedFiles.ToArray();
			return true;
		}
	}
}
