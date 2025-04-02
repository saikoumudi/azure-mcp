#!/usr/bin/env node

const os = require('os');
const path = require('path');
const fs = require('fs');

let platform = os.platform();
let arch = os.arch();

// get the current platform and architecture
const packageName = `@azure/azure-mcp-${platform}-${arch}`;

function getPackagePath() {
  const basePaths = require.resolve.paths(packageName) || [];
  for (const basePath of basePaths) {
    const packagePath = path.join(basePath, packageName);
    if (fs.existsSync(packagePath)) {
      return packagePath;
    }
  }

  return null;
}

// ensure that one of the optional dependencies is installed
let packagePath = getPackagePath();

if (!packagePath) {
  console.error(`Platform package "${packageName}" is not installed.`);
  process.exit(1); // Exit with an error code
}

console.log(`Using package: ${packagePath}`);

// run package at path passing all args
const args = process.argv.slice(2);
const childProcess = require('child_process');
const execPath = path.join(packagePath, 'bin', 'azmcp');

const child = childProcess.spawn(execPath, args, {
  stdio: 'inherit',
  shell: true,
});

child.on('error', (err) => {
  console.error(`Error executing package: ${err.message}`);
  process.exit(1); // Exit with an error code
});

child.on('exit', (code) => {
  process.exit(code); // Exit with the same code as the child process
});