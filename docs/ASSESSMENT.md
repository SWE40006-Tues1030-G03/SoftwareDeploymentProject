# Assessment mapping and demonstration notes

Based on the project brief and presentation screenshots provided by Christom. Check Canvas for updates. The screenshots specify a 15-minute group presentation plus 5 minutes Q&A, 4–8 slides in the official template, and a signed contribution document. They do not prescribe specific shopping-cart features or real payments.

## Requirements and evidence boundaries

| Requirement | Application support | Remaining group evidence |
| --- | --- | --- |
| Level 1: repository, CI/build, test/staging and production environments | Existing repo/Actions project preserved; isolated local acceptance runtime available | Show actual environment instances and clarify Development/Testing/Staging/Production isolation with tutor |
| Level 2: production instrumentation | Readiness/liveness endpoints available | Actual production system/request metrics, monitoring configuration and dashboard |
| Level 3: working app, automated unit/integration/E2E tests | App and all three layers implemented; local evidence in this repo | Execute suites on designated test server before actual deployment |
| Level 3: live availability/database/behaviour | Persistent SQLite and deployment contract | Real public endpoint, production DB persistence and browser smoke results |
| Level 4: automatic trigger-to-deployment | Existing Actions build now gates image publishing on tests | Demonstrate source change, successful stages, deployment and live visible result |
| Final report: steps, screenshots, raw logs, errors, lessons and contributions | Setup, architecture, testing, deployment guide, work log and selected evidence | Incorporate each teammate's actual infrastructure work and failures |

The brief lists four environment names alongside server roles that do not map one-to-one. Do not assume a single Actions job establishes all testing/staging environment requirements. Record what the team actually runs and resolve that interpretation with teaching staff.

## App demo: approximately 2–3 minutes

1. Show the catalogue and briefly explain Razor Pages + SQLite were selected to fit the existing .NET build/container.
2. Filter Stationery, then return to all products. Add a Campus hoodie and open the cart.
3. Set quantity to two: total should be $130.00. Explain prices are read server-side and totals use integer cents.
4. Place a demo order. Show saved receipt, reload it, explain atomic order persistence and duplicate-checkout protection.
5. Show the test summary and one relevant test: HTTP antiforgery rejection or concurrent checkout. Explain unit vs integration vs browser tests.
6. Hand over to the pipeline/infrastructure presenter for actual trigger, deployment and monitoring evidence.

Prepare a recorded fallback using the same steps if needed. The presentation page permits a live or recorded walkthrough; confirm any group-specific arrangements separately. A local demo cannot replace the required proof of a deployed pipeline.

## Suggested 6-slide group structure

1. Background/problem: consistent, repeatable deployment and early detection of failures.
2. Scope/environment architecture: application and actual servers; boundaries/owners.
3. Tool choices/trade-offs: .NET, GitHub Actions, Docker, SQLite, AWS and monitoring.
4. Implementation/challenges: request flow, test layers, persistence and problems solved.
5. Live/recorded code and pipeline demonstration with actual evidence.
6. Results, limitations, lessons and contribution breakdown.

Use the official Swinburne slide template supplied by the course; it is not present in this repository. Do not fabricate screenshots, signatures, deployment outcomes or contribution claims.

## Explaining the design in plain language

- **Razor Pages:** C# handles a browser request and renders HTML; no separate React app is needed.
- **Docker image:** packages the published app/runtime; database files live in a persistent volume outside the replaceable image.
- **Unit test:** verifies a small calculation or rule.
- **Integration test:** verifies components together, including real SQLite or HTTP form handling.
- **Acceptance test:** a browser follows a shopper's journey.
- **Idempotent checkout:** clicking or submitting again returns the same order instead of creating another.
- **Readiness:** the app can query its database, not merely answer a network request.
