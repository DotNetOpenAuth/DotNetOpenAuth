using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

public static class Util {
	public static void EnsureHttpsByRedirection() {
		if (HttpContext.Current.Request.Url.Scheme == Uri.UriSchemeHttp) {
			UriBuilder requestUri = new UriBuilder(HttpContext.Current.Request.Url);
			requestUri.Scheme = Uri.UriSchemeHttps;
			requestUri.Host = "test-id.org";
			requestUri.Port = 443;
			HttpContext.Current.Response.Redirect(requestUri.Uri.AbsoluteUri);
		}
	}

	public static string BuildErrorMessage(Exception ex) {
		StringBuilder sb = new StringBuilder();
		while (ex != null) {
			sb.Append(ex.Message);
			ex = ex.InnerException;
		}

		return sb.ToString();
	}

	public static Uri GetPublicPageSourcePath() {
		return GetPublicPathForLocalPath(GetPageLocalPath());
	}

	public static Uri GetPublicPageCodeBehindSourcePath() {
		string pageFileSystemPath = GetPageLocalPath();
		string codeBehindPath = pageFileSystemPath + ".cs";
		if (File.Exists(codeBehindPath)) {
			return GetPublicPathForLocalPath(codeBehindPath);
		} else {
			return null;
		}
	}

	private static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo) {
		DirectoryInfo parentDirInfo = dirInfo.Parent;
		if (null == parentDirInfo)
			return dirInfo.Name;
		return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
							parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
	}

	private static string GetProperFilePathCapitalization(string filename) {
		FileInfo fileInfo = new FileInfo(filename);
		DirectoryInfo dirInfo = fileInfo.Directory;
		return Path.Combine(GetProperDirectoryCapitalization(dirInfo),
							dirInfo.GetFiles(fileInfo.Name)[0].Name);
	}

	private static Uri GetPublicPathForLocalPath(string localPath) {
		Uri rootFileSystemPath = new Uri(GetGitRepoRoot());
		string relativeFileSystemPath = rootFileSystemPath.MakeRelative(new Uri(localPath));
		return new Uri("http://github.com/aarnott/dotnetopenid/tree/osis/" + relativeFileSystemPath);
	}

	private static string GetPageLocalPath() {
		return GetProperFilePathCapitalization(HttpContext.Current.Request.PhysicalPath);
	}

	private static string GetGitRepoRoot() {
		string directory = Path.GetDirectoryName(GetPageLocalPath());
		do {
			if (Directory.GetDirectories(directory, ".git").Length == 1) {
				break;
			}
			directory = Path.GetDirectoryName(directory);
		} while (true);

		if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString())) {
			directory += Path.DirectorySeparatorChar;
		}

		return directory;
	}
}
