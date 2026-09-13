#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5000}"
EMAIL="smoke$(date +%s)@test.com"
PASSWORD="123456"

register_response=$(curl -fsS -X POST "$BASE_URL/api/auth/register" \
  -H 'Content-Type: application/json' \
  -d "{\"nome\":\"Smoke User\",\"email\":\"$EMAIL\",\"senha\":\"$PASSWORD\"}")

token=$(printf '%s' "$register_response" | python3 -c 'import sys, json; print(json.load(sys.stdin)["token"])')

me_response=$(curl -fsS -H "Authorization: Bearer $token" "$BASE_URL/api/auth/me")
python3 - "$EMAIL" <<'PY' <<<"$me_response"
import json, sys
payload = json.loads(sys.stdin.read())
assert payload["email"] == sys.argv[1], payload
print("AUTH_OK")
PY

admin_token=$(curl -fsS -X POST "$BASE_URL/api/auth/login" \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@helpdesk.com","senha":"admin123"}' | python3 -c 'import sys, json; print(json.load(sys.stdin)["token"])')

curl -fsS -H "Authorization: Bearer $admin_token" "$BASE_URL/api/chamados" > /tmp/helpdesk-smoke.json
python3 - <<'PY'
import json
with open('/tmp/helpdesk-smoke.json', 'r', encoding='utf-8') as f:
    data = json.load(f)
assert isinstance(data, list) and len(data) > 0, data
print("CHAMADOS_OK")
PY

echo "SMOKE_TEST_OK"
