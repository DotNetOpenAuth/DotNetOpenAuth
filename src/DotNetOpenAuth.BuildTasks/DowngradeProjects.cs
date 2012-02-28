//-----------------------------------------------------------------------
// <copyright file="DowngradeProjects.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Downgrades Visual Studio 2010 solutions and projects so that they load in Visual Studio 2008.
	/// </summary>
	public class DowngradeProjects : Task {
		/// <summary>
		/// Gets or sets the projects and solutions to downgrade.
		/// </summary>
		[Required]
		public ITaskItem[] Projects { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether project files are downgraded and re-saved to the same paths.
		/// </summary>
		public bool InPlaceDowngrade { get; set; }

		/// <summary>
		/// Gets or sets the set of newly created project files.  Empty if <see cref="InPlaceDowngrade"/> is <c>true</c>.
		/// </summary>
		[Output]
		public ITaskItem[] DowngradedProjects { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether ASP.NET MVC 2 projects are downgraded to MVC 1.0.
		/// </summary>
		public bool DowngradeMvc2ToMvc1 { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			var newProjectToOldProjectMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var createdProjectFiles = new List<TaskItem>();

			foreach (ITaskItem taskItem in this.Projects) {
				switch (GetClassification(taskItem)) {
					case ProjectClassification.VS2010Project:
					case ProjectClassification.VS2010Solution:
						string projectNameForVS2008 = InPlaceDowngrade
														? taskItem.ItemSpec
														: Path.Combine(
															Path.GetDirectoryName(taskItem.ItemSpec),
															Path.GetFileNameWithoutExtension(taskItem.ItemSpec) + "-vs2008" +
															Path.GetExtension(taskItem.ItemSpec));
						newProjectToOldProjectMapping[taskItem.ItemSpec] = projectNameForVS2008;
						break;
				}
			}

			foreach (ITaskItem taskItem in this.Projects) {
				switch (GetClassification(taskItem)) {
					case ProjectClassification.VS2010Project:
						this.Log.LogMessage(MessageImportance.Low, "Downgrading project \"{0}\".", taskItem.ItemSpec);
						var project = new Project();
						project.Load(taskItem.ItemSpec, ProjectLoadSettings.IgnoreMissingImports);
						project.DefaultToolsVersion = "3.5";

						if (this.DowngradeMvc2ToMvc1) {
							string projectTypeGuids = project.GetEvaluatedProperty("ProjectTypeGuids");
							if (!string.IsNullOrEmpty(projectTypeGuids)) {
								projectTypeGuids = projectTypeGuids.Replace("{F85E285D-A4E0-4152-9332-AB1D724D3325}", "{603c0e0b-db56-11dc-be95-000d561079b0}");
								project.SetProperty("ProjectTypeGuids", projectTypeGuids);
							}
						}

						// MSBuild v3.5 doesn't support the GetDirectoryNameOfFileAbove function
						var enlistmentInfoImports = project.Imports.Cast<Import>().Where(i => i.ProjectPath.IndexOf("[MSBuild]::GetDirectoryNameOfFileAbove", StringComparison.OrdinalIgnoreCase) >= 0);
						enlistmentInfoImports.ToList().ForEach(i => project.Imports.RemoveImport(i));

						// Web projects usually have an import that includes these substrings));)
						foreach (Import import in project.Imports) {
							import.ProjectPath = import.ProjectPath
								.Replace("$(MSBuildExtensionsPath32)", "$(MSBuildExtensionsPath)")
								.Replace("VisualStudio\\v10.0", "VisualStudio\\v9.0");
						}

						// VS2010 won't let you have a System.Core reference, but VS2008 requires it.
						BuildItemGroup references = project.GetEvaluatedItemsByName("Reference");
						if (!references.Cast<BuildItem>().Any(item => item.FinalItemSpec.StartsWith("System.Core", StringComparison.OrdinalIgnoreCase))) {
							project.AddNewItem("Reference", "System.Core");
						}

						// Rewrite ProjectReferences to other renamed projects.
						BuildItemGroup projectReferences = project.GetEvaluatedItemsByName("ProjectReference");
						foreach (var mapping in newProjectToOldProjectMapping) {
							string oldName = Path.GetFileName(mapping.Key);
							string newName = Path.GetFileName(mapping.Value);
							foreach (BuildItem projectReference in projectReferences) {
								projectReference.Include = Regex.Replace(projectReference.Include, oldName, newName, RegexOptions.IgnoreCase);
							}
						}

						project.Save(newProjectToOldProjectMapping[taskItem.ItemSpec]);
						createdProjectFiles.Add(new TaskItem(taskItem) { ItemSpec = newProjectToOldProjectMapping[taskItem.ItemSpec] });
						break;
					case ProjectClassification.VS2010Solution:
						this.Log.LogMessage(MessageImportance.Low, "Downgrading solution \"{0}\".", taskItem.ItemSpec);
						string[] contents = File.ReadAllLines(taskItem.ItemSpec);
						if (contents[1] != "Microsoft Visual Studio Solution File, Format Version 11.00" ||
							contents[2] != "# Visual Studio 2010") {
							this.Log.LogError("Unrecognized solution file header in \"{0}\".", taskItem.ItemSpec);
							break;
						}

						contents[1] = "Microsoft Visual Studio Solution File, Format Version 10.00";
						contents[2] = "# Visual Studio 2008";

						for (int i = 3; i < contents.Length; i++) {
							contents[i] = contents[i].Replace("TargetFrameworkMoniker = \".NETFramework,Version%3Dv", "TargetFramework = \"");
						}

						foreach (var mapping in newProjectToOldProjectMapping) {
							string oldName = Path.GetFileName(mapping.Key);
							string newName = Path.GetFileName(mapping.Value);
							for (int i = 0; i < contents.Length; i++) {
								contents[i] = Regex.Replace(contents[i], oldName, newName, RegexOptions.IgnoreCase);
							}
						}

						File.WriteAllLines(newProjectToOldProjectMapping[taskItem.ItemSpec], contents);
						createdProjectFiles.Add(new TaskItem(taskItem) { ItemSpec = newProjectToOldProjectMapping[taskItem.ItemSpec] });
						break;
					default:
						this.Log.LogWarning("Unrecognized project type for \"{0}\".", taskItem.ItemSpec);
						break;
				}
			}

			if (InPlaceDowngrade) {
				this.DowngradedProjects = new ITaskItem[0];
			} else {
				this.DowngradedProjects = createdProjectFiles.ToArray();
			}

			return !this.Log.HasLoggedErrors;
		}

		private static ProjectClassification GetClassification(ITaskItem taskItem) {
			if (Path.GetExtension(taskItem.ItemSpec).EndsWith("proj", StringComparison.OrdinalIgnoreCase)) {
				return ProjectClassification.VS2010Project;
			} else if (Path.GetExtension(taskItem.ItemSpec).Equals(".sln", StringComparison.OrdinalIgnoreCase)) {
				return ProjectClassification.VS2010Solution;
			} else {
				return ProjectClassification.Unrecognized;
			}
		}

		private enum ProjectClassification {
			VS2010Project,
			VS2010Solution,
			Unrecognized,
		}
	}
}
