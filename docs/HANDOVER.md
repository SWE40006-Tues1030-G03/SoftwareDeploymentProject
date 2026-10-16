# Christom's handover

## Current scope

This contribution provides the application foundation, automated tests and documentation. It uses the main team's existing .NET project rather than the previously suggested Flask/React examples. The separate `ExperimentWithActions` repository was inspected as context and remains unchanged.

**Start with:** [README](../README.md), then run `bash scripts/verify.sh --browser` after browser setup. The work branch is `codex/shopping-cart`. Merge only after reviewing the code and the GitHub workflow results; merging into `master` triggers the team's existing image-publishing workflow and may depend on AWS/registry secrets.

## Ownership and handover boundaries

| Area | Application contribution | Next owner action |
| --- | --- | --- |
| App, cart, database and tests | Christom's deliverables implemented here | Christom / covering teammate review and maintain |
| Docker / Compose | Existing runtime Dockerfile retained; runtime/data contract documented | Amogh with Tang Ky: validate image and persistent volume |
| CI/CD | Application verification inserted before existing build-artifact upload | Tang Ky: validate GitHub runner, immutable tags, staging and live deployment |
| AWS / monitoring | Configuration and verification checklist only; no cloud changes | Confirm current team owner; screenshots indicate roles may have changed |
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

## Known limitations to communicate

No real payments, inventory, accounts, delivery, order history or admin editing. Anonymous receipt access ends when a new cart starts or the cookie is lost. Data has no automatic cleanup. SQLite is for a single-instance demo. Schema initialization is not a versioned migration system. Public TLS, rate limiting, backups, access control and monitoring are infrastructure responsibilities. The visual checks are not a full accessibility audit.

## Change record discipline

For every additional application or AWS change record: author/reviewer, purpose, files or resource IDs affected, configuration names, commands/procedure, actual result, evidence location, commit/image reference, rollback/cleanup and remaining issues. Keep secret values out of logs and Git. Only claim work you can explain and reproduce. The implementation was prepared with Codex assistance; review it and follow the unit's applicable AI-use disclosure rules.

The travel/presentation arrangement was handled by Christom with the tutor. The group still needs its required deck and signed contribution document; technical notes here do not replace those submissions.

Registry publishing is restricted to pushes to `master`; pull requests run verification without publishing images.
