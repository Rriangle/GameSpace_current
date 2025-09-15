#!/bin/bash
# MiniGame Area Admin 專案打包腳本

set -euo pipefail

echo "開始建立 MiniGame Area Admin 專案壓縮檔案..."

# 建立時間戳
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
ARCHIVE_NAME="GameSpace_MiniGame_Admin_${TIMESTAMP}.tar.gz"

# 切換到 workspace 目錄
cd /workspace

# 建立壓縮檔案，排除不必要的目錄
tar -czf "${ARCHIVE_NAME}" GameSpace_current/ \
  --exclude="*/node_modules/*" \
  --exclude="*/bin/*" \
  --exclude="*/obj/*" \
  --exclude="*/.git/*" \
  --exclude="*/packages/*" \
  --exclude="*/.vs/*" \
  --exclude="*/.vscode/*"

# 移動壓縮檔案到目標位置
mv "${ARCHIVE_NAME}" "GameSpace_current/${ARCHIVE_NAME}"

echo "✅ 壓縮檔案已建立：GameSpace_current/${ARCHIVE_NAME}"
echo "檔案大小：$(du -h "GameSpace_current/${ARCHIVE_NAME}" | cut -f1)"
echo "內容摘要：包含完整的 MiniGame Area Admin 後台管理系統"

# 顯示檔案資訊
ls -la "GameSpace_current/${ARCHIVE_NAME}"

echo ""
echo "請手動執行以下步驟完成上傳："
echo "1. 下載壓縮檔案：GameSpace_current/${ARCHIVE_NAME}"
echo "2. 上傳到您的 GitHub 倉庫"
echo "3. 取得 GitHub 絕對路徑並提供給使用者"