// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;
using System.Threading;
using AzureMcp.Services.Azure;
using Newtonsoft.Json.Linq;

namespace AzureMcp.Areas.TerraformSchema.Services;


public class TerraformSchemaService() : BaseAzureService, ITerraformSchemaService
{
    public string GetResourceSchema(string resourceTypeName, string providerName, string workspacePath)
    {
        string command = "terraform providers schema -json";
        string fullSchemaJson = RunCommand("cmd.exe", "/c " + command, workspacePath);

        using var doc = JsonDocument.Parse(fullSchemaJson);

        if (doc.RootElement.TryGetProperty("provider_schemas", out var providerSchemas) &&
            providerSchemas.TryGetProperty($"registry.terraform.io/hashicorp/{providerName}", out var provider) &&
            provider.TryGetProperty("resource_schemas", out var resourceSchemas) &&
            resourceSchemas.TryGetProperty(resourceTypeName, out var resourceSchema))
        {
            return resourceSchema.ToString();
        }
        return $"Resource '{resourceTypeName}' not found in the schema.";

    }



    static string RunCommand(string fileName, string arguments, string workingDirectory)
    {
        // try
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    outputBuilder.AppendLine(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    errorBuilder.AppendLine(args.Data);
            };

            try
            {
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!process.WaitForExit(100000))
                {
                    process.Kill(true);
                    throw new TimeoutException("Terraform schema command timed out.");
                }

                // Ensure all output events have been processed
                process.WaitForExit(); // this second call ensures async buffers flush

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Terraform exited with code {process.ExitCode}");
                    Console.WriteLine($"stderr: {errorBuilder}");
                    return string.Empty;
                }

                return outputBuilder.ToString();
                /*
                process.Start();
                // Read output and error asynchronously to avoid deadlocks
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                // Give it up to 2 seconds to exit gracefully after output is read
                if (!process.WaitForExit(30000))
                {
                    Console.WriteLine("Warning: process did not exit within timeout. Killing it.");
                    process.Kill(true);
                }

                string output = outputTask.Result;
                string error = errorTask.Result;

                // Make sure output has completed
                Task.WaitAll(outputTask, errorTask);

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Terraform command failed with exit code {process.ExitCode}");
                    Console.WriteLine($"stderr: {error}");
                    return string.Empty;
                }
                return output;
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return string.Empty;
            }
        }
    }
}

