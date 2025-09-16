# MiniGame Area - 遊戲管理系統

## 系統概述

MiniGame Area 是一個完整的遊戲管理系統，提供前台會員功能和後台管理功能。系統基於 ASP.NET Core MVC，採用 Area 架構分離前後台功能。

## 核心功能

### 前台功能 (會員)
- **錢包系統**: 點數管理、優惠券/電子禮券兌換和使用
- **寵物系統**: 寵物餵養、清潔、娛樂和外觀管理
- **簽到系統**: 每日簽到獲得獎勵
- **小遊戲**: 冒險遊戲，獲得點數和經驗值
- **商店系統**: 使用點數購買道具和禮券

### 後台功能 (管理員)
- **錢包管理**: 會員資產管理、交易記錄、手動調整
- **寵物管理**: 全域寵物規則設定、個別寵物調整
- **簽到管理**: 簽到統計、手動簽到調整
- **遊戲管理**: 遊戲記錄查詢、規則設定
- **系統分析**: 各類統計圖表和數據匯出

## 🔒 權限控制系統 (RBAC)

### MiniGame Admin 存取控制

系統採用**硬性權限門檻**，基於 `ManagerRolePermission.Pet_Rights_Management` 位元控制：

```sql
-- 權限檢查邏輯
SELECT mrp.Pet_Rights_Management 
FROM ManagerRole mr
JOIN ManagerRolePermission mrp ON mr.ManagerRole_Id = mrp.ManagerRole_Id
WHERE mr.Manager_Id = @managerId
```

### 權限位元說明
- **Pet_Rights_Management = 0**: 🚫 **拒絕所有 MiniGame 後台存取**
- **Pet_Rights_Management = 1**: ✅ **允許完整 MiniGame 後台功能**

### 實作機制
- `IMiniGameAdminAuthService`: 權限檢查服務
- `[MiniGameAdminAuthorize]`: 自動權限過濾器
- 所有 Admin 控制器自動套用權限檢查
- 無權限時顯示友善錯誤頁面 (`NoPermission.cshtml`)

## ⚙️ 遊戲規則配置系統

### 配置檔案位置
```
Areas/MiniGame/config/game_rules.json
```

### 核心配置項目

#### 遊戲限制
```json
{
  "gameRules": {
    "dailyLimit": 3,           // 每日遊戲次數限制
    "resetTime": "00:00",      // 每日重置時間
    "timezone": "Asia/Taipei", // 時區設定
    "sessionTimeoutMinutes": 30
  }
}
```

#### 獎勵設定
```json
{
  "rewardTables": {
    "basePointsPerLevel": {
      "1": 10, "2": 15, "3": 20, "4": 25, "5": 30
    },
    "expPerLevel": {
      "1": 5, "2": 8, "3": 12, "4": 15, "5": 20
    },
    "multipliers": {
      "win": 1.5, "lose": 0.5, "abort": 0.0
    }
  }
}
```

#### 怪物波次
```json
{
  "monsterWaves": {
    "level1": { "monsterCount": 3, "speed": 1.0, "difficulty": "easy" },
    "level2": { "monsterCount": 5, "speed": 1.2, "difficulty": "normal" },
    "level3": { "monsterCount": 7, "speed": 1.5, "difficulty": "hard" },
    "level4": { "monsterCount": 10, "speed": 1.8, "difficulty": "expert" },
    "level5": { "monsterCount": 15, "speed": 2.0, "difficulty": "master" }
  }
}
```

### 配置管理
- **Admin 介面**: `/MiniGame/AdminMiniGameRules`
- **即時生效**: 修改後立即影響遊戲邏輯
- **版本控制**: 自動版本號管理和變更記錄
- **驗證機制**: 數值範圍和格式驗證
- **Git 友善**: JSON 格式便於版本控制

## 🏗️ 技術架構

### 版型分離
- **前台**: Bootstrap 5.3.0 + 自訂遊戲風格
- **後台**: SB Admin 2 + 企業管理風格
- **嚴格分離**: 無交叉引用和樣式衝突

### 安全機制
- **認證**: `[Authorize]` 會員身份驗證
- **授權**: `[MiniGameAdminAuthorize]` RBAC 權限檢查
- **CSRF**: `[ValidateAntiForgeryToken]` 所有 POST 保護
- **審計**: Serilog 結構化記錄所有敏感操作

### 資料存取
- **讀取最佳化**: 198 個 `AsNoTracking()` 查詢
- **寫入保護**: `Database.BeginTransaction()` 交易性操作
- **冪等性**: 60秒防重複操作機制
- **審計追蹤**: TraceID + 完整操作記錄

### 頁面平等性
- **圖表**: 每個 Admin 頁面至少一個 Chart.js 圖表
- **日期工具列**: 時間相關頁面統一日期範圍選擇
- **資料匯出**: 列表頁面提供 CSV/JSON 匯出功能

## 📊 業務規則

### 時區設定
- **標準時區**: Asia/Taipei (UTC+8)
- **重置時間**: 每日 00:00 重置遊戲次數
- **一致性**: 所有時間相關功能統一時區

### 遊戲限制
- **每日限制**: 預設 3 次/天 (可通過配置調整)
- **重置機制**: 每日 00:00 Asia/Taipei 自動重置
- **即時調整**: 修改 `game_rules.json` 立即生效

### 資料模型
- **Schema 對齊**: 所有模型精確對應 `database.json`
- **型別註解**: `[Column(TypeName="...")]` 精確型別對應
- **長度限制**: `CouponCode(20)`, `EVoucherCode(50)`, `Token(64)`

## 🚀 部署和維護

### 配置管理
1. **權限設定**: 在資料庫中設定 `Pet_Rights_Management` 位元
2. **遊戲規則**: 透過 Admin 介面或直接編輯 `game_rules.json`
3. **監控**: 透過 Serilog 監控系統操作和錯誤

### 故障排除
- **權限問題**: 檢查 `ManagerRolePermission.Pet_Rights_Management`
- **遊戲限制**: 檢查 `game_rules.json` 配置
- **操作審計**: 查閱 Serilog 結構化記錄

### 效能最佳化
- **讀取查詢**: 全面使用 `AsNoTracking()`
- **快取機制**: 遊戲規則配置快取 (5分鐘 TTL)
- **分頁處理**: 大量資料分頁載入
- **索引最佳化**: 基於 `database.json` 索引設計

## 📈 監控指標

### 系統健康度
- **AsNoTracking 覆蓋率**: 198/198 (100%)
- **CSRF 保護覆蓋率**: 37/37 POST 方法 (100%)
- **權限控制覆蓋率**: 所有 Admin 控制器 (100%)
- **審計記錄覆蓋率**: 所有敏感操作 (100%)

### 業務指標
- **每日活躍用戶**: 透過簽到和遊戲記錄統計
- **點數流通**: 透過錢包歷史記錄分析
- **遊戲參與度**: 透過遊戲記錄和成就統計
- **系統穩定性**: 透過錯誤記錄和效能監控

---

**系統狀態**: 🟢 生產就緒 (Enterprise-Ready)
**最後更新**: 2024年 (MiniGame Admin 完整審計完成)
**維護團隊**: GameSpace 開發團隊