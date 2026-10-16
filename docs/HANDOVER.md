# Christom's handover

## Current scope

This contribution provides the application foundation, automated tests and documentation. It uses the main team's existing .NET project rather than the previously suggested Flask/React examples. The separate `ExperimentWithActions` repository was inspected as context and remains unchanged.

**Start with:** [short team and presentation guide](QUICK_GUIDE.md), then [README](../README.md) for commands. The work branch is `Christoms-Branch`; the default branch is named `master`, not `main`. The application commits have been pushed to the work branch. A merge into `master` triggers image publishing, so the team should review the pull request and its checks first.

## Ownership and handover boundaries

| Area | Application contribution | Next owner action |
| --- | --- | --- |
| App, cart, database and tests | Christom's development and local automated testing implemented here | Christom verifies production functionality after deployment; a covering teammate must accept this check if he is away |
| Docker / Compose and staging | Existing runtime Dockerfile retained; runtime/data contract documented | Amogh, supported by Tang Ky: validate image, persistent volume and test/staging environments |
| CI/CD | Application verification inserted before existing build-artifact upload | Tang Ky: validate GitHub runner, immutable tags and automated deployment |
| AWS / monitoring | Christom restored SSH access and inspected an existing server; see [AWS access review](AWS_ACCESS_REVIEW.md) | Judah leads EC2, deployment and CloudWatch, supported by Christom; current server contains WordPress |
| Presentation/report | Architecture, evidence captions, demo script and contribution notes provided | Group incorporates actual infrastructure results and signs its contribution document |

## Complete vs pending

Implemented: product browsing/filtering; persistent anonymous carts; quantity validation; removal/totals; atomic simulated checkout; per-browser receipts; readiness/liveness; automated tests; selected local evidence; documented configuration and deployment contract.

Pending external verification: Docker execution, GitHub Actions execution for this branch, registry publishing, separate test/staging runtime instances, AWS production deployment, live database persistence and monitoring, rollback rehearsal, final Level 4 source-change demonstration. Do not label the entire assignment complete based on application tests alone.

## Suggested first 30 minutes for the covering teammate

1. Read README and this file, select the work branch and check `git status` before editing.
2. Run the app, add two hoodies, confirm $130.00, place a demo order and reload the receipt.
3. Run full verification and inspect `.artifacts/tests/` and browser screenshots.
4. Read the architecture walkthrough to locate page handlers, SQL and cookie behaviour.
5. Review the deployment contract with the Docker and CI owners, especially `/data` persistence and HTTPS cookies.
6. Record any changes and actual test results in the work log; retain commit links for contribution evidence.

## Before Christom leaves on Sunday

1. A covering teammate checks out the work branch, runs the app and follows the short guide's demo. Record their name and any issues in the work log; no acceptance has been recorded yet.
2. Judah confirms the deployment target. Preserve the existing WordPress workload on `SWE40006-WebServer-Pass`; this is not an empty server. Resolve the different server and ECR accounts before granting deployment access.
3. Amogh and Tang Ky review the runtime/data contract, then run the container and pipeline checks they own. A local test pass does not establish their results.
4. If production is available, Christom runs the checklist in [deployment](DEPLOYMENT.md). Otherwise, the group names a covering teammate to run it after deployment and records production verification as pending until then.
5. Use the short guide for presentation preparation. Both Word guides are included in this branch under `docs/`. Start with the four-page guide; use the larger reference for detailed code explanations. The large reference predates the AWS access work; use the AWS access review for the current infrastructure status.

## Where evidence lives

Selected test results and screenshots are indexed in [evidence](evidence/README.md). The handover rerun is recorded under `docs/evidence/handover-2026-10-16/`. Raw `.artifacts/` output is disposable and ignored by Git. AWS access evidence is linked from its review. Do not present local results as production or GitHub runner evidence.

## Known limitations to communicate

No real payments, inventory, accounts, delivery, order history or admin editing. Anonymous receipt access ends when a new cart starts or the cookie is lost. Data has no automatic cleanup. SQLite is for a single-instance demo. Schema initialization is not a versioned migration system. Public TLS, rate limiting, backups, access control and monitoring are infrastructure responsibilities. The visual checks are not a full accessibility audit.

## Change record discipline

For every additional application or AWS change record: author/reviewer, purpose, files or resource IDs affected, configuration names, commands/procedure, actual result, evidence location, commit/image reference, rollback/cleanup and remaining issues. Keep secret values out of logs and Git. Only claim work you can explain and reproduce. The implementation was prepared with Codex assistance; review it and follow the unit's applicable AI-use disclosure rules.

The travel/presentation arrangement was handled by Christom with the tutor. The group still needs its required deck and signed contribution document; technical notes here do not replace those submissions.

Registry publishing is restricted to pushes to `master`; pull requests run verification without publishing images.
