# Deployment integration contract

This guide describes the application contract for Amogh, Tang Ky and Judah. Christom supports Judah with AWS and verifies the deployed application's behaviour. The app is not yet deployed by this contribution. A later access task updated one existing SSH source and verified direct SSH; see [AWS access review](AWS_ACCESS_REVIEW.md). No production credential is needed for local development.

## Application contract

| Setting | Requirement |
| --- | --- |
| Runtime | ASP.NET Core 10; existing `SoftwareDeploymentProject.dll` entry point retained |
| Container listening port | Explicit `ASPNETCORE_URLS=http://+:8080`; existing Dockerfile exposes 8080 |
| Persistent data | `Shop__DatabasePath=/data/shop.db` and a writable volume at `/data` |
| Cookie keys | Automatically stored at `/data/keys`; persist and restrict access with the DB directory |
| HTTPS | Terminate TLS at a reverse proxy/load balancer; set `Shop__SecureCookies=true` for the public HTTPS endpoint |
| Liveness | `GET /health` returns 200 when the app responds |
| Readiness | `GET /health/ready` returns 200 only if the catalogue can be queried; DB errors return 503 |
| Scaling | One app instance per SQLite file; no shared network-file database / multiple replicas |

The app does not redirect container HTTP to HTTPS. The public proxy must enforce HTTPS. It should set HSTS, handle certificates and constrain inbound traffic. The app does not trust arbitrary forwarded headers. For an internal HTTP-only test environment keep secure cookies false; browsers will not return Secure cookies over ordinary HTTP.

## Preserve the existing build contract

The existing runtime Dockerfile copies **prepublished files from `build/`**. It does not build C# source. GitHub Actions publishes into `publish/`, uploads that artifact, then downloads it into `build/` before the image job. This application retains those paths and entry point.

Local Docker example, from repository root, after successful verification:

```sh
dotnet publish SoftwareDeploymentProject/SoftwareDeploymentProject.csproj -c Release -o build
docker build -f SoftwareDeploymentProject.dockerfile -t campus-store:local .
docker volume create campus-store-data
docker run --rm --name campus-store -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production -e ASPNETCORE_URLS=http://+:8080 -e Shop__DatabasePath=/data/shop.db -v campus-store-data:/data campus-store:local
```

Open http://localhost:8080 and check both health endpoints. These commands are integration instructions, not evidence that Docker was executed on the development machine. When Amogh creates Compose configuration, use this same environment/volume contract. Do not mount the database in `wwwroot` or bake it into the image. Docker `EXPOSE` does not configure HTTPS or publish a port by itself; the existing 8443 exposure is not an HTTPS setup.

## AWS checklist for the team

1. Follow the supplied responsibility table: Judah leads AWS production/monitoring with Christom's support; Amogh leads Docker/staging; Tang Ky leads CI/CD. Record any agreed reassignment. Select a deployment target before changing the existing WordPress server.
2. Record account/environment identifiers and region, EC2 instance ID/type, OS, security-group rules, IAM roles, volume locations, TLS endpoint and monitoring resources. Do not record secret values.
3. Store deployment credentials in GitHub secrets or use an agreed IAM role/OIDC approach. Do not share `.pem` files in source control or screenshots.
4. Configure persistent storage and its owner/permissions. Back up the data and keys together. Validate restart/replacement with the same volume.
5. Configure a public HTTPS endpoint and secure cookies. Restrict SSH and internal application ports to intended sources.
6. Have the pipeline deploy an immutable tested image tag/digest, wait for readiness, and record the commit SHA, image digest and deployment time. The current workflow pushes `latest`; immutable tags and rollback are outstanding team pipeline work.
7. Add production monitoring (CPU, memory, disk, network and request latency as required). Memory metrics generally need an agent/configuration; collect evidence from the actual setup.
8. Document recurring costs, billing alerts and exact shutdown/deletion steps for resources actually created. No new cloud resources have been created by this contribution; the access review records the single firewall update.

## Production verification evidence

Record the actual endpoint, environment and timestamp, commit SHA and image digest. Then demonstrate:

- Public HTTPS page loads, asset requests succeed, both health endpoints return 200.
- Product addition, quantity change, removal and simulated checkout work in a browser.
- Cart/order data persists after restarting/replacing the app with the persistent volume.
- Failed readiness or application tests prevent rollout; monitor dashboards show production data.
- A visible source change moves through the pipeline into the live page (Level 4).

Use synthetic orders only. Keep these production records separate from the local evidence shipped here.

## Rollback and recovery

Retain a prior image digest. Before deployment, back up the stopped app's data directory. If a new image fails readiness, redeploy the known-good digest using the same volume and configuration, then rerun smoke checks. This initial schema has no destructive migrations. Future schema changes require a migration and rollback plan before deployment; an older image may not understand a newer schema. Never restore an older database over live orders without an explicit data-loss decision by the team.

Cleanup must identify actual resources and owners. Stop/remove demo containers without removing the data volume unless disposal is intended. Remove cloud instances, unattached disks, images, monitoring/log resources and public endpoints according to the team's resource inventory; stopping EC2 alone does not necessarily eliminate charges.
