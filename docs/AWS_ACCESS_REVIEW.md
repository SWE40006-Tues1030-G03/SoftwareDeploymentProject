# AWS access review — 16 October 2026

Prepared with Codex assistance for Christom. This records verified access and an inspection, not a completed application deployment.

## Resource and approved change

- Account: `243530875428`; region: Sydney (`ap-southeast-2`).
- Instance: `SWE40006-WebServer-Pass`, `i-0967046243e39fb28`, `t3.micro`.
- Public IPv4 at inspection: `3.104.124.218` (auto-assigned; may change after stop/start).
- Security group: `sg-011db4646c96363a2` (`launch-wizard-1`).
- After Christom approved the exact preview, updated existing SSH rule `sgr-019643ce6459584d9`: TCP 22 source changed from `49.193.90.7/32` to `49.185.87.133/32`.
- Description: `Christom current IP - project administration 2026-10-16`.
- HTTP 80 and HTTPS 443 rules remain as previously configured; no instance, role, application or database changes were made.
- AWS displayed success and direct SSH subsequently succeeded. [Saved console evidence](evidence/aws-ssh-rule-saved.jpg).

## Connection procedure

The existing private key is on Christom's laptop at `~/Downloads/swe40006-pass-key.pem`. Its contents were not printed or copied into Git. Other team members need their own approved access arrangement; do not distribute the private key in chat or documentation.

```sh
ssh -i ~/Downloads/swe40006-pass-key.pem ec2-user@3.104.124.218
```

Verify the host identity using a trusted source before accepting a new host key. Inspection used a separate known-hosts file at `/private/tmp/swe40006-known-hosts`. The temporary file is local bookkeeping, not a team credential.

If SSH times out again, check the current public IP, instance address/state and security-group source. Update SSH only to the approved administrator's current `/32`; travel can change this address. The browser Instance Connect attempt failed under the original source restriction; it has not been re-tested. Direct key-based SSH is verified.

## Existing workload found (read-only inspection)

| Item | Verified observation |
| --- | --- |
| Operating system | Amazon Linux 2023.12 |
| Existing website | WordPress files under `/var/www/html`, including `index.php` and `wp-admin/` |
| Web stack | `httpd`, `php-fpm`, and `mariadb` services running |
| Listening ports | SSH 22, Apache 80, MariaDB 3306; no HTTPS listener observed |
| Docker | Not installed |
| Root disk | 8 GB total, approximately 5.5 GB available |
| Session Manager | Agent running, but no instance IAM role attached in the console; working Session Manager access is not established |

Inspection commands included `cat /etc/os-release`, `rpm -q`, `systemctl is-active`, `systemctl list-units --type=service --state=running`, `sudo ss -lntp`, `df -h /`, and a filename-only listing under `/var/www/html`. No WordPress configuration, private key contents or database records were read.

## Deployment decision still required

This is an occupied WordPress server, not an empty Campus Store host. Preserve its files, database and Apache configuration. Do not replace the website or stop its services without an agreed migration and recovery plan. A separate instance would need an agreed spending limit; coexistence would need an explicit routing/port design and backup plan.

The existing CI ECR destination uses account `977456087342`, whereas this server is in `243530875428`. Resolve the intended registry/account and deployment permissions before wiring CI deployment. No cross-account access has been configured.

## Rollback and follow-up

To reverse this firewall change, edit the same SSH rule and restore source `49.193.90.7/32` and its original empty description. This removes access from the new address; verify a working administrator path first. No server software rollback is needed because this session only inspected it.

Remaining: agree where Campus Store will run; back up any workload affected by deployment; configure Docker/persistent data, CI deployment, monitoring and rollback; verify the live application. Keep this document's IP addresses and observations current as the setup evolves.
