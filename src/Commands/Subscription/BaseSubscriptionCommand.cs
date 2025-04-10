// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Arguments.Subscription;

namespace AzureMCP.Commands.Subscription;

public abstract class BaseSubscriptionCommand<TArgs> : GlobalCommand<TArgs>
    where TArgs : BaseSubscriptionArguments, new()
{
    protected BaseSubscriptionCommand()
    {
    }
}