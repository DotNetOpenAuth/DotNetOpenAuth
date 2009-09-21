// Copyright (c) Microsoft Corporation.  All rights reserved.
//

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
