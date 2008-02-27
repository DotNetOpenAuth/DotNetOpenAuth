using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using DotNetOpenId.Provider;
using ProviderPortal;
using DotNetOpenId;

/// <summary>
/// Summary description for Util
/// </summary>
public class Util {
	public static string ExtractUserName(Uri url) {
		return url.Segments[url.Segments.Length - 1];
	}
}
