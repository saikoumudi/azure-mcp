using Azure.Core;

namespace AzureMCP.Models;

public static class ArgumentDefinitions
{
    public static class Common
    {
        public const string TenantIdName = "tenant-id";
        public const string SubscriptionIdName = "subscription-id";
        public const string ResourceGroupName = "resource-group";
        public const string AuthMethodName = "auth-method";

        public static readonly ArgumentDefinition<string> TenantId = new(
            TenantIdName,
            "The Azure Active Directory (tenant) ID. This is a unique identifier for your Azure AD instance.",
            required: false
        );

        public static readonly ArgumentDefinition<string> SubscriptionId = new(
            SubscriptionIdName,
            "The Azure subscription ID. This is the GUID identifier for the Azure subscription to use.",
            required: true
        );

        public static readonly ArgumentDefinition<AuthMethod> AuthMethod = new(
            AuthMethodName,
            "Authentication method to use. Options: 'credential' (Azure CLI/managed identity), 'key' (access key), or 'connectionString'.",
            defaultValue: Models.AuthMethod.Credential,
            required: false
        );

        public static readonly ArgumentDefinition<string> ResourceGroup = new(
            ResourceGroupName,
            "The name of the Azure resource group. This is a logical container for Azure resources.",
            required: true
        );
    }

    public static class RetryPolicy
    {
        public const string DelayName = "retry-delay";
        public const string MaxDelayName = "retry-max-delay";
        public const string MaxRetriesName = "retry-max-retries";
        public const string ModeName = "retry-mode";
        public const string NetworkTimeoutName = "retry-network-timeout";

        public static readonly ArgumentDefinition<double> Delay = new(
            DelayName,
            "Initial delay in seconds between retry attempts. For exponential backoff, this value is used as the base.",
            defaultValue: 2.0,
            required: false
        );

        public static readonly ArgumentDefinition<double> MaxDelay = new(
            MaxDelayName,
            "Maximum delay in seconds between retries, regardless of the retry strategy.",
            defaultValue: 10.0,
            required: false
        );

        public static readonly ArgumentDefinition<int> MaxRetries = new(
            MaxRetriesName,
            "Maximum number of retry attempts for failed operations before giving up.",
            defaultValue: 3,
            required: false
        );

        public static readonly ArgumentDefinition<RetryMode> Mode = new(
            ModeName,
            "Retry strategy to use. 'fixed' uses consistent delays, 'exponential' increases delay between attempts.",
            defaultValue: Azure.Core.RetryMode.Exponential,
            required: false
        );

        public static readonly ArgumentDefinition<double> NetworkTimeout = new(
            NetworkTimeoutName,
            "Network operation timeout in seconds. Operations taking longer than this will be cancelled.",
            defaultValue: 100.0,
            required: false
        );
    }

    public static class Storage
    {
        public const string AccountName = "account-name";
        public const string ContainerName = "container-name";
        public const string TableName = "table-name";

        public static readonly ArgumentDefinition<string> Account = new(
            AccountName,
            "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
            required: true
            
        );

        public static readonly ArgumentDefinition<string> Container = new(
            ContainerName,
            "The name of the container to access within the storage account.",
            required: true
        );

        public static readonly ArgumentDefinition<string> Table = new(
            TableName,
            "The name of the table to access within the storage account.",
            required: true
        );
    }

    public static class Cosmos
    {
        public const string AccountName = "account-name";
        public const string DatabaseName = "database-name";
        public const string ContainerName = "container-name";
        public const string QueryText = "query";

        public static readonly ArgumentDefinition<string> Account = new(
            AccountName,
            "The name of the Cosmos DB account to query (e.g., my-cosmos-account).",
            required: true
        );

        public static readonly ArgumentDefinition<string> Database = new(
            DatabaseName,
            "The name of the database to query (e.g., my-database).",
            required: true
        );

        public static readonly ArgumentDefinition<string> Container = new(
            ContainerName,
            "The name of the container to query (e.g., my-container).",
            required: true      
        );

        public static readonly ArgumentDefinition<string> Query = new(
            QueryText,
            "SQL query to execute against the container. Uses Cosmos DB SQL syntax.",
            defaultValue: "SELECT * FROM c",
            required: false
        );
    }

    public static class Monitor
    {
        public const string WorkspaceIdName = "workspace-id";
        public const string WorkspaceNameName = "workspace-name";
        public const string TableTypeName = "table-type";
        public const string QueryTextName = "query";
        public const string HoursName = "hours";
        public const string LimitName = "limit";

        public static readonly ArgumentDefinition<string> WorkspaceId = new(
            WorkspaceIdName,
            "The Log Analytics workspace ID to query. This is the unique identifier for your workspace.",
            required: true
        );

        public static readonly ArgumentDefinition<string> WorkspaceName = new(
            WorkspaceNameName,
            "The name of the Log Analytics workspace to query.",
            required: true
        );

        public static readonly ArgumentDefinition<string> TableType = new(
            TableTypeName,
            "The type of table to query. Options: 'CustomLog', 'AzureMetrics', etc.",
            defaultValue: "CustomLog",
            required: true
        );

        public static readonly ArgumentDefinition<string> Query = new(
            QueryTextName,
            "The KQL query to execute against the Log Analytics workspace.",
            required: true
        );

        public static readonly ArgumentDefinition<int> Hours = new(
            HoursName,
            "The number of hours to query back from now.",
            defaultValue: 24,
            required: true
        );

        public static readonly ArgumentDefinition<int> Limit = new(
            LimitName,
            "The maximum number of results to return.",
            defaultValue: 20,
            required: true
        );
    }
}