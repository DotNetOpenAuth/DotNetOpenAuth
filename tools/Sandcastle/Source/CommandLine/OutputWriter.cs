// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;

public static class OutputWriter {

    private static int warningCount;

    public static int WarningCount {
        get {
            return (warningCount);
        }
    }

    public static void WriteWarning(string text) {
        warningCount++;
        Console.WriteLine(text);
    }

    public static void WriteWarning(string format, params Object[] values) {
        warningCount++;
        Console.WriteLine(format, values);
    }
}
