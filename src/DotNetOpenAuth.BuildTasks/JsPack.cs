//-----------------------------------------------------------------------
// <copyright file="JsPack.cs" company="Outercurve Foundation">
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
	/// Compresses .js files.
	/// </summary>
	public class JsPack : Task {
		/// <summary>
		/// The Javascript packer to use.
		/// </summary>
		private Dean.Edwards.ECMAScriptPacker packer = new Dean.Edwards.ECMAScriptPacker();

		/// <summary>
		/// Gets or sets the set of javascript files to compress.
		/// </summary>
		/// <value>The inputs.</value>
		[Required]
		public ITaskItem[] Inputs { get; set; }

		/// <summary>
		/// Gets or sets the paths where the packed javascript files should be saved.
		/// </summary>
		/// <value>The outputs.</value>
		[Required]
		public ITaskItem[] Outputs { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		/// <returns>A value indicating whether the packing was successful.</returns>
		public override bool Execute() {
			if (this.Inputs.Length != this.Outputs.Length) {
				Log.LogError("{0} inputs and {1} outputs given.", this.Inputs.Length, this.Outputs.Length);
				return false;
			}

			for (int i = 0; i < this.Inputs.Length; i++) {
				if (!File.Exists(this.Outputs[i].ItemSpec) || File.GetLastWriteTime(this.Outputs[i].ItemSpec) < File.GetLastWriteTime(this.Inputs[i].ItemSpec)) {
					Log.LogMessage(MessageImportance.Normal, TaskStrings.PackingJsFile, this.Inputs[i].ItemSpec, this.Outputs[i].ItemSpec);
					string input = File.ReadAllText(this.Inputs[i].ItemSpec);
					string output = this.packer.Pack(input);
					if (!Directory.Exists(Path.GetDirectoryName(this.Outputs[i].ItemSpec))) {
						Directory.CreateDirectory(Path.GetDirectoryName(this.Outputs[i].ItemSpec));
					}

					// Minification removes all comments, including copyright notices
					// that must remain.  So if there's metadata on this item with
					// a copyright notice on it, stick it on the front of the file.
					string copyright = this.Inputs[i].GetMetadata("Copyright");
					if (!string.IsNullOrEmpty(copyright)) {
						output = "/*" + copyright + "*/" + output;
					}

					File.WriteAllText(this.Outputs[i].ItemSpec, output, Encoding.UTF8);
				} else {
					Log.LogMessage(MessageImportance.Low, TaskStrings.SkipPackingJsFile, this.Inputs[i].ItemSpec);
				}
			}

			return true;
		}
	}
}
