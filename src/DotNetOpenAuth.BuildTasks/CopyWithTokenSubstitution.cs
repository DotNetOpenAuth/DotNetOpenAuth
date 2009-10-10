//-----------------------------------------------------------------------
// <copyright file="CopyWithTokenSubstitution.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
		/// Gets or sets a value indicating whether the task should
		/// skip the copying of files that are unchanged between the source and destination.
		/// </summary>
		/// <value>
		/// 	<c>true</c> to skip copying files where the destination files are newer than the source files; otherwise, <c>false</c> to copy all files.
		/// </value>
		public bool SkipUnchangedFiles { get; set; }

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
		/// Executes this instance.
		/// </summary>
		/// <returns><c>true</c> if the operation was successful.</returns>
		public override bool Execute() {
			if (this.SourceFiles.Length != this.DestinationFiles.Length) {
				Log.LogError("{0} inputs and {1} outputs given.", this.SourceFiles.Length, this.DestinationFiles.Length);
				return false;
			}

			for (int i = 0; i < this.SourceFiles.Length; i++) {
				string sourcePath = this.SourceFiles[i].ItemSpec;
				string destPath = this.DestinationFiles[i].ItemSpec;

				if (this.SkipUnchangedFiles && File.GetLastWriteTimeUtc(sourcePath) < File.GetLastWriteTimeUtc(destPath)) {
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
						while (line.Append(sr.ReadLine()) != null) {
							for (int j = 0; j < beforeTokens.Length; j++) {
								line.Replace(beforeTokens[j], afterTokens[j]);
							}

							sw.WriteLine(line);

							// Clear out the line buffer for the next input.
							line.Length = 0;
						}
					}
				}
			}

			return true;
		}
	}
}
