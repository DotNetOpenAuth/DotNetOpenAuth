//-----------------------------------------------------------------------
// <copyright file="InfoCardImage.cs" company="Dominick Baier, Andrew Arnott">
//     Copyright (c) Dominick Baier, Outercurve Foundation. All rights reserved.
// </copyright>
// <license>New BSD License</license>
//-----------------------------------------------------------------------

// embedded images
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_114x80.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_14x10.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_214x150.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_23x16.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_300x210.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_34x24.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_365x256.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_41x29.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_50x35.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_60x42.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_71x50.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_81x57.png", "image/png")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.Util.DefaultNamespace + ".InfoCard.infocard_92x64.png", "image/png")]

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;

	/// <summary>
	/// A set of sizes for which standard InfoCard icons are available.
	/// </summary>
	public enum InfoCardImageSize {
		/// <summary>
		/// A standard InfoCard icon with size 14x10
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size14x10,

		/// <summary>
		/// A standard InfoCard icon with size 23x16
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size23x16,

		/// <summary>
		/// A standard InfoCard icon with size 34x24
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size34x24,

		/// <summary>
		/// A standard InfoCard icon with size 41x29
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size41x29,

		/// <summary>
		/// A standard InfoCard icon with size 50x35
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size50x35,

		/// <summary>
		/// A standard InfoCard icon with size 60x42
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size60x42,

		/// <summary>
		/// A standard InfoCard icon with size 71x50
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size71x50,

		/// <summary>
		/// A standard InfoCard icon with size 92x64
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size92x64,

		/// <summary>
		/// A standard InfoCard icon with size 114x80
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size114x80,

		/// <summary>
		/// A standard InfoCard icon with size 164x108
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size164x108,

		/// <summary>
		/// A standard InfoCard icon with size 214x50
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size214x50,

		/// <summary>
		/// A standard InfoCard icon with size 300x210
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size300x210,

		/// <summary>
		/// A standard InfoCard icon with size 365x256
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "By design")]
		Size365x256,
	}

	/// <summary>
	/// Assists in selecting the InfoCard image to display in the user agent.
	/// </summary>
	internal static class InfoCardImage {
		/// <summary>
		/// The default size of the InfoCard icon to use.
		/// </summary>
		internal const InfoCardImageSize DefaultImageSize = InfoCardImageSize.Size114x80;

		/// <summary>
		/// The format to use when generating the image manifest resource stream name.
		/// </summary>
		private const string UrlFormatString = Util.DefaultNamespace + ".InfoCard.infocard_{0}.png";

		/// <summary>
		/// Gets the name of the image manifest resource stream for an InfoCard image of the given size.
		/// </summary>
		/// <param name="size">The size of the desired InfoCard image.</param>
		/// <returns>The manifest resource stream name.</returns>
		internal static string GetImageManifestResourceStreamName(InfoCardImageSize size) {
			string imageSize = size.ToString();
			Assumes.True(imageSize.Length >= 6);
			imageSize = imageSize.Substring(4);
			return string.Format(CultureInfo.InvariantCulture, UrlFormatString, imageSize);
		}
	}
}
