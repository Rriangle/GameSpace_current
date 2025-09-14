#!/usr/bin/env bash
set -euo pipefail

echo "[selftest] shell: $(command -v bash)"
echo "[selftest] pwd: $(pwd)"

echo "[selftest] git version:"; git --version || true
echo "[selftest] dotnet version:"; dotnet --version || true

echo "[selftest] remote -v:"; git remote -v || true
echo "[selftest] branch status:"; git status -sb || true

echo "[selftest] list sln/csproj:"; ls -1 **/*.sln **/*.csproj 2>/dev/null || true

echo "[selftest] hooks setup"
chmod +x .githooks/* 2>/dev/null || true
git config core.hooksPath .githooks || true
echo "[selftest] done"
