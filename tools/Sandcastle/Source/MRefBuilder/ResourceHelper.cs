// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System.IO;
using System.Reflection;
using System.Resources;

namespace Microsoft.Ddue.Tools.DxCoach {

    internal sealed class ResourceHelper {

        private static ResourceManager manager = new ResourceManager("TextStrings", Assembly.GetExecutingAssembly());

        private ResourceHelper() { }

        public static Stream GetStream(string file) {
            return (Assembly.GetExecutingAssembly().GetManifestResourceStream(file));
        }

        public static string GetString(string key) {
            return (manager.GetString(key));
        }

    }

}
