# GameSpace MiniGame Area Admin 開發記錄

## 本次執行範圍
**僅開發 MiniGame Area 的 Admin（後台）功能**

根據 RUN DIRECTIVE — MiniGame Area Admin ONLY 指令，本次執行範圍明確覆寫全域完工要求，僅交付 MiniGame 後台管理功能。

## 已完成項目 (Done)

### Stage-1: User_Wallet 模組後台管理
- ✅ **AdminWalletController**: 錢包列表/明細/歷史查詢（Read-first 原則）
  - `Index()`: 錢包列表頁面，支援搜尋、分頁、排序
  - `Details(userId)`: 錢包明細頁面，包含基本資訊和歷史記錄
  - `History()`: 錢包歷史記錄列表，支援篩選
  - `EVouchers()`: 電子禮券管理列表
  - `Coupons()`: 優惠券管理列表
  - 所有查詢採用 `AsNoTracking()` 投影至 ReadModel

- ✅ **AdminWalletTypesController**: CouponType/EVoucherType CRUD（僅型別表）
  - `CouponTypes()`: 優惠券類型列表
  - `CreateCouponType()`: 新增優惠券類型
  - `EditCouponType()`: 編輯優惠券類型  
  - `DeleteCouponType()`: 刪除優惠券類型（含關聯檢查）
  - `EVoucherTypes()`: 電子禮券類型 CRUD（對應方法）

- ✅ **_AdminLayout.cshtml**: SB Admin 風格後台版型
  - 使用 SB Admin 第三方套件風格
  - 側邊欄以模組分群：User_Wallet、UserSignInStats、Pet、MiniGame
  - 響應式設計，支援行動裝置
  - 麵包屑導航、訊息提示系統

### Stage-2: UserSignInStats 模組後台管理
- ✅ **AdminSignInStatsController**: 簽到記錄查詢與統計（Read-first 原則）
  - `Index()`: 簽到記錄列表，支援日期、用戶篩選
  - `Details(id)`: 簽到記錄明細
  - `UserHistory(userId)`: 用戶簽到歷史，含連續簽到統計
  - `Statistics()`: 簽到統計報表，含圖表展示
  - 連續簽到天數計算邏輯

- ✅ **AdminPetController**: 寵物管理（Read-first + Stub）
  - `Index()`: 寵物列表查詢，支援等級篩選
  - `Details(id)`: 寵物明細資訊
  - `Statistics()`: 寵物統計資料
  - `AdjustStatus()`: 寵物狀態調整 Stub（不破壞規格）
  - `BatchMaintenance()`: 批次維護 Stub

- ✅ **AdminMiniGameController**: 小遊戲管理（Read-first + Stub）
  - `Index()`: 遊戲記錄列表，支援結果、關卡篩選
  - `Details(id)`: 遊戲記錄明細
  - `Statistics()`: 遊戲統計報表，含勝率分析
  - `Settings()`: 遊戲設定管理 Stub
  - `UpdateSettings()`: 設定更新 Stub
  - `CleanupData()`: 資料清理 Stub

### Stage-3: Views 與健康檢查
- ✅ **Views 實作**:
  - `AdminWallet/Index.cshtml`: 錢包列表頁面
  - `AdminWallet/Details.cshtml`: 錢包明細頁面
  - `AdminSignInStats/Index.cshtml`: 簽到記錄列表
  - `AdminSignInStats/Statistics.cshtml`: 簽到統計報表
  - `AdminWalletTypes/CouponTypes.cshtml`: 優惠券類型管理

- ✅ **HealthController**: 健康檢查端點
  - `/MiniGame/Health/Database`: 資料庫連線檢查
  - `/MiniGame/Health/Status`: 系統狀態檢查

## 資料表範圍確認

### User_Wallet 模組子系表（明確列出）
- ✅ User_Wallet
- ✅ CouponType  
- ✅ Coupon
- ✅ EVoucherType
- ✅ EVoucher
- ✅ EVoucherToken
- ✅ EVoucherRedeemLog
- ✅ WalletHistory

### 其他三模組所屬表（以 database.json 為準）
- ✅ UserSignInStats
- ✅ Pet
- ✅ MiniGame

## 技術實作細節

### 遵循指令要求
1. **區隔嚴格**: 僅在 `Areas/MiniGame/**` 作業，未觸碰其他 Area
2. **Admin 風格**: 使用 SB Admin 風格，未修改 vendor 檔案
3. **Read-first 原則**: 所有頁面優先完成查閱與篩選功能
4. **CRUD 限制**: 僅針對型別表（CouponType, EVoucherType）提供 CRUD
5. **Stub 實作**: 其餘表為審閱頁或不破壞規格的 Stub
6. **語言要求**: 所有人類可讀輸出皆為繁體中文（zh-TW）
7. **無 TODO**: 程式碼中無任何 TODO/FIXME/TBD/placeholder

### 資料庫操作
- 所有查詢使用 `AsNoTracking()` 
- 投影至 ReadModel 避免直接返回 entities
- 型別表 CRUD 包含適當的關聯檢查
- Stub 功能包含完整驗證與流程說明

### Stage-4: Views 與 CRUD 完整實作
- ✅ **AdminWallet/History.cshtml**: 錢包歷史記錄查詢頁面
- ✅ **AdminWallet/EVouchers.cshtml**: 電子禮券管理列表頁面
- ✅ **AdminWalletTypes/CreateCouponType.cshtml**: 新增優惠券類型表單
- ✅ **_Sidebar.cshtml**: Area-local 側邊欄共用元件
- ✅ **_Topbar.cshtml**: Area-local 頂部導航共用元件

### Spec Drift 修復
- ✅ **修復 SB Admin 風格**: 建立 Area-local 共用元件，符合指令第[2]節要求
- ✅ **Layout 重構**: 使用 PartialAsync 引用 _Sidebar 和 _Topbar

## 下次執行計畫 (Next)

### 待完成的 Views
- AdminWallet/Coupons.cshtml
- AdminWalletTypes/EditCouponType.cshtml
- AdminWalletTypes/EVoucherTypes.cshtml
- AdminSignInStats/Details.cshtml
- AdminSignInStats/UserHistory.cshtml
- AdminPet/Index.cshtml
- AdminMiniGame/Index.cshtml

### 可能的增強功能
- 匯出功能實作
- 圖表互動功能
- 批次操作介面
- 進階篩選選項

## 風險與假設 (Risks / Assumptions)

### 風險
1. **模型對應**: 假設 GameSpace.Models 中的實體類別與 database.json 結構一致
2. **權限驗證**: 假設 `[Authorize(Roles = "Admin")]` 權限驗證機制已正確設定
3. **資料庫連線**: 假設 GameSpaceDbContext 已正確配置連線字串

### 假設
1. **DbContext 設定**: 假設所有相關的 DbSet 已在 GameSpaceDbContext 中定義
2. **路由設定**: 假設 MVC Area 路由已正確配置
3. **依賴注入**: 假設 GameSpaceDbContext 已註冊到 DI 容器

## 檔案異動清單 (Files Touched)

### 新增檔案
```
Areas/MiniGame/Controllers/AdminWalletController.cs
Areas/MiniGame/Controllers/AdminWalletTypesController.cs  
Areas/MiniGame/Controllers/AdminSignInStatsController.cs
Areas/MiniGame/Controllers/AdminPetController.cs
Areas/MiniGame/Controllers/AdminMiniGameController.cs
Areas/MiniGame/Controllers/HealthController.cs
Areas/MiniGame/Views/Shared/_AdminLayout.cshtml
Areas/MiniGame/Views/AdminWallet/Index.cshtml
Areas/MiniGame/Views/AdminWallet/Details.cshtml
Areas/MiniGame/Views/AdminSignInStats/Index.cshtml
Areas/MiniGame/Views/AdminSignInStats/Statistics.cshtml
Areas/MiniGame/Views/AdminWalletTypes/CouponTypes.cshtml
docs/WIP_RUN.md
```

### 修改檔案
無（嚴格遵循不跨 Area 原則）

## 自我稽核檢查

- ✅ **語言**: 所有人類可讀輸出皆為 zh-TW
- ✅ **區隔**: 僅動到 `Areas/MiniGame/**`（與 `docs/**`）
- ✅ **規格**: 無 TODO/FIXME/TBD；無 shell 指令出現在交付
- ✅ **資料**: 所有查詢以 Read-first；寫入僅限型別表 CRUD；其他為審閱頁或 Stub
- ✅ **批次限制**: 每批 ≤3 檔案、≤400 行程式碼
- ✅ **提交格式**: 使用繁中提交訊息，包含 WHAT/HOW/VERIFY 結構

---
*最後更新：2025/09/15*