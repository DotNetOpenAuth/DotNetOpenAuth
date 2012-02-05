//-----------------------------------------------------------------------
// <copyright file="DiscoverProjectTemplates.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class DiscoverProjectTemplates : Task {
		[Required]
		public ITaskItem[] TopLevelTemplates { get; set; }

		[Output]
		public ITaskItem[] ProjectTemplates { get; set; }

		[Output]
		public ITaskItem[] ProjectTemplateContents { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			List<ITaskItem> projectTemplates = new List<ITaskItem>();
			List<ITaskItem> projectTemplateContents = new List<ITaskItem>();
			foreach (ITaskItem topLevelTemplate in this.TopLevelTemplates) {
				var vsTemplate = XElement.Load(topLevelTemplate.ItemSpec);
				var templateContent = vsTemplate.Element(XName.Get("TemplateContent", MergeProjectWithVSTemplate.VSTemplateNamespace));
				var projectCollection = templateContent.Element(XName.Get("ProjectCollection", MergeProjectWithVSTemplate.VSTemplateNamespace));
				var links = projectCollection.Elements(XName.Get("ProjectTemplateLink", MergeProjectWithVSTemplate.VSTemplateNamespace));
				var subTemplates = links.Select(
					link => (ITaskItem)new TaskItem(
						link.Value,
						new Dictionary<string, string> { 
						{ "TopLevelTemplate", topLevelTemplate.ItemSpec },
						{ "TopLevelTemplateFileName", Path.GetFileNameWithoutExtension(topLevelTemplate.ItemSpec) },
					}));
				projectTemplates.AddRange(subTemplates);

				foreach (var link in links.Select(link => link.Value)) {
					string[] files = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(topLevelTemplate.ItemSpec), Path.GetDirectoryName(link)), "*.*", SearchOption.AllDirectories);
					projectTemplateContents.AddRange(files.Select(file => (ITaskItem)new TaskItem(
						file,
						new Dictionary<string, string> { 
						{ "TopLevelTemplate", topLevelTemplate.ItemSpec },
						{ "TopLevelTemplateFileName", Path.GetFileNameWithoutExtension(topLevelTemplate.ItemSpec) },
					})));
				}
			}

			this.ProjectTemplates = projectTemplates.ToArray();
			this.ProjectTemplateContents = projectTemplateContents.ToArray();

			return !this.Log.HasLoggedErrors;
		}
	}
}
