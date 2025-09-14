#!/usr/bin/env bash
set -euo pipefail

pushd GameSpace_current/GameSpace >/dev/null
echo "[build] dotnet restore"; dotnet restore
echo "[build] dotnet build -c Release"; dotnet build -c Release
popd >/dev/null

pushd GameSpace_current/GameSpace.Tests >/dev/null
echo "[test] dotnet test --no-build -c Release"; dotnet test --no-build -c Release
popd >/dev/null

echo "[build] done"
