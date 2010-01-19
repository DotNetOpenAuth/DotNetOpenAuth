//-----------------------------------------------------------------------
// <copyright file="AddProjectItems.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using System.Collections;

	public class AddProjectItems : Task {
		/// <summary>
		/// Gets or sets the projects to add items to.
		/// </summary>
		/// <value>The projects.</value>
		[Required]
		public ITaskItem[] Projects { get; set; }

		/// <summary>
		/// Gets or sets the items to add to each project.
		/// </summary>
		/// <value>The items.</value>
		/// <remarks>
		/// Use the metadata "ItemType" on each item to specify the item type to use for the new
		/// project item.  If the metadata is absent, "None" is used as the item type.
		/// </remarks>
		[Required]
		public ITaskItem[] Items { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			foreach (var projectTaskItem in this.Projects) {
				var project = new Project();
				project.Load(projectTaskItem.ItemSpec);

				foreach (var projectItem in this.Items) {
					string itemType = projectItem.GetMetadata("ItemType");
					if (string.IsNullOrEmpty(itemType)) {
						itemType = "None";
					}
					BuildItem newItem = project.AddNewItem(itemType, projectItem.ItemSpec, false);
					var customMetadata = projectItem.CloneCustomMetadata();
					foreach (DictionaryEntry entry in customMetadata) {
						newItem.SetMetadata((string)entry.Key, (string)entry.Value);
					}
				}

				project.Save(projectTaskItem.ItemSpec);
			}

			return !this.Log.HasLoggedErrors;
		}
	}
}
