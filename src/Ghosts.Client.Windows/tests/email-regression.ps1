# Run with Windows PowerShell after building the x64 Windows client.
param([string]$ClientDirectory = "$PSScriptRoot/../bin/x64/Release")
$ErrorActionPreference = 'Stop'
$runner = Join-Path (Resolve-Path $ClientDirectory) ('email-regression-' + [Guid]::NewGuid() + '.exe')
Add-Type -TypeDefinition (Get-Content "$PSScriptRoot/EmailRegression.cs" -Raw) -OutputAssembly $runner -OutputType ConsoleApplication
& $runner
if ($LASTEXITCODE -ne 0) { throw 'Email regression checks failed' }
