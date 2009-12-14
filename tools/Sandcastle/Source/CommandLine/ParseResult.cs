// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

namespace Microsoft.Ddue.Tools.CommandLine
{

    internal enum ParseResult {
        Success,
        ArgumentNotAllowed,
        MalformedArgument,
        MissingOption,
        UnrecognizedOption,
        MultipleOccurance
    }

}
