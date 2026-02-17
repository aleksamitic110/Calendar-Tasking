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

SQLCMD_COMMON_ARGS=(
  -S "${DB_HOST},${DB_PORT}"
  -U sa
  -P "${SA_PASSWORD}"
  -C
  -b
  -V 16
)

run_sql_file_with_retry() {
  local file_path="$1"
  local max_attempts="${2:-30}"
  local sleep_seconds="${3:-2}"
  local attempt

  for attempt in $(seq 1 "$max_attempts"); do
    if "$SQLCMD" "${SQLCMD_COMMON_ARGS[@]}" -i "$file_path"; then
      return 0
    fi

    if [[ "$attempt" -eq "$max_attempts" ]]; then
      echo "Failed to apply ${file_path} after ${max_attempts} attempts." >&2
      return 1
    fi

    echo "Retrying ${file_path} (${attempt}/${max_attempts})..."
    sleep "$sleep_seconds"
  done
}

echo "Waiting for SQL Server (${DB_HOST}:${DB_PORT})..."
for i in $(seq 1 60); do
  if "$SQLCMD" "${SQLCMD_COMMON_ARGS[@]}" -Q "SELECT 1" >/dev/null 2>&1; then
    break
  fi

  if [[ "$i" -eq 60 ]]; then
    echo "SQL Server did not become ready in time." >&2
    exit 1
  fi

  sleep 2
done

echo "Applying database/schema.sql"
run_sql_file_with_retry /scripts/schema.sql

echo "Applying database/seed.sql"
run_sql_file_with_retry /scripts/seed.sql

echo "Database initialization completed."
