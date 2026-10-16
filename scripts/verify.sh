#!/usr/bin/env bash
# Run from anywhere. --browser also checks the published app using Chromium.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"
dotnet_bin="${DOTNET_BIN:-dotnet}"
if [[ "${1:-}" != "" && "${1:-}" != "--browser" ]]; then
  echo "Usage: bash scripts/verify.sh [--browser]" >&2; exit 2
fi
mkdir -p .artifacts/tests
"$dotnet_bin" restore SoftwareDeploymentProject.slnx --locked-mode
"$dotnet_bin" test SoftwareDeploymentProject.slnx -c Release --no-restore --logger 'trx;LogFileName=application-tests.trx' --results-directory "$root/.artifacts/tests"
"$dotnet_bin" publish SoftwareDeploymentProject/SoftwareDeploymentProject.csproj -c Release --no-restore -o "$root/publish"
if [[ "${1:-}" != "--browser" ]]; then exit 0; fi
"$dotnet_bin" restore SoftwareDeploymentProject.E2E/SoftwareDeploymentProject.E2E.csproj --locked-mode
# Every invocation gets an isolated disposable database; never use the production DB.
test_directory="$(mktemp -d)"
port="${SHOP_TEST_PORT:-5087}"
server_pid=""
cleanup() {
  if [[ -n "$server_pid" ]]; then kill "$server_pid" 2>/dev/null || true; wait "$server_pid" 2>/dev/null || true; fi
  rm -rf "$test_directory"
}
trap cleanup EXIT
ASPNETCORE_ENVIRONMENT=Testing Shop__DatabasePath="$test_directory/shop.db" "$dotnet_bin" "$root/publish/SoftwareDeploymentProject.dll" --contentRoot "$root/publish" --urls "http://127.0.0.1:$port" > "$root/.artifacts/test-server.log" 2>&1 &
server_pid=$!
ready=false
for attempt in {1..40}; do
  if ! kill -0 "$server_pid" 2>/dev/null; then cat .artifacts/test-server.log; exit 1; fi
  if curl --fail --silent "http://127.0.0.1:$port/health/ready" > /dev/null; then ready=true; break; fi
  sleep 0.5
done
if [[ "$ready" != true ]]; then cat .artifacts/test-server.log; echo "Test server did not become ready" >&2; exit 1; fi
SHOP_E2E_URL="http://127.0.0.1:$port" SHOP_EVIDENCE_DIR="$root/.artifacts/browser" "$dotnet_bin" test SoftwareDeploymentProject.E2E -c Release --no-restore --logger 'trx;LogFileName=acceptance-tests.trx' --results-directory "$root/.artifacts/tests"
