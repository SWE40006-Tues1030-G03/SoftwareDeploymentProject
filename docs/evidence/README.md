# Local verification evidence

Captured 16 October 2026 (Australia/Melbourne), on macOS arm64 using .NET SDK 10.0.402, runtime 10.0.13 and Playwright Chromium. These are real local application results, not AWS or GitHub runner results.

## Final Release run

Command from repository root:

```sh
DOTNET_BIN=/private/tmp/swe40006-dotnet/dotnet bash scripts/verify.sh --browser
```

Result: **27 unit/integration tests passed, 1 browser acceptance test passed, zero failures or skips.** Locked dependency restores and Release publication succeeded. The published app ran on loopback port 5087 against a disposable SQLite database. The script stopped the server and deleted that database on completion. The temporary SDK path is specific to the development session; teammates normally use `dotnet` on PATH.

| Evidence | Caption / purpose |
| --- | --- |
| [Desktop catalogue](01-catalogue-desktop.png) | Six database-backed products, category filters, prices and desktop layout |
| [Cart](02-cart.png) | Browser changed hoodie quantity to two; server calculated $130.00 |
| [Receipt](03-confirmation.png) | Saved simulated order, reloaded successfully in the same browser |
| [Mobile catalogue](04-catalogue-mobile.png) | 390px viewport; browser checked no horizontal overflow |
| [Raw verification output](release-verification.log) | Restore, Release build/test, publish and browser test output |
| [Published-server log](published-test-server.log) | Actual published application startup and test-request logs |
| [Unit/integration TRX](application-tests.trx) | Machine-readable 27-test results |
| [Acceptance TRX](acceptance-tests.trx) | Machine-readable browser result |

Absolute development-machine paths and generated demo order references may appear in these local logs. No production credentials were used. Receipt references in screenshots are synthetic, and their disposable test database was removed.

Final dependency audit: application, unit/integration and browser projects reported no known vulnerable packages from the configured NuGet source, including transitive dependencies. Raw output: [application audit](dependency-audit.log), [browser audit](browser-dependency-audit.log). This is a point-in-time advisory check.

Additional checks: C# whitespace formatting completed; `git diff --check` passed; Bash syntax and workflow YAML parsed; local documentation links resolved.

## Boundaries

These results prove local application behaviour and the published-app verification script. They do not prove Docker execution, Linux runner compatibility, private registry publishing, distinct group test/staging servers, AWS deployment, public HTTPS, live persistence or production monitoring. Docker's daemon was stopped on the development machine. The team must capture those actual results separately.

Screenshots are selected evidence, not a full accessibility audit. Preserve this record when adding later runs; use dated subdirectories for additional production/CI evidence and include the relevant commit SHA/image digest.
