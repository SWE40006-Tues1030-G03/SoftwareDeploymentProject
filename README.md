# Campus Store — SWE40006

An unofficial Swinburne-themed shopping cart, built on the team's existing **C# / ASP.NET Core Razor Pages / .NET 10** project. It demonstrates a working database-backed application for the deployment pipeline. Checkout is simulated: no payment, personal data, shipping or university integration.

## Start here

1. Install the **.NET 10 SDK** (not just the runtime), Git and a browser. No Node.js, npm, AWS account or local Docker is required to develop the app.
2. Clone this repository and open a terminal in the directory containing `SoftwareDeploymentProject.slnx`.
3. Run:

```sh
dotnet --version
dotnet restore SoftwareDeploymentProject.slnx --locked-mode
dotnet run --project SoftwareDeploymentProject --no-launch-profile --urls http://localhost:5080
```

Open **http://localhost:5080**. Stop with Ctrl+C. Startup creates and seeds `SoftwareDeploymentProject/App_Data/shop.db`. Data survives app restarts. The six products are demo data, not an official university catalogue.

If `dotnet` is not found, install the SDK or add its installation folder to PATH. On this development Mac the pre-existing system SDKs were too old; verification used a temporary .NET 10 SDK. Do not copy the temporary path into the team's pipeline. See [setup](docs/SETUP.md).

## Verify the work

```sh
# Unit + database + HTTP integration tests and a Release publish:
bash scripts/verify.sh

# One-time browser preparation (Linux CI also needs --with-deps):
dotnet restore SoftwareDeploymentProject.E2E --locked-mode
dotnet run --project SoftwareDeploymentProject.E2E --no-restore -- install chromium

# All of the above plus a real Chromium shopping journey on an isolated test server:
bash scripts/verify.sh --browser
```

The browser command starts and stops its own published app on port 5087 and deletes its temporary database. It produces screenshots and logs under `.artifacts/`. It does not contact AWS. On Windows use Git Bash for the shell script or follow the individual commands in [testing](docs/TESTING.md).

## What works

- Catalogue with six products and category filters.
- Anonymous database-backed cart; add, update (1–20 per product), remove and totals in AUD.
- Server-owned prices and integer-cent calculations.
- Atomic checkout, immutable order-item snapshots and duplicate-submission protection.
- Receipt restricted to the cart's browser cookie; a new purchase starts a fresh cart.
- Responsive layouts, labelled controls, keyboard focus and a skip link.
- `/health` for liveness and `/health/ready` for database readiness.

## Handover map

| Read | Purpose |
| --- | --- |
| [Short team and presentation guide](docs/QUICK_GUIDE.md) | Essential explanation, demo, tutor questions and next owners |
| [Handover](docs/HANDOVER.md) | Current status, team responsibilities and next actions |
| [Setup](docs/SETUP.md) | Run, configure, restart, back up and troubleshoot |
| [Architecture](docs/ARCHITECTURE.md) | Code walkthrough, schema, decisions and limits |
| [Testing](docs/TESTING.md) | Test types, commands, expected results and CI integration |
| [Deployment](docs/DEPLOYMENT.md) | Docker/AWS integration contract, persistence and verification |
| [Assessment and demo](docs/ASSESSMENT.md) | Requirements mapping, evidence and presentation walkthrough |
| [Work log](docs/WORK_LOG.md) | Changes, problems encountered and verification record |
| [Evidence](docs/evidence/README.md) | Captured local screenshots and test logs |
| [AWS access review](docs/AWS_ACCESS_REVIEW.md) | Verified server access, existing WordPress workload and change rollback |

The existing image-publishing workflow now runs application tests and browser acceptance checks before its build artifact is uploaded. **A production deployment, production monitoring and isolated team test/staging infrastructure still need to be completed by the group.** Local tests do not prove those requirements are complete.

Registry publishing is restricted to pushes to `master`; pull requests run verification without publishing images.
