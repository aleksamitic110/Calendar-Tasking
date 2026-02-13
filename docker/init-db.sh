#!/usr/bin/env bash
set -euo pipefail

DB_HOST="${DB_HOST:-db}"
DB_PORT="${DB_PORT:-1433}"

if [[ -z "${SA_PASSWORD:-}" ]]; then
  echo "SA_PASSWORD is not set." >&2
  exit 1
fi

SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
if [[ ! -x "$SQLCMD" ]]; then
  SQLCMD="/opt/mssql-tools/bin/sqlcmd"
fi

if [[ ! -x "$SQLCMD" ]]; then
  SQLCMD="$(command -v sqlcmd || true)"
fi

if [[ -z "${SQLCMD}" || ! -x "${SQLCMD}" ]]; then
  echo "sqlcmd not found in container." >&2
  exit 1
fi

echo "Waiting for SQL Server (${DB_HOST}:${DB_PORT})..."
for i in $(seq 1 60); do
  if "$SQLCMD" -S "${DB_HOST},${DB_PORT}" -U sa -P "${SA_PASSWORD}" -C -Q "SELECT 1" >/dev/null 2>&1; then
    break
  fi

  if [[ "$i" -eq 60 ]]; then
    echo "SQL Server did not become ready in time." >&2
    exit 1
  fi

  sleep 2
done

echo "Applying database/schema.sql"
"$SQLCMD" -S "${DB_HOST},${DB_PORT}" -U sa -P "${SA_PASSWORD}" -C -i /scripts/schema.sql

echo "Applying database/seed.sql"
"$SQLCMD" -S "${DB_HOST},${DB_PORT}" -U sa -P "${SA_PASSWORD}" -C -i /scripts/seed.sql

echo "Database initialization completed."
