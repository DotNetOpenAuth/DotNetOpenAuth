//-----------------------------------------------------------------------
// <copyright file="MultipartContentMember.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Describes a part from a multi-part POST.
	/// </summary>
	public struct MultipartContentMember {
		/// <summary>
		/// Initializes a new instance of the <see cref="MultipartContentMember"/> struct.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="name">The name of this part as it may come from an HTML form.</param>
		/// <param name="fileName">Name of the file.</param>
		public MultipartContentMember(HttpContent content, string name = null, string fileName = null)
			: this() {
			this.Content = content;
			this.Name = name;
			this.FileName = fileName;
		}

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>
		/// The content.
		/// </value>
		public HttpContent Content { get; set; }

		/// <summary>
		/// Gets or sets the HTML form name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>
		/// The name of the file.
		/// </value>
		public string FileName { get; set; }
	}
}
