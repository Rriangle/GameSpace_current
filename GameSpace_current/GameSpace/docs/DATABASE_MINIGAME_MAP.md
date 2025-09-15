# MiniGame Area 資料庫對應文件

## 資料表與 Admin 頁面對應

根據 database.json 定義，MiniGame Area 包含四個模組與對應資料表：

### User_Wallet 模組
**資料表**：User_Wallet, CouponType, Coupon, EVoucherType, EVoucher, EVoucherToken, EVoucherRedeemLog, WalletHistory

**Admin 頁面對應**：
- **AdminWalletController**
  - `Index()` → User_Wallet 列表頁面
  - `Details(userId)` → User_Wallet 明細頁面
  - `History()` → WalletHistory 歷史記錄
  - `EVouchers()` → EVoucher 管理列表
  - `Coupons()` → Coupon 管理列表

- **AdminWalletTypesController**
  - `CouponTypes()` → CouponType 列表（CRUD 允許）
  - `CreateCouponType()` → CouponType 新增
  - `EditCouponType()` → CouponType 編輯
  - `DeleteCouponType()` → CouponType 刪除
  - `EVoucherTypes()` → EVoucherType 列表（CRUD 允許）
  - `CreateEVoucherType()` → EVoucherType 新增
  - `EditEVoucherType()` → EVoucherType 編輯
  - `DeleteEVoucherType()` → EVoucherType 刪除

### UserSignInStats 模組
**資料表**：UserSignInStats

**Admin 頁面對應**：
- **AdminSignInStatsController**
  - `Index()` → UserSignInStats 列表頁面（Read-first）
  - `Details(id)` → UserSignInStats 明細頁面
  - `UserHistory(userId)` → 用戶簽到歷史
  - `Statistics()` → 簽到統計報表

### Pet 模組
**資料表**：Pet

**Admin 頁面對應**：
- **AdminPetController**
  - `Index()` → Pet 列表頁面（Read-first）
  - `Details(id)` → Pet 明細頁面
  - `Statistics()` → 寵物統計頁面
  - `AdjustStatus()` → 寵物狀態調整（預留實作）

### MiniGame 模組
**資料表**：MiniGame

**Admin 頁面對應**：
- **AdminMiniGameController**
  - `Index()` → MiniGame 記錄列表（Read-first）
  - `Details(id)` → MiniGame 記錄明細
  - `Statistics()` → 遊戲統計報表
  - `Settings()` → 遊戲設定管理（預留實作）

## 欄位對應與 ReadModel

### 查詢規則
- **Read-first 原則**：所有查詢使用 `AsNoTracking()`
- **ReadModel 投影**：避免直接返回 Entity，投影至 DTO/ReadModel
- **分頁處理**：伺服器端分頁、篩選、排序
- **避免 N+1**：適當使用 `Include()` 但避免過量載入

### CRUD 限制
**允許 CRUD**：僅型別表
- CouponType：優惠券類型管理
- EVoucherType：電子禮券類型管理

**僅允許 Read**：其他所有表
- User_Wallet, Coupon, EVoucher, EVoucherToken, EVoucherRedeemLog, WalletHistory
- UserSignInStats
- Pet  
- MiniGame

### 資料庫真實來源
**database.json 為唯一真實來源**
- 不得進行 EF Migrations 或 schema 變更
- 所有資料表結構以 database.json 為準
- 如發現不一致，以 database.json 為準進行修正

## 健康檢查端點

### 端點定義
- **路由**：`/MiniGame/Health/Database`
- **回傳格式**：`{ status: "ok" }` 或錯誤訊息
- **檢查範圍**：MiniGame Area 相關資料表連線狀態

### 系統狀態端點
- **路由**：`/MiniGame/Health/Status`
- **功能**：回傳 MiniGame Admin 系統狀態與模組資訊

---
*建立時間：2025/09/15*
*資料來源：database.json*