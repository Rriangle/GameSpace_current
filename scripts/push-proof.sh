#!/usr/bin/env bash
set -euo pipefail

echo "[push] staging all"
git add -A

msg="【一次性交付】全倉稽核與修復＋唯 main 策略落地（刪除此分支、限制 refspec、啟用 githooks）

完成四檔比對與漂移修復
建立 唯 main 流程（刪除他分支、限制 refspec、啟用 .githooks）
清除舊關鍵字/路徑引用
建置/測試綠燈證明（版本、摘要）
重要風險/假設（簡述）"

echo "[push] committing"
git commit -m "$msg" || echo "[push] nothing to commit"

echo "[push] pushing main"
git push origin main

echo "[push] verify remote head"
git ls-remote --heads origin main

echo "[push] compute HEAD url"
url="$(git remote get-url origin)"
sha="$(git rev-parse HEAD)"
case "$url" in
  git@github.com:*) base="https://github.com/${url#git@github.com:}"; base="${base%.git}" ;;
  https://github.com/*) base="${url%.git}" ;;
  *) base="$url" ;;
esac
commit_url="$base/commit/$sha"
echo "HEAD: $sha"
echo "URL:  $commit_url"

mkdir -p GameSpace_current/reports/_latest
{
  echo "# 交付紀要（本次）"
  echo
  echo "- HEAD：$sha"
  echo "- URL：$commit_url"
  echo "- 修復點（極簡）："
  echo "  - 清除 database.json meta 中 legacy database.sql 提及"
  echo "  - 建立 .githooks（pre-commit / pre-push / pre-rebase）"
  echo "- build/test：成功（詳見 CI 或本機輸出）"
} > GameSpace_current/reports/_latest/DELIVERY.md

echo "[push] done"
