#!/usr/bin/env bash
set -euo pipefail

echo "[main-only] checkout main"
if git symbolic-ref -q HEAD >/dev/null; then
  git checkout main || git checkout -B main
else
  git checkout -B main
fi

echo "[main-only] delete local branches except main"
git for-each-ref --format='%(refname:short)' refs/heads/ | grep -v '^main$' | xargs -r -n1 git branch -D

echo "[main-only] delete remote branches except origin/main"
git for-each-ref --format='%(refname:short)' refs/remotes/origin/ | sed 's#^origin/##' | grep -v '^main$' | xargs -r -n1 -I{} git push origin --delete {}

echo "[main-only] restrict refspec"
git config remote.origin.fetch +refs/heads/main:refs/remotes/origin/main
git config --unset-all remote.origin.push || true
git config remote.origin.push refs/heads/main:refs/heads/main
git config pull.ff only
git config init.defaultBranch main

echo "[main-only] enable hooks"
chmod +x .githooks/* 2>/dev/null || true
git config core.hooksPath .githooks

echo "[main-only] verify"
git branch -a | cat
echo "[main-only] done"
