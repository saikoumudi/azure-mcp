# Coding Instructions for GitHub Copilot

Always use primary constructors in C#
Always run dotnet build after making a change
Always use System.Text.Json over Newtonsoft

## Engineering System

Structural changes to powershell, c# project files and npm packages can be verified by running `./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx`
  - wait for script completion
  - status messages like `Build completed successfully!` and `Packaging completed successfully!` do not indicate script completion

Don't run local builds to validate pipeline yaml changes. Pipeline YAML files (e.g., files in `eng/pipelines/` with `.yml` extension) should only be validated using:
1. Azure DevOps pipeline validation feature in the web UI
2. YAML linting tools specific to Azure DevOps pipelines

Local build verification won't catch pipeline-specific issues and is an ineffective validation method for pipeline configuration changes.