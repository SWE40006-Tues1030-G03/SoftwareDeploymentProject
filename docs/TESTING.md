# Testing and repeatable verification

## Test layers

| Layer | Location | What it proves |
| --- | --- | --- |
| Unit | `SoftwareDeploymentProject.Tests/ShopRulesTests.cs` | Quantity boundaries, total arithmetic and AUD formatting |
| Database integration | `SoftwareDeploymentProject.Tests/StoreTests.cs` | Real SQLite schema, cart isolation, updates/removal, persistence, duplicate and concurrent checkouts, stale writes |
| HTTP integration | `SoftwareDeploymentProject.Tests/WebTests.cs` | Real Razor routing/forms, cookies, antiforgery rejection, invalid input, server-owned prices and receipt isolation |
| Browser acceptance | `SoftwareDeploymentProject.E2E/ShoppingTests.cs` | Real Chromium navigation, add/update/remove, checkout/receipt reload, category filtering and mobile overflow check |

Integration tests use temporary SQLite files, not the EF in-memory provider or mocked persistence. Every HTTP factory and store test uses its own directory. Browser tests create a fresh browser context and the verification script launches a published app against a separate disposable database. Production must never be used for acceptance runs: these tests deliberately create orders.

The E2E project is separate from the solution so `dotnet test SoftwareDeploymentProject.slnx` works without browser setup. `scripts/verify.sh --browser` explicitly runs it. This is not a skipped test: missing browser or server prerequisites cause the explicit acceptance command to fail.

## Recommended full check

From repository root in Bash:

```sh
dotnet restore SoftwareDeploymentProject.E2E --locked-mode
dotnet run --project SoftwareDeploymentProject.E2E --no-restore -- install chromium
bash scripts/verify.sh --browser
```

On Linux, provision browser system dependencies with `install --with-deps chromium` (may require elevated OS package permissions). CI performs this setup on the GitHub-hosted runner.

`verify.sh` restores locked dependencies, runs the solution tests in Release, publishes the app to `publish/`, starts the published app on loopback, waits for database readiness, runs Chromium tests and stops the test server even if a test fails. A temporary database is deleted on exit. Set `SHOP_TEST_PORT` if 5087 is occupied. `curl` is needed for the readiness wait. Paths are quoted to support spaces.

```sh
# SDK outside PATH, as on the initial development machine:
DOTNET_BIN=/path/to/dotnet bash scripts/verify.sh --browser
```

## Individual commands / Windows

```sh
dotnet restore SoftwareDeploymentProject.slnx --locked-mode
dotnet test SoftwareDeploymentProject.slnx -c Release --no-restore --logger "trx;LogFileName=application-tests.trx" --results-directory .artifacts/tests
dotnet publish SoftwareDeploymentProject/SoftwareDeploymentProject.csproj -c Release --no-restore -o publish
```

For a direct E2E run, start the app in a separate terminal with a disposable database and port, then:

```sh
SHOP_E2E_URL=http://localhost:5080 SHOP_EVIDENCE_DIR=/absolute/path/to/evidence dotnet test SoftwareDeploymentProject.E2E -c Release
```

PowerShell uses `$env:SHOP_E2E_URL='http://localhost:5080'` and `$env:SHOP_EVIDENCE_DIR='C:/path/to/evidence'` on separate lines before `dotnet test`. Stop the server manually after a direct run. `SHOP_EVIDENCE_DIR` should be absolute because the test runner's working directory is its binary output directory.

## Outputs and evidence

- `.artifacts/tests/application-tests.trx`: machine-readable unit/integration results.
- `.artifacts/tests/acceptance-tests.trx`: browser acceptance results.
- `.artifacts/browser/*.png`: desktop catalogue, cart, order confirmation and mobile catalogue.
- `.artifacts/test-server.log`: published test server startup/request log.
- `publish/`: build output consumed by the original Dockerfile after artifact download into `build/`.

Generated outputs are ignored by Git. Selected, reviewed local results and screenshots are retained in `docs/evidence/` with captions. CI uploads `.artifacts/` as `test-evidence`, including results on failure where available.

## Pipeline integration

The existing `.github/workflows/dotnet.yml` still targets pushes and pull requests to `master`. Its `build` job installs Chromium, runs full verification, uploads evidence and then uploads the `publish/` build artifact. The existing image job has `needs: build`; a failed test prevents that job from running. No production deployment step was added by the application work.

**Team action:** run and review this workflow in GitHub, add isolated test/staging deployment as required by the brief, and wire production deployment after successful verification. Local Linux-container execution, GitHub runner execution, AWS credentials, registry pushes and deployment have not been proven by a passing macOS test run.

## Additional checks before public release

Check keyboard navigation, screen-reader labels, colour contrast and actual mobile devices. Automated viewport/overflow checks are useful but are not a complete accessibility audit. Test database persistence across container replacement, TLS/secure cookies and production health checks. Record actual results rather than assuming local results prove live deployment.

Registry publishing is restricted to pushes to `master`; pull requests run verification without publishing images.
