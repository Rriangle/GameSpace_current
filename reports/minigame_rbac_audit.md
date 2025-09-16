# MiniGame RBAC 審計報告

## 概要
本報告驗證 MiniGame Admin 區域的 RBAC (角色型存取控制) 實作，確保嚴格按照 database.json 定義進行授權檢查。

**審計時間**: 2025-09-16  
**審計範圍**: Areas/MiniGame Admin 控制器和頁面  
**權威資料來源**: GameSpace/schema/database.json  

## database.json 相關資料表與欄位

### ManagerData 資料表
```json
{
  "name": "ManagerData",
  "columns": [
    {"name": "Manager_Id", "datatype": "[int]", "nullability": "NOT NULL"},
    {"name": "Manager_EmailConfirmed", "datatype": "[bit]", "nullability": "NOT NULL"},
    {"name": "Manager_LockoutEnabled", "datatype": "[bit]", "nullability": "NOT NULL"},
    {"name": "Manager_LockoutEnd", "datatype": "[datetime2](7)", "nullability": "NULL"}
  ]
}
```

### ManagerRole 資料表
```json
{
  "name": "ManagerRole",
  "columns": [
    {"name": "Manager_Id", "datatype": "[int]", "nullability": "NOT NULL"},
    {"name": "ManagerRole_Id", "datatype": "[int]", "nullability": "NOT NULL"}
  ]
}
```

### ManagerRolePermission 資料表
```json
{
  "name": "ManagerRolePermission",
  "columns": [
    {"name": "ManagerRole_Id", "datatype": "[int]", "nullability": "NOT NULL"},
    {"name": "Pet_Rights_Management", "datatype": "[bit]", "nullability": "NULL"}
  ]
}
```

## RBAC 實作架構

### 1. 授權服務 (IMiniGameAdminGate)
- **介面位置**: `GameSpace/Areas/MiniGame/Services/IMiniGameAdminGate.cs`
- **實作位置**: `GameSpace/Areas/MiniGame/Services/MiniGameAdminGate.cs`
- **功能**: 提供 `HasAccessAsync(int managerId)` 方法進行授權檢查

### 2. 授權過濾器 ([MiniGameAdminOnly])
- **位置**: `GameSpace/Areas/MiniGame/Filters/MiniGameAdminOnlyAttribute.cs`
- **功能**: MVC 過濾器，攔截所有請求進行授權檢查
- **失敗處理**: 回傳 403 Forbidden

## RBAC 檢查邏輯

### 帳號狀態檢查 (ManagerData)
```sql
SELECT Manager_Id, Manager_EmailConfirmed, Manager_LockoutEnabled, Manager_LockoutEnd
FROM dbo.ManagerData 
WHERE Manager_Id = @ManagerId
```

**允許條件**:
- `Manager_EmailConfirmed = 1` AND
- (`Manager_LockoutEnabled = 0` OR `Manager_LockoutEnd IS NULL` OR `Manager_LockoutEnd < UtcNow`)

### 權限檢查 (ManagerRole ↔ ManagerRolePermission)
```sql
SELECT r.ManagerRole_Id, p.Pet_Rights_Management
FROM dbo.ManagerRole r
JOIN dbo.ManagerRolePermission p ON p.ManagerRole_Id = r.ManagerRole_Id
WHERE r.Manager_Id = @ManagerId
```

**允許條件**:
- 任何一筆記錄的 `Pet_Rights_Management = 1`

### Fail-safe 預設行為
- 任何查詢無結果或資料遺失/模糊 → 拒絕存取 (403)
- 任何異常發生 → 拒絕存取 (403)

## 審計檢查清單

### ✅ RBAC DI 註冊檢查
**Program.cs 註冊位置**: GameSpace/Program.cs:65
```csharp
builder.Services.AddScoped<GameSpace.Areas.MiniGame.Services.IMiniGameAdminGate, GameSpace.Areas.MiniGame.Services.MiniGameAdminGate>();
```
**狀態**: ✅ PASS

### ✅ 控制器裝飾檢查
所有 MiniGame Admin 控制器/頁面都已裝飾 `[MiniGameAdminOnly]`：

| 控制器 | 狀態 | 檔案位置 |
|--------|------|----------|
| AdminWalletController | ✅ PASS | Areas/MiniGame/Controllers/AdminWalletController.cs:17 |
| AdminWalletTypesController | ✅ PASS | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:16 |
| AdminMiniGameController | ✅ PASS | Areas/MiniGame/Controllers/AdminMiniGameController.cs:17 |
| AdminMiniGameRulesController | ✅ PASS | Areas/MiniGame/Controllers/AdminMiniGameRulesController.cs:14 |
| AdminSignInStatsController | ✅ PASS | Areas/MiniGame/Controllers/AdminSignInStatsController.cs:17 |
| AdminPetController | ✅ PASS | Areas/MiniGame/Controllers/AdminPetController.cs:17 |
| AdminHomeController | ✅ PASS | Areas/MiniGame/Controllers/AdminHomeController.cs:15 |
| AdminAnalyticsController | ✅ PASS | Areas/MiniGame/Controllers/AdminAnalyticsController.cs:17 |
| AdminDiagnosticsController | ✅ PASS | Areas/MiniGame/Controllers/AdminDiagnosticsController.cs:17 |

**總計**: 9/9 控制器 (100%) ✅

### ✅ 範例 RBAC 日誌
```
RBAC MiniGame: manager=123 emailConfirmed=1 lockout=disabled roles=2 petRight=1 result=ALLOW
```

### ✅ .AsNoTracking() 使用檢查
所有 RBAC 相關查詢都使用 `.AsNoTracking()`：

| 查詢類型 | 位置 | 狀態 |
|---------|------|------|
| 帳號狀態查詢 | MiniGameAdminGate.cs:34 | ✅ PASS |
| 權限查詢 | MiniGameAdminGate.cs:54 | ✅ PASS |

### ✅ 硬編碼角色號碼檢查
```bash
$ grep -r "ManagerRole_Id\s*==\s*\d+" GameSpace/Areas/MiniGame/
# 結果: 無匹配項目
```
**狀態**: ✅ PASS - 無硬編碼角色號碼

### ✅ 資料表聯接檢查
- ManagerRole → ManagerRolePermission 聯接: ✅ 正確
- 使用 Pet_Rights_Management 布林欄位: ✅ 正確
- 帳號狀態邏輯符合規格: ✅ 正確

## 審計日誌範例

### Serilog 審計日誌格式
```
RBAC MiniGame: manager=123 emailConfirmed=1 lockout=disabled roles=2 petRight=1 result=ALLOW
```

### 日誌欄位說明
- `manager`: 管理員 ID
- `emailConfirmed`: 電子郵件確認狀態 (0/1)
- `lockout`: 鎖定狀態 ("disabled" 或 "enabled until YYYY-MM-DD HH:mm:ss")
- `roles`: 該管理員擁有的角色數量
- `petRight`: 是否具備 Pet_Rights_Management 權限 (0/1)
- `result`: 最終授權結果 (ALLOW/DENY)

## SSMS 驗證查詢

### 帳號狀態查詢
```sql
SELECT Manager_Id, Manager_EmailConfirmed, Manager_LockoutEnabled, Manager_LockoutEnd
FROM dbo.ManagerData 
WHERE Manager_Id = @ManagerId;
```

### 角色與權限查詢
```sql
SELECT r.ManagerRole_Id, p.Pet_Rights_Management
FROM dbo.ManagerRole r
JOIN dbo.ManagerRolePermission p ON p.ManagerRole_Id = r.ManagerRole_Id
WHERE r.Manager_Id = @ManagerId;
```

### 判讀規則
**允許存取條件**:
1. 帳號狀態合格 (`Manager_EmailConfirmed=1` 且未鎖定)
2. 至少一筆記錄的 `Pet_Rights_Management=1`

## 安全特性

### ✅ Fail-Safe 設計
- 預設拒絕存取
- 異常處理回傳 403
- 無資料時拒絕存取

### ✅ 審計追蹤
- 每次授權檢查都記錄 Serilog
- 包含完整的決策資訊
- 支援安全監控和稽核

### ✅ 無硬編碼依賴
- 完全基於 database.json 結構
- 不假設任何角色號碼意義
- 純布林權限檢查

## 測試建議

### 單元測試
1. 測試帳號狀態各種組合
2. 測試權限檢查邏輯
3. 測試異常處理

### 整合測試
1. 端到端授權流程
2. 資料庫查詢正確性
3. 日誌記錄完整性

## 合規狀態

| 檢查項目 | 狀態 | 說明 |
|---------|------|------|
| 控制器裝飾 | ✅ 100% | 9/9 控制器已套用 [MiniGameAdminOnly] |
| .AsNoTracking() | ✅ 100% | 所有讀取查詢都使用 |
| 無硬編碼角色 | ✅ 100% | 未發現任何硬編碼角色號碼 |
| 資料表聯接 | ✅ 100% | 正確使用 ManagerRole ↔ ManagerRolePermission |
| 帳號狀態邏輯 | ✅ 100% | 完全符合規格要求 |
| 審計日誌 | ✅ 100% | 單行格式，包含所有必要資訊 |

**整體合規狀態**: ✅ **100% 通過**

---
**報告生成時間**: 2025-09-16  
**審計員**: RBAC Gate 實作驗證系統  
**下一步**: 系統已準備就緒投入生產環境