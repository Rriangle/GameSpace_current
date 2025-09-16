# MiniGame Admin 追溯性系統使用指南

## 系統概述

MiniGame Admin 追溯性系統提供端點 ↔ 視圖 ↔ 資料表的完整雙向追溯能力，確保程式碼變更時能夠準確評估影響範圍。

## 文檔結構

### 核心文檔
- `minigame_traceability_matrix.md` - 主追溯性矩陣
- `endpoints_catalog.md` - Admin 端點完整目錄
- `view_to_endpoint_map.md` - 視圖端點映射
- `table_column_dictionary.md` - 資料表欄位字典

### 報告文檔
- `reports/_latest/trace_guard.txt` - 最新守護檢查報告
- `reports/minigame_admin_ui_audit.md` - UI 審計報告
- `ui_specs/minigame_admin_ui_spec.md` - UI 規格文檔

## 使用方式

### 程式碼變更前檢查
1. 查閱 `minigame_traceability_matrix.md` 找到相關記錄
2. 檢查 "變更影響" 欄位了解受影響範圍
3. 確認所有受影響的檔案都會一併更新

### 新功能開發
1. 在 `endpoints_catalog.md` 中添加新端點記錄
2. 更新 `minigame_traceability_matrix.md` 對應行
3. 在程式碼中添加 `[TRACE]` 註解連結到矩陣錨點

### 追溯性檢查
1. 執行追溯守護工具 (未來實作)
2. 檢查 `reports/_latest/trace_guard.txt` 報告
3. 修復任何發現的不一致問題

## 錨點格式

### 矩陣錨點
- 格式: `#{controller}-{action}`
- 範例: `#adminwallet-index`, `#adminpet-edit`

### 程式碼註解
- 格式: `// [TRACE] see docs/traceability/minigame_traceability_matrix.md#{anchor}`
- 位置: 控制器動作和視圖檔案頂部

## 維護原則

### 文檔同步
- 程式碼變更時必須同步更新追溯性文檔
- 新增端點時必須添加對應的矩陣記錄
- 刪除功能時必須移除相關追溯記錄

### 檢查頻率
- 每次重大功能變更後執行守護檢查
- 定期 (每月) 執行完整追溯性審計
- CI/CD 流程中可選擇性集成守護檢查

## 最佳實務

### 開發流程
1. 變更前查閱追溯性矩陣
2. 評估影響範圍和相依性
3. 實作變更並更新文檔
4. 執行守護檢查確認一致性

### 文檔維護
- 保持矩陣記錄的完整性和準確性
- 定期檢查錨點連結的有效性
- 及時更新變更影響分析

MiniGame Admin 追溯性系統確保了系統的長期可維護性和變更安全性。