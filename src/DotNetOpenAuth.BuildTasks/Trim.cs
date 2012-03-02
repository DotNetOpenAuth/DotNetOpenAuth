//-----------------------------------------------------------------------
// <copyright file="Trim.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Trims item identities or metadata.
	/// </summary>
	public class Trim : Task {
		/// <summary>
		/// Gets or sets the name of the metadata to trim.  Leave empty or null to operate on itemspec.
		/// </summary>
		/// <value>The name of the metadata.</value>
		public string MetadataName { get; set; }

		/// <summary>
		/// Gets or sets the characters that should be trimmed off if found at the start of items' ItemSpecs.
		/// </summary>
		public string StartCharacters { get; set; }

		/// <summary>
		/// Gets or sets the characters that should be trimmed off if found at the end of items' ItemSpecs.
		/// </summary>
		public string EndCharacters { get; set; }

		/// <summary>
		/// Gets or sets the substring that should be trimmed along with everything that appears after it.
		/// </summary>
		public string AllAfter { get; set; }

		/// <summary>
		/// Gets or sets the items with ItemSpec's to be trimmed.
		/// </summary>
		[Required]
		public ITaskItem[] Inputs { get; set; }

		/// <summary>
		/// Gets or sets the items with trimmed ItemSpec strings.
		/// </summary>
		[Output]
		public ITaskItem[] Outputs { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		/// <returns>A value indicating whether the task completed successfully.</returns>
		public override bool Execute() {
			this.Outputs = new ITaskItem[this.Inputs.Length];
			for (int i = 0; i < this.Inputs.Length; i++) {
				this.Outputs[i] = new TaskItem(this.Inputs[i]);
				string value = string.IsNullOrEmpty(this.MetadataName) ? this.Outputs[i].ItemSpec : this.Outputs[i].GetMetadata(this.MetadataName);
				if (!string.IsNullOrEmpty(this.StartCharacters)) {
					value = value.TrimStart(this.StartCharacters.ToCharArray());
				}
				if (!string.IsNullOrEmpty(this.EndCharacters)) {
					value = value.TrimEnd(this.EndCharacters.ToCharArray());
				}
				if (!string.IsNullOrEmpty(this.AllAfter)) {
					int index = value.IndexOf(this.AllAfter);
					if (index >= 0) {
						value = value.Substring(0, index);
					}
				}
				if (string.IsNullOrEmpty(this.MetadataName)) {
					this.Outputs[i].ItemSpec = value;
				} else {
					this.Outputs[i].SetMetadata(this.MetadataName, value);
				}
			}

			return true;
		}
	}
}
