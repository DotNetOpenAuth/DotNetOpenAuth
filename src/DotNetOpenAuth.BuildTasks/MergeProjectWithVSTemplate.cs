//-----------------------------------------------------------------------
// <copyright file="MergeProjectWithVSTemplate.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using System.Xml.Linq;
	using System.IO;
	using Microsoft.Build.BuildEngine;
	using System.Diagnostics.Contracts;

	public class MergeProjectWithVSTemplate : Task {
		internal const string VSTemplateNamespace = "http://schemas.microsoft.com/developer/vstemplate/2005";

		[Required]
		public string[] ProjectItemTypes { get; set; }

		[Required]
		public string[] ReplaceParametersExtensions { get; set; }

		[Required]
		public ITaskItem[] Templates { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			foreach(ITaskItem sourceTemplateTaskItem in this.Templates) {
				var template = XElement.Load(sourceTemplateTaskItem.ItemSpec);
				var templateContentElement = template.Element(XName.Get("TemplateContent", VSTemplateNamespace));
				var projectElement = templateContentElement.Element(XName.Get("Project", VSTemplateNamespace));
				if (projectElement == null) {
					Log.LogMessage("Skipping merge of \"{0}\" with a project because no project was referenced from the template.", sourceTemplateTaskItem.ItemSpec);
					continue;
				}

				var projectPath = Path.Combine(Path.GetDirectoryName(sourceTemplateTaskItem.ItemSpec), projectElement.Attribute("File").Value);
				Log.LogMessage("Merging project \"{0}\" with \"{1}\".", projectPath, sourceTemplateTaskItem.ItemSpec);
				var sourceProject = new Project();
				sourceProject.Load(projectPath);

				// Collect the project items from the project that are appropriate
				// to include in the .vstemplate file.
				var itemsByFolder = from item in sourceProject.EvaluatedItems.Cast<BuildItem>()
							where this.ProjectItemTypes.Contains(item.Name)
							orderby item.Include
							group item by Path.GetDirectoryName(item.Include);
				foreach (var folder in itemsByFolder) {
					XElement parentNode = FindOrCreateParent(folder.Key, projectElement);

					foreach (var item in folder) {
						bool replaceParameters = this.ReplaceParametersExtensions.Contains(Path.GetExtension(item.Include));
						var itemName = XName.Get("ProjectItem", VSTemplateNamespace);
						var projectItem = parentNode.Elements(itemName).FirstOrDefault(el => string.Equals(el.Value, Path.GetFileName(item.Include), StringComparison.OrdinalIgnoreCase));
						if (projectItem == null) {
							projectItem = new XElement(itemName, Path.GetFileName(item.Include));
							parentNode.Add(projectItem);
						}
						if (replaceParameters) {
							projectItem.SetAttributeValue("ReplaceParameters", "true");
						}
					}
				}

				template.Save(sourceTemplateTaskItem.ItemSpec);
			}

			return !Log.HasLoggedErrors;
		}

		private static XElement FindOrCreateParent(string directoryName, XElement projectElement) {
			Contract.Requires<ArgumentNullException>(projectElement != null);

			if (string.IsNullOrEmpty(directoryName)) {
				return projectElement;
			}

			string[] segments = directoryName.Split(Path.DirectorySeparatorChar);
			XElement parent = projectElement;
			for (int i = 0; i < segments.Length; i++) {
				var candidateName = XName.Get("Folder", VSTemplateNamespace);
				var candidate = parent.Elements(XName.Get("Folder", VSTemplateNamespace)).FirstOrDefault(n => n.Attribute("Name").Value == segments[i]);
				if (candidate == null) {
					candidate = new XElement(
						candidateName,
						new XAttribute("Name", segments[i]));
					parent.Add(candidate);
				}

				parent = candidate;
			}

			return parent;
		}
	}
}
