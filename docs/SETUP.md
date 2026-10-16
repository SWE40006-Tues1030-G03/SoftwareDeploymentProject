# Setup and day-to-day operations

## Prerequisites and commands

Use a .NET 10 SDK. `global.json` permits installed .NET 10 feature bands, while the project targets `net10.0`. The original runtime Docker image is `mcr.microsoft.com/dotnet/aspnet:10.0`; keep the runtime and application major versions aligned.

```sh
git clone https://github.com/SWE40006-Tues1030-G03/SoftwareDeploymentProject.git
cd SoftwareDeploymentProject
# If reviewing Christom's work before merge:
git switch codex/shopping-cart
dotnet --info
dotnet restore SoftwareDeploymentProject.slnx --locked-mode
dotnet run --project SoftwareDeploymentProject --no-launch-profile --urls http://localhost:5080
```

Private repository access must be granted through the organisation. The `.slnx` solution needs a modern SDK; using .NET 6 or 7 will not build this project. `--no-launch-profile` avoids IDE-specific ports or HTTPS certificate requirements. No frontend package installation is required. Product illustrations are local SVG markup, with no external asset service.

## Configuration

ASP.NET configuration reads `appsettings.json`, environment-specific JSON and environment variables. Use a double underscore for nested environment variables.

| Variable | Default | Meaning |
| --- | --- | --- |
| `Shop__DatabasePath` | `App_Data/shop.db`, relative to app content root | SQLite database; use an absolute persistent path in containers |
| `Shop__SecureCookies` | `false` | Set `true` for public HTTPS deployments behind a proxy |
| `ASPNETCORE_ENVIRONMENT` | `Production` without a launch profile | `Development` for diagnostics locally; `Testing` for test servers; `Production` on AWS |
| `ASPNETCORE_URLS` | Host-dependent; pass `--urls` explicitly locally | Listening addresses; use `http://+:8080` in the container |
| `DOTNET_BIN` | `dotnet` | Optional SDK executable override for `scripts/verify.sh` only |
| `SHOP_TEST_PORT` | `5087` | Verification script's loopback test-server port |
| `SHOP_E2E_URL` | Required for direct browser-test execution | Isolated server under test; set automatically by verification script |
| `SHOP_EVIDENCE_DIR` | `TestResults/browser`, relative to test process | Browser evidence destination; verification script sets an absolute path |

Example on macOS/Linux:

```sh
ASPNETCORE_ENVIRONMENT=Development Shop__DatabasePath=/tmp/campus-local/shop.db dotnet run --project SoftwareDeploymentProject --no-launch-profile --urls http://localhost:5080
```

PowerShell equivalent:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Shop__DatabasePath = "$env:TEMP/campus-local/shop.db"
dotnet run --project SoftwareDeploymentProject --no-launch-profile --urls http://localhost:5080
```

The app creates the database parent directory and a `keys/` subdirectory for ASP.NET Data Protection keys. These keys protect antiforgery/TempData cookies and should persist with the deployment. Keep this directory private and outside `wwwroot`. The app needs read/write access. Keys are stored on disk without application-level encryption; host access controls protect them.

## Data lifecycle

Startup runs `CREATE TABLE IF NOT EXISTS` and `INSERT OR IGNORE` to seed products. Restarting does not delete orders. Editing the seed SQL does not update existing product rows. For a disposable development database, stop the app and move the entire `App_Data` directory to a backup location, then restart to create fresh data. Never perform that reset on production to apply a code change.

For a backup, stop the only app instance cleanly, then copy the database directory including `shop.db`, any WAL/SHM files and `keys/` to a protected backup directory. Restart the app. Restore only while stopped, using a consistent directory backup and matching application/schema version. For backups without downtime use SQLite's backup API through a separately reviewed tool; copying just an active `.db` file is unsafe in WAL mode.

No automatic expiration job is implemented. Cart cookies expire after seven days; database rows remain until deliberately cleaned up. Use synthetic demo data only.

## Troubleshooting

| Symptom | Check and recovery |
| --- | --- |
| `NETSDK1045` / unsupported `net10.0` | Run `dotnet --info`; install/select .NET 10 SDK |
| NuGet restore fails | Check network/private proxy access to nuget.org; do not delete lock files to bypass a mismatch |
| Locked-mode restore detects changes | Restore without `--locked-mode` after an intentional package edit, review and commit lock-file changes |
| Address already in use | Stop the previous process or choose another `--urls` / `SHOP_TEST_PORT` |
| Unable to open database | Check absolute path, directory write permission and disk space |
| Database locked | Confirm only the intended single app instance uses this file; inspect long-running requests and logs |
| POST returns 400 | Reload the page for a fresh antiforgery token; check cookies and persistent Data Protection keys |
| Browser test cannot find executable | Run the documented Playwright browser installation; Linux needs system dependencies |
| Images/styles missing in published app | Run publish output with its content root; preserve all publish files, not just the DLL |
| Receipt returns 404 | Use the same browser cookie; starting another cart or clearing cookies removes access to the old receipt |
| Cart resets after container replacement | Correct the `/data` volume mount and `Shop__DatabasePath`; see deployment guide |

Do not paste AWS keys, SSH keys, cookies or connection secrets into reports, screenshots or Git. Record configuration variable names and credential-storage locations, not their values.
