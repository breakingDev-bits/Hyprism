# Copyright (C) 2026 HyPrism Launcher
# SPDX-License-Identifier: GPL-3.0-only

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ResultsDirectory,

    [Parameter(Mandatory = $true)]
    [string] $Title
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-CountAttribute {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement] $Node,

        [Parameter(Mandatory = $true)]
        [string] $Name
    )

    $value = $Node.GetAttribute($Name)
    if ($value -match '^\d+$') {
        return [int] $value
    }

    return 0
}

function Get-NodeText {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlNode] $Node,

        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $child = $Node.SelectSingleNode($Path)
    if ($null -eq $child) {
        return $null
    }

    $text = $child.InnerText.Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    return $text
}

$summaryPath = $env:GITHUB_STEP_SUMMARY
if ([string]::IsNullOrWhiteSpace($summaryPath)) {
    $summaryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("hyprism-test-summary-" + [Guid]::NewGuid() + '.md')
}

$trxFiles = @(
    Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -File -Recurse -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTimeUtc
)
$testResults = @()
$counterTotal = 0
$counterPassed = 0
$counterSkipped = 0
$counterErrors = 0
$hasCounters = $false
$parseErrors = @()

foreach ($trxFile in $trxFiles) {
    try {
        $document = [System.Xml.XmlDocument]::new()
        $document.Load($trxFile.FullName)

        $testResults += @($document.SelectNodes("//*[local-name()='UnitTestResult']"))

        $counter = @($document.SelectNodes("//*[local-name()='Counters']")) | Select-Object -Last 1
        if ($counter -is [System.Xml.XmlElement]) {
            $hasCounters = $true
            $counterTotal += Get-CountAttribute $counter 'total'
            $counterPassed += Get-CountAttribute $counter 'passed'
            $counterSkipped += Get-CountAttribute $counter 'notExecuted'
            $counterSkipped += Get-CountAttribute $counter 'notRunnable'
            $counterErrors += Get-CountAttribute $counter 'failed'
            $counterErrors += Get-CountAttribute $counter 'error'
            $counterErrors += Get-CountAttribute $counter 'timeout'
            $counterErrors += Get-CountAttribute $counter 'aborted'
            $counterErrors += Get-CountAttribute $counter 'inconclusive'
            $counterErrors += Get-CountAttribute $counter 'disconnected'
        }
    }
    catch {
        $parseErrors += "$($trxFile.Name): $($_.Exception.Message)"
    }
}

$unclassifiedCounterResults = $counterTotal - $counterPassed - $counterSkipped - $counterErrors
if ($unclassifiedCounterResults -gt 0) {
    $counterSkipped += $unclassifiedCounterResults
}

if ($hasCounters) {
    $total = $counterTotal
    $passed = $counterPassed
    $skipped = $counterSkipped
    $errors = $counterErrors
}
else {
    $passed = @($testResults | Where-Object { $_.GetAttribute('outcome') -eq 'Passed' }).Count
    $skipped = @(
        $testResults |
            Where-Object { $_.GetAttribute('outcome') -in @('Skipped', 'NotExecuted') }
    ).Count
    $errors = @(
        $testResults |
            Where-Object { $_.GetAttribute('outcome') -in @('Failed', 'Error', 'Timeout', 'Aborted', 'Inconclusive') }
    ).Count
    $total = $testResults.Count
}

$failureResults = @(
    $testResults |
        Where-Object { $_.GetAttribute('outcome') -in @('Failed', 'Error', 'Timeout', 'Aborted', 'Inconclusive') }
)
$lastFailure = $failureResults | Select-Object -Last 1

$lines = [System.Collections.Generic.List[string]]::new()
[void] $lines.Add("### $Title")
[void] $lines.Add('')

if ($trxFiles.Count -eq 0) {
    [void] $lines.Add('No TRX result file was produced')
}
else {
    [void] $lines.Add('| Total | Passed | Skipped | Errors |')
    [void] $lines.Add('| ---: | ---: | ---: | ---: |')
    [void] $lines.Add("| $total | $passed | $skipped | $errors |")
    [void] $lines.Add('')
    [void] $lines.Add("Result files: $($trxFiles.Name -join ', ')")
}

if ($parseErrors.Count -gt 0) {
    [void] $lines.Add('')
    [void] $lines.Add('#### Result parsing warnings')
    foreach ($parseError in $parseErrors) {
        [void] $lines.Add("- $parseError")
    }
}

if ($null -ne $lastFailure) {
    $errorInfoPath = "./*[local-name()='Output']/*[local-name()='ErrorInfo']"
    $message = Get-NodeText $lastFailure "$errorInfoPath/*[local-name()='Message']"
    $stackTrace = Get-NodeText $lastFailure "$errorInfoPath/*[local-name()='StackTrace']"
    $standardError = Get-NodeText $lastFailure "./*[local-name()='Output']/*[local-name()='StdErr']"
    $details = @($message, $stackTrace, $standardError) |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -Unique

    if ($details.Count -eq 0) {
        $details = @(Get-NodeText $lastFailure "./*[local-name()='Output']/*[local-name()='StdOut']")
    }
    if ($details.Count -eq 0) {
        $details = @($lastFailure.InnerText.Trim())
    }

    $failureText = ($details -join [Environment]::NewLine).Trim()
    if ([string]::IsNullOrWhiteSpace($failureText)) {
        $failureText = 'The test runner did not include failure details in the TRX file'
    }
    if ($failureText.Length -gt 16000) {
        $failureText = $failureText.Substring(0, 16000) + [Environment]::NewLine + '[output truncated]'
    }

    [void] $lines.Add('')
    [void] $lines.Add('#### Last failure')
    [void] $lines.Add("Test: $($lastFailure.GetAttribute('testName'))")
    [void] $lines.Add('')
    [void] $lines.Add('````text')
    [void] $lines.Add($failureText)
    [void] $lines.Add('````')
}

$summary = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Add-Content -LiteralPath $summaryPath -Value $summary -Encoding utf8
Write-Output "Test summary written to $summaryPath"
