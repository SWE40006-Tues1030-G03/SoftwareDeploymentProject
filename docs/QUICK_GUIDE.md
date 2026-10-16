# Campus Store quick handover and presentation guide

Christom | SWE40006 Group 03 | 16 October 2026

This guide gives the team enough detail to run the app, demonstrate it and explain the main decisions to the tutor. Use the larger code reference when you need individual files and variables explained.

## What is ready

Campus Store is an unofficial university themed shopping demo. A shopper can browse six products, filter categories, add items, change quantities, remove items and place a simulated order. The receipt is saved in the database. There are no real payments, shipping or university services.

The app and local tests are complete. The latest handover run passed 27 unit/integration tests and one Chromium acceptance test, with no failures or skips. Release publication also succeeded. AWS deployment, production monitoring and production functionality checks are still pending.

The code is on `codex/shopping-cart` in the team's SoftwareDeploymentProject repository. The default branch is called `master`. Review and merge the work through the team's normal process; pushing a work branch does not put the app on the live server.

## Who does what next

- Christom owns application development and tests, checks production functionality, and assists Judah with AWS. Local development and testing are ready for handover.
- Judah leads EC2 setup, application deployment and CloudWatch monitoring. He needs to confirm the deployment target with the team.
- Amogh leads Docker and the testing/staging environments, supported by Tang Ky.
- Tang Ky leads GitHub Actions and deployment automation. Christom's changes add application verification before image publishing.

Before Sunday, a covering teammate should run the app and accept the handover. If deployment happens after Christom leaves, name someone to perform the production checks on page four. That acceptance and production result have not yet been recorded.

## A short explanation for the presentation

“I developed the shopping application and its automated tests using the team's existing .NET project. Products, carts and demo orders are stored in SQLite. The tests check the business rules, the database and HTTP requests, and a complete browser shopping journey. The app gives our pipeline something meaningful to build, test and deploy. The infrastructure owners will demonstrate the actual deployment and monitoring results.”

Use this as speaking notes and adjust the final sentence when the team has real deployment evidence. Review the implementation so you can explain it in your own words. The work was prepared with Codex assistance; follow the unit's AI disclosure requirements.

<!-- page break -->

## Run the app on another laptop

Install Git and the .NET 10 SDK. Use a terminal in the repository root, beside `SoftwareDeploymentProject.slnx`. A runtime alone or an older SDK will not build it. No npm commands are needed.

```sh
git clone https://github.com/SWE40006-Tues1030-G03/SoftwareDeploymentProject.git
cd SoftwareDeploymentProject
git switch codex/shopping-cart
dotnet restore SoftwareDeploymentProject.slnx --locked-mode
dotnet run --project SoftwareDeploymentProject --no-launch-profile --urls http://localhost:5080
```

Open http://localhost:5080. Stop with Ctrl+C. The first start creates the demo database at `SoftwareDeploymentProject/App_Data/shop.db`. Restarting the app keeps its data. Organisation access is required to clone this private repository.

## Run the checks

From the same root directory, use Bash or Git Bash on Windows:

```sh
bash scripts/verify.sh
dotnet restore SoftwareDeploymentProject.E2E --locked-mode
dotnet run --project SoftwareDeploymentProject.E2E --no-restore -- install chromium
bash scripts/verify.sh --browser
```

The first command runs the application tests and publishes a Release build. The middle two prepare Chromium once. The final command repeats verification and runs the browser journey on its own temporary server and database. Linux CI installs Chromium with `--with-deps`. Results go into `.artifacts/`; reviewed evidence is kept in `docs/evidence/`.

## The few files worth knowing

- `Program.cs` starts the app, connects its services and maps the health checks.
- `Pages/` holds the screens and their C# request handlers. `Index` is the catalogue, `Cart` handles the basket and `Confirmation` displays the receipt.
- `Services/ShopStore.cs` contains database operations and checkout. `Services/CartIdentity.cs` connects a browser's cookie to its cart.
- `Models/` defines the product, cart and order data. `wwwroot/` contains browser assets such as styles.
- `SoftwareDeploymentProject.Tests/` contains unit, SQLite and HTTP tests. `SoftwareDeploymentProject.E2E/` contains the Chromium journey.
- `scripts/verify.sh` runs verification. `.github/workflows/dotnet.yml` uses it in CI. `docs/` contains the detailed handover and evidence.

The first four paths are inside the application folder `SoftwareDeploymentProject/`. Treat `bin/`, `obj/`, `publish/` and `.artifacts/` as generated output. Use the committed `.slnx` for these commands; a local IDE generated `.sln` is not the documented entry point.

<!-- page break -->

## How the application works

A browser submits a form to a Razor Page. Its C# handler validates the request and calls ShopStore, which reads or writes SQLite. The server renders the next HTML page. This keeps the frontend and backend in the existing .NET project.

A random cookie identifies an anonymous cart. Cart contents live in SQLite; the cookie holds the identifier. Prices come from the server, and money is stored as integer cents to avoid rounding errors. Quantities are limited to 1 through 20 per product.

Checkout saves the order and its item details in one database transaction, so the operation succeeds together or is rolled back. Submitting checkout again returns the same order. Receipts are checked against the browser's cart identifier. Starting a new cart or losing the cookie removes access to the previous receipt.

## Demonstrate the app in about three minutes

1. Open the catalogue, show the six products and filter Stationery. Return to all products.
2. Add a Campus hoodie, open the cart and set its quantity to two. The total should be $130.00.
3. Place a demo order, show the receipt and reload it. Explain that the database saved the order and repeated checkout does not create another one.
4. Show the test summary. Explain one small rule test, one integration test and the browser journey.
5. Hand over to the infrastructure presenter for the actual pipeline, live app and monitoring. If this demo uses localhost, say so.

## Questions the tutor might ask

**Why this technology?** We kept the team's existing C# and ASP.NET Core Razor Pages project so the app fits its .NET 10 build and runtime image. SQLite gives the demo a real database without a separate database server.

**What is the SQLite trade off?** It is simple to run and back up for a single app instance. The current design is not intended for multiple replicas sharing one database file.

**What do the tests prove?** Unit tests check small rules, integration tests exercise SQLite and HTTP form behaviour, and Chromium completes a shopping journey. Local passes do not establish that AWS or the GitHub runner works.

**What protections are implemented?** Server owned prices, quantity validation, parameterised SQL, antiforgery checks, cookie restricted receipt access and atomic checkout. This is a teaching demo, not a full retail security assessment.

**What are the health endpoints?** `/health` checks that the app responds. `/health/ready` also checks that it can query the catalogue database. Database errors return 503 from readiness.

<!-- page break -->

## What the deployment owners need

The runtime is ASP.NET Core 10. The existing Dockerfile expects already published files in `build/` and starts `SoftwareDeploymentProject.dll`. It does not compile the source itself. Full container commands are in `docs/DEPLOYMENT.md`.

Use `ASPNETCORE_URLS=http://+:8080` in the container and `Shop__DatabasePath=/data/shop.db` with a writable persistent volume mounted at `/data`. Keep `/data/keys` with the database: these Data Protection keys support antiforgery and TempData cookies. Replacing an image must keep the data volume.

For public HTTPS, the proxy handles TLS and HTTPS enforcement; set `Shop__SecureCookies=true`. Leave it false for ordinary local HTTP testing. Back up the stopped application's data directory, including the database and keys. Do not copy only an active database file and assume the backup is consistent.

The workflow runs verification on pull requests targeting `master`. A successful push to `master` also publishes images. It currently has no EC2 deployment step. Tang Ky still needs to connect deployment and demonstrate a visible change reaching the live app.

## AWS findings to hand to Judah

We restored direct SSH access to the existing Sydney instance `SWE40006-WebServer-Pass` at `3.104.124.218`. SSH is limited to Christom's current public IP; travelling may require updating that rule. No private key is included in the repository.

That server already contains WordPress with Apache, PHP and MariaDB. Docker is absent. Preserve the existing site and agree on a hosting plan before installing or replacing services. The server account also differs from the workflow's ECR account, so registry permissions need resolving. Resource IDs, the approved firewall change, evidence and rollback are in `docs/AWS_ACCESS_REVIEW.md`.

## Production checks still to record

The assigned tester should record the date, public URL, deployed commit/image and their name, then check:

1. The public HTTPS page, styles and both health endpoints load successfully.
2. Adding products, updating quantities, removal and checkout work. Reload the receipt in the same browser.
3. A controlled app restart or container replacement keeps the cart/order data through the same persistent volume. Coordinate this with the deployment owner.
4. Save screenshots and results, including any failures and fixes. Judah records monitoring evidence; Tang Ky records the automated deployment and rollback evidence.

Keep local, CI and production evidence labelled separately. The team still needs its 4 to 8 presentation slides in the official template and its signed contribution document. This guide supports those submissions; it does not replace them.

## Where to look next

Start with `README.md` and `docs/HANDOVER.md`. Use `SETUP.md` for configuration and troubleshooting, `TESTING.md` for checks, `DEPLOYMENT.md` for the runtime contract and `ASSESSMENT.md` for the demo and requirement mapping. `docs/evidence/README.md` links to the recorded local results.
