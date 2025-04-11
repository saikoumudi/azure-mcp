// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Extensions;

using AzureMcp.Models.Argument;
using System;
using System.Collections.Generic;

public static class ArgumentExtensions
{
    public static void SortArguments(this List<ArgumentInfo> arguments)
    {
        arguments.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    }
}