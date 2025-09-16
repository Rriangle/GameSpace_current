#!/bin/bash

# 檢查分支狀態的腳本

echo "🔍 GameSpace 分支狀態檢查"
echo "========================"

# 檢查當前分支
current_branch=$(git branch --show-current)
echo "📍 當前分支: $current_branch"

if [ "$current_branch" != "main" ]; then
    echo "❌ 錯誤：不在 main 分支！"
    echo "💡 請執行: git checkout main"
    exit 1
fi

# 檢查遠端分支
echo ""
echo "🌐 遠端分支列表:"
git branch -r

# 檢查本地分支
echo ""
echo "💻 本地分支列表:"
git branch

# 檢查是否有未提交的變更
echo ""
echo "📝 工作區狀態:"
if [ -z "$(git status --porcelain)" ]; then
    echo "✅ 工作區乾淨"
else
    echo "⚠️  有未提交的變更:"
    git status --short
fi

# 檢查與遠端的同步狀態
echo ""
echo "🔄 與遠端同步狀態:"
git fetch origin main --quiet
LOCAL=$(git rev-parse @)
REMOTE=$(git rev-parse @{u})
BASE=$(git merge-base @ @{u})

if [ $LOCAL = $REMOTE ]; then
    echo "✅ 與遠端同步"
elif [ $LOCAL = $BASE ]; then
    echo "⬇️  需要拉取遠端更新"
    echo "💡 請執行: git pull origin main"
elif [ $REMOTE = $BASE ]; then
    echo "⬆️  有本地提交未推送"
    echo "💡 請執行: git push origin main"
else
    echo "🔀 本地與遠端有分歧"
    echo "💡 請執行: git pull origin main --rebase"
fi

echo ""
echo "📖 分支管理政策: 請參考 BRANCH_POLICY.md"
echo "========================"