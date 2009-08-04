//-----------------------------------------------------------------------
// <copyright file="Trim.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Trims item identities.
	/// </summary>
	public class Trim : Task {
		/// <summary>
		/// Gets or sets the characters that should be trimmed off if found at the start of items' ItemSpecs.
		/// </summary>
		public string StartCharacters { get; set; }

		/// <summary>
		/// Gets or sets the characters that should be trimmed off if found at the end of items' ItemSpecs.
		/// </summary>
		public string EndCharacters { get; set; }

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
				if (!string.IsNullOrEmpty(this.StartCharacters)) {
					this.Outputs[i].ItemSpec = this.Outputs[i].ItemSpec.TrimStart(this.StartCharacters.ToCharArray());
				}
				if (!string.IsNullOrEmpty(this.EndCharacters)) {
					this.Outputs[i].ItemSpec = this.Outputs[i].ItemSpec.TrimEnd(this.EndCharacters.ToCharArray());
				}
			}

			return true;
		}
	}
}
