# Work log and troubleshooting record

## 16 October 2026 — application implementation

Prepared with Codex assistance for Christom's application/testing contribution. Review before submitting as individual/group work.

### Starting point

Main repository baseline: `904d598d64d1852ea57ba41da408f3c7e78b5544` on `master`. It contained the default .NET 10 Razor Pages page, a runtime Dockerfile copying `build/`, and GitHub Actions publishing images to the team's registries. `ExperimentWithActions` was downloaded for context and was not edited. Work isolated on `codex/shopping-cart`.

### Changes

- Replaced the starter page with a responsive, unofficial Campus Store and local SVG illustrations.
- Added SQLite products/cart/orders, integer-cent totals, quantity limits and atomic checkout.
- Added anonymous cart cookies, receipt ownership checks, duplicate checkout handling and persistent Data Protection keys.
- Added health endpoints, pinned dependencies/lock files, unit/database/HTTP/browser tests and a repeatable Release verification script.
- Updated the existing Actions build to run tests before passing its artifact to the image job; registry publishing is restricted to pushes to master, avoiding image changes during PR review.
- Added setup, architecture, test, deployment, handover, evidence and assessment documentation.

### Problems and resolutions

| Observed problem | Investigation | Resolution / lesson |
| --- | --- | --- |
| GitHub connector returned 404 for private repositories | Local Git authentication could resolve the main repo | Downloaded both through authenticated Git; connector 404 did not mean repos were absent |
| `dotnet` absent from PATH | Existing system installation had only SDKs 6/7 | Installed temporary .NET 10 SDK in `/private/tmp/swe40006-dotnet`; keep normal team setup portable |
| SQLite native package advisory NU1903 on initial restore | Initial .NET SQLite package resolved a vulnerable native bundle | Explicitly updated native bundle to 2.1.14; dependency audit included in verification |
| Razor compile rejected SVG `<text>` attributes | Razor interprets direct `<text>` in code blocks specially | Wrapped each illustration branch in SVG `<g>` markup |
| Two HTTP tests failed to find antiforgery token on an empty cart | Empty-cart view has no forms | Test helper obtains a valid token from the catalogue; app antiforgery protection stays enabled |
| SDK test worker could not open a local socket in sandbox | MSBuild/test process requires local IPC | Ran verification with approved local process access |
| Template `.gitignore` hid the E2E project | Case-insensitive `*.e2e` matched the directory name | Added a directory exception so browser tests are tracked |
| Relative screenshot destination went under test binaries | Test runner uses output directory as cwd | Verification script supplies an absolute evidence directory |

### Verification

See [evidence index](evidence/README.md) for the final recorded run, output files and boundaries. Earlier intermediate run: 26 unit/integration tests passed and one browser journey passed; subsequent changes added a stale-cart regression test and final Release verification supersedes those intermediate counts.

Final Release result: **27 unit/integration tests passed, 1 Chromium acceptance test passed, 0 failed/skipped**. Locked restore, Release publish and published-app browser checks completed. Docker client was present but its daemon was not running, so no container execution is claimed.

Final dependency audits reported no known vulnerable packages across all three projects (including transitive dependencies). C# formatting, Bash syntax, workflow YAML parsing, documentation links and diff whitespace were checked. The formatter initially inherited an older `DOTNET_ROOT`; pointing it at the temporary SDK runtime resolved the issue.

### External actions

No AWS resources or credentials were created, modified or used. Existing Dockerfile, ECR/GHCR destinations and image-publishing job were preserved. Registry/image publishing and production deployment are not claimed as verified by this work.

### Future log entries

Record date/author, change/decision, affected files/resources, verification command and actual result, evidence location, commit/image reference, rollback and outstanding tasks. Record credentials by secret name/location only.

## 16 October 2026 — AWS access restoration and inventory

Christom approved updating the existing SWE40006 server's SSH source to the laptop's current public `/32`. AWS confirmed the update and direct SSH succeeded with the existing local key. Read-only inspection found an existing WordPress/Apache/PHP/MariaDB workload and no Docker installation. No application files, services or database records were changed. See [AWS access review](AWS_ACCESS_REVIEW.md) for resource IDs, procedure, verification, evidence, rollback and remaining deployment decisions. This later entry supersedes the earlier no-AWS-actions statement for the access work only; Campus Store production deployment remains unverified.

## 16 October 2026 — final application handover

Added a short team/presentation guide and a four-page local Word edition. It covers setup, the key code locations, design explanations, a demo script, tutor questions, ownership and pending production checks. The large Word reference is retained. Both Word files stay outside Git, following Christom's earlier request; the short guide's Markdown source is version controlled.

Reconciled the handover and deployment notes with the latest responsibility table: Christom owns development/testing and production functionality checks, assists Judah; Judah leads AWS/monitoring; Amogh leads Docker/staging; Tang Ky leads CI/CD. Documented that the covering teammate's acceptance and the live production check remain pending. No deployment or default-branch merge is claimed.

Repeated full verification: locked restore, 27 unit/integration tests, Release publish and 1 Chromium acceptance test all passed. Evidence is retained in `docs/evidence/handover-2026-10-16/`. Checked documentation links and diff whitespace. No application code changes were needed for this handover. Left the user's untracked IDE `.sln` untouched; documented commands use the existing tracked `.slnx`.

## 16 October 2026 — named handover branch and Word guides

At Christom's request, created `Christoms-Branch` from the completed application branch and included both Word guides in Git. This supersedes the earlier instruction to keep Word files local. Updated the current setup/short guide to use this branch. Included the existing IDE `.sln` alongside the documented `.slnx`. The detailed Word reference is retained as originally prepared; the AWS access review is the current source for infrastructure status. Commit title: Complete Christom application handover with tests and both Word guides.

## 16 October 2026 — local stylesheet delivery fix

Christom reported an unstyled VS Code run. Reproduced on the documented source command: the fingerprinted CSS URL returned 200 with an empty body when the request accepted compression, while an uncompressed request returned the stylesheet. Switched to Static File Middleware serving `wwwroot` and ordinary Razor Pages mapping, retaining the layout's versioned CSS link. Added a computed-style assertion to the existing Chromium journey so visible content alone cannot pass as a styled page. Restart the running app after updating; this changes server startup code. The detailed Word guide predates this fix; `Program.cs` now uses `UseStaticFiles` instead of `MapStaticAssets`/`WithStaticAssets`.

Verification after the fix: 27 application tests and 1 browser journey passed; Release publish succeeded. The actual source-run browser rendered the intended layout, and a compression-capable request returned 10,369 bytes of CSS with content type text/css. See `docs/evidence/stylesheet-fix-2026-10-16/` and `docs/evidence/local-stylesheet-fixed.jpg`.
