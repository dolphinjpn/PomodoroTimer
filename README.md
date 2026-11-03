# WinUI AI-Driven Development Template

A minimal starter for building a **WinUI 3** desktop app with **Claude Code**, using a
simple **Issue → PR** workflow.

> This template targets **Claude Code** (`CLAUDE.md` and `.claude/commands/`). The WinUI agent
> plugin itself also works with GitHub Copilot CLI and OpenAI Codex, but the `/kickoff` and
> `/wrapup` commands here are Claude Code slash commands — adapt them for other agents.

## What's inside

| File | Purpose |
|---|---|
| `CLAUDE.md` | Rules the agent follows every session |
| `.claude/commands/kickoff.md` | `/kickoff <issue>` — read the issue, branch, propose a plan |
| `.claude/commands/wrapup.md` | `/wrapup` — commit, push, open a Draft PR (`Closes #`) |
| `.github/ISSUE_TEMPLATE/feature.md` | A feature issue with acceptance criteria |
| `.github/ISSUE_TEMPLATE/bug.md` | A bug report with steps to reproduce and expected behavior |

No CI, auto-review, or release automation is included — this template is intentionally minimal.

## How to use

1. Create a new repository from this template (GitHub "Use this template", or
   `gh repo create <name> --template <owner>/winui-ai-dev-template --public --clone`).
2. Install the WinUI agent plugin for Claude Code (see Microsoft Learn: *AI-assisted Windows development*).
3. Open an Issue with the Feature template and write its **acceptance criteria** (what, not how).
4. Run `/kickoff <issue-number>`, review the plan, and let the agent implement.
5. Run `/wrapup`, then review the Draft PR, run the app once yourself, and **squash-merge**.

The loop is: **Issue → `/kickoff` → implement → `/wrapup` → review → squash merge**.

## Packaging the Pomodoro Timer as MSIX

Requires WinApp CLI 0.7+ (`winget install --id Microsoft.WinAppCli --exact`).

### Build a signed package

```powershell
.\Package.ps1
```

This builds a **Release x64** MSIX with `winapp package` and signs it. The output is
`AppPackages\PomodoroTimer_<version>_x64.msix` (the exact path is printed at the end).

On the first run the script creates a self-signed **development** certificate whose subject
matches the manifest Publisher (`CN=dolphinjpn`):

| File | Content |
|---|---|
| `.certs\devcert.pfx` | Signing certificate (private key) |
| `.certs\devcert.password` | Random password for the `.pfx` |
| `.certs\devcert.cer` | Public certificate, used to trust the package |

`.certs/`, `*.pfx`, `*.cer`, and `AppPackages/` are gitignored — never commit them. To sign with
another certificate, run `.\Package.ps1 -CertPath <file.pfx> -CertPassword <password>`.

### Trust the certificate (one-time, elevated)

Windows only installs MSIX packages whose signing certificate is trusted by the machine. In an
**elevated** PowerShell at the repository root:

```powershell
Import-Certificate -FilePath .\.certs\devcert.cer -CertStoreLocation Cert:\LocalMachine\TrustedPeople
```

Only do this on a development/test machine. If you regenerate the certificate, trust the new one.

### Install

In a normal (non-elevated) PowerShell:

```powershell
Add-AppxPackage -Path .\AppPackages\PomodoroTimer_1.0.0.0_x64.msix
```

The package depends on the Windows App SDK runtime (`Microsoft.WindowsAppRuntime.2`). Launch
**PomodoroTimer** from the Start menu.

The development registration created by `.\BuildAndRun.ps1` uses the same package name, so both
can be installed side by side. The commands below filter on `IsDevelopmentMode` so they only touch
the installed MSIX.

### Undo

```powershell
# Uninstall the installed MSIX only (non-elevated); leaves the BuildAndRun.ps1 registration alone
Get-AppxPackage -Name EE9703F4-AA11-4F56-8BF1-482DCEB2D5C7 |
    Where-Object { -not $_.IsDevelopmentMode } |
    Remove-AppxPackage

# Remove the trusted certificate (elevated)
Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object Subject -eq 'CN=dolphinjpn' | Remove-Item
```

Delete `.certs\` to have `Package.ps1` generate a fresh certificate next time.
