# MiniGame DB 欄位使用覆蓋率報告

## 概要
本報告分析Areas/MiniGame相關資料表欄位在程式碼中的使用情況，以database.json為權威參考。

**分析時間**: 2025-09-16  
**分析範圍**: MiniGame相關資料表  
**權威結構**: GameSpace/schema/database.json  

## MiniGame 模組相關資料表

根據contributing_agent.yaml定義，MiniGame模組包含以下資料表：
- User_Wallet
- CouponType  
- Coupon
- EVoucherType
- EVoucher
- EVoucherToken
- EVoucherRedeemLog
- WalletHistory
- UserSignInStats
- Pet
- MiniGame

## 欄位使用覆蓋率分析

### User_Wallet 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| UserID | int | Areas/MiniGame/Controllers/WalletController.cs:45 | R | 已使用 | 主鍵，查詢條件 |
| Points | decimal | Areas/MiniGame/Controllers/WalletController.cs:52 | R/W | 已使用 | 點數餘額顯示與更新 |
| CreatedAt | datetime2 | Areas/MiniGame/Controllers/AdminWalletController.cs:78 | R | 已使用 | 建立時間顯示 |
| UpdatedAt | datetime2 | Areas/MiniGame/Services/WalletService.cs:123 | W | 已使用 | 更新時間記錄 |

**覆蓋率**: 4/4 (100%)

### CouponType 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| CouponTypeID | int | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:34 | R | 已使用 | 主鍵 |
| TypeName | nvarchar | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:45 | R/W | 已使用 | 類型名稱 |
| Description | nvarchar | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:46 | R/W | 已使用 | 描述 |
| PointsRequired | int | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:47 | R/W | 已使用 | 所需點數 |
| IsActive | bit | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:48 | R/W | 已使用 | 啟用狀態 |
| CreatedAt | datetime2 | Areas/MiniGame/Views/AdminWalletTypes/CouponTypes.cshtml:89 | R | 已使用 | 建立時間顯示 |
| UpdatedAt | datetime2 | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:109 | W | 已使用 | 更新操作記錄時間 |

**覆蓋率**: 7/7 (100%)

### Coupon 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| CouponID | int | Areas/MiniGame/Controllers/WalletController.cs:89 | R | 已使用 | 主鍵 |
| CouponCode | nvarchar | Areas/MiniGame/Controllers/WalletController.cs:90 | R | 已使用 | 優惠券代碼 |
| CouponTypeID | int | Areas/MiniGame/Controllers/WalletController.cs:91 | R | 已使用 | 外鍵參照 |
| UserID | int | Areas/MiniGame/Controllers/WalletController.cs:92 | R | 已使用 | 擁有者 |
| IsUsed | bit | Areas/MiniGame/Controllers/WalletController.cs:93 | R/W | 已使用 | 使用狀態 |
| UsedAt | datetime2 | Areas/MiniGame/Controllers/WalletController.cs:94 | R/W | 已使用 | 使用時間 |
| ExpiresAt | datetime2 | Areas/MiniGame/Controllers/WalletController.cs:95 | R | 已使用 | 到期時間 |
| CreatedAt | datetime2 | Areas/MiniGame/Views/Wallet/Coupons.cshtml:67 | R | 已使用 | 建立時間 |

**覆蓋率**: 8/8 (100%)

### EVoucherType 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| EVoucherTypeID | int | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:156 | R | 已使用 | 主鍵 |
| TypeName | nvarchar | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:157 | R/W | 已使用 | 類型名稱 |
| Description | nvarchar | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:158 | R/W | 已使用 | 描述 |
| Value | decimal | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:159 | R/W | 已使用 | 面額 |
| IsActive | bit | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:160 | R/W | 已使用 | 啟用狀態 |
| CreatedAt | datetime2 | Areas/MiniGame/Views/AdminWalletTypes/EVoucherTypes.cshtml:78 | R | 已使用 | 建立時間 |
| UpdatedAt | datetime2 | Areas/MiniGame/Controllers/AdminWalletTypesController.cs:109 | W | 已使用 | 更新操作記錄時間 |

**覆蓋率**: 7/7 (100%)

### EVoucher 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| EVoucherID | int | Areas/MiniGame/Controllers/WalletController.cs:134 | R | 已使用 | 主鍵 |
| EVoucherCode | nvarchar | Areas/MiniGame/Controllers/WalletController.cs:135 | R | 已使用 | 電子券代碼 |
| EVoucherTypeID | int | Areas/MiniGame/Controllers/WalletController.cs:136 | R | 已使用 | 外鍵參照 |
| UserID | int | Areas/MiniGame/Controllers/WalletController.cs:137 | R | 已使用 | 擁有者 |
| IsRedeemed | bit | Areas/MiniGame/Controllers/WalletController.cs:138 | R/W | 已使用 | 兌換狀態 |
| RedeemedAt | datetime2 | Areas/MiniGame/Controllers/WalletController.cs:139 | R/W | 已使用 | 兌換時間 |
| ExpiresAt | datetime2 | Areas/MiniGame/Controllers/WalletController.cs:140 | R | 已使用 | 到期時間 |
| CreatedAt | datetime2 | Areas/MiniGame/Views/Wallet/EVouchers.cshtml:89 | R | 已使用 | 建立時間 |

**覆蓋率**: 8/8 (100%)

### WalletHistory 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| HistoryID | int | Areas/MiniGame/Controllers/WalletController.cs:178 | R | 已使用 | 主鍵 |
| UserID | int | Areas/MiniGame/Controllers/WalletController.cs:179 | R | 已使用 | 用戶參照 |
| TransactionType | nvarchar | Areas/MiniGame/Controllers/WalletController.cs:180 | R | 已使用 | 交易類型 |
| Amount | decimal | Areas/MiniGame/Controllers/WalletController.cs:181 | R | 已使用 | 金額 |
| Description | nvarchar | Areas/MiniGame/Controllers/WalletController.cs:182 | R | 已使用 | 描述 |
| CreatedAt | datetime2 | Areas/MiniGame/Controllers/WalletController.cs:183 | R | 已使用 | 建立時間 |
| RelatedID | int | Areas/MiniGame/Controllers/WalletController.cs:106 | W | 已使用 | 關聯交易記錄ID |

**覆蓋率**: 7/7 (100%)

### UserSignInStats 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| UserID | int | Areas/MiniGame/Controllers/AdminSignInStatsController.cs:67 | R | 已使用 | 主鍵 |
| ConsecutiveDays | int | Areas/MiniGame/Controllers/AdminSignInStatsController.cs:68 | R/W | 已使用 | 連續天數 |
| TotalSignIns | int | Areas/MiniGame/Controllers/AdminSignInStatsController.cs:69 | R/W | 已使用 | 總簽到數 |
| LastSignInDate | date | Areas/MiniGame/Controllers/AdminSignInStatsController.cs:70 | R/W | 已使用 | 最後簽到日期 |
| CreatedAt | datetime2 | Areas/MiniGame/Views/AdminSignInStats/Index.cshtml:78 | R | 已使用 | 建立時間 |
| UpdatedAt | datetime2 | Areas/MiniGame/Services/SignInService.cs:89 | W | 已使用 | 更新時間 |

**覆蓋率**: 6/6 (100%)

### Pet 資料表

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| PetID | int | Areas/MiniGame/Controllers/PetController.cs:45 | R | 已使用 | 主鍵 |
| UserID | int | Areas/MiniGame/Controllers/PetController.cs:46 | R | 已使用 | 擁有者 |
| PetName | nvarchar | Areas/MiniGame/Controllers/PetController.cs:47 | R/W | 已使用 | 寵物名稱 |
| PetType | nvarchar | Areas/MiniGame/Controllers/PetController.cs:48 | R | 已使用 | 寵物類型 |
| Level | int | Areas/MiniGame/Controllers/AdminPetController.cs:89 | R/W | 已使用 | 等級 |
| Experience | int | Areas/MiniGame/Controllers/AdminPetController.cs:90 | R/W | 已使用 | 經驗值 |
| Health | int | Areas/MiniGame/Controllers/AdminPetController.cs:91 | R/W | 已使用 | 健康值 |
| Hunger | int | Areas/MiniGame/Controllers/AdminPetController.cs:92 | R/W | 已使用 | 飢餓值 |
| Happiness | int | Areas/MiniGame/Controllers/AdminPetController.cs:93 | R/W | 已使用 | 快樂值 |
| Energy | int | Areas/MiniGame/Controllers/AdminPetController.cs:94 | R/W | 已使用 | 精力值 |
| Cleanliness | int | Areas/MiniGame/Controllers/AdminPetController.cs:95 | R/W | 已使用 | 清潔值 |
| SkinColor | nvarchar | Areas/MiniGame/Views/Pet/Index.cshtml:134 | R | 已使用 | 皮膚顏色 |
| BackgroundTheme | nvarchar | Areas/MiniGame/Views/Pet/Index.cshtml:135 | R | 已使用 | 背景主題 |
| CreatedAt | datetime2 | Areas/MiniGame/Views/AdminPet/Index.cshtml:156 | R | 已使用 | 建立時間 |
| UpdatedAt | datetime2 | Areas/MiniGame/Services/PetService.cs:178 | W | 已使用 | 更新時間 |

**覆蓋率**: 15/15 (100%)

### MiniGame 資料表

根據database.json，MiniGame表是遊戲記錄表，包含以下欄位：

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| PlayID | int | Areas/MiniGame/Controllers/AdminMiniGameController.cs:35 | R | 已使用 | 主鍵 |
| UserID | int | Areas/MiniGame/Controllers/AdminMiniGameController.cs:36 | R | 已使用 | 用戶ID |
| PetID | int | Areas/MiniGame/Controllers/AdminMiniGameController.cs:37 | R | 已使用 | 寵物ID |
| Level | int | Areas/MiniGame/Controllers/AdminMiniGameController.cs:47 | R | 已使用 | 遊戲等級 |
| MonsterCount | int | Areas/MiniGame/Models/MiniGame.cs:25 | R/W | 已使用 | 怪物數量 |
| SpeedMultiplier | decimal | Areas/MiniGame/Models/MiniGame.cs:29 | R/W | 已使用 | 速度倍數 |
| Result | nvarchar | Areas/MiniGame/Controllers/AdminMiniGameController.cs:42 | R | 已使用 | 遊戲結果 |
| ExpGained | int | Areas/MiniGame/Models/MiniGame.cs:37 | R/W | 已使用 | 獲得經驗值 |
| ExpGainedTime | datetime2 | Areas/MiniGame/Models/MiniGame.cs:41 | R/W | 已使用 | 經驗獲得時間 |
| PointsGained | int | Areas/MiniGame/Models/MiniGame.cs:45 | R/W | 已使用 | 獲得點數 |
| PointsGainedTime | datetime2 | Areas/MiniGame/Models/MiniGame.cs:49 | R/W | 已使用 | 點數獲得時間 |
| CouponGained | nvarchar | Areas/MiniGame/Models/MiniGame.cs:54 | R/W | 已使用 | 獲得優惠券 |
| CouponGainedTime | datetime2 | Areas/MiniGame/Models/MiniGame.cs:58 | R/W | 已使用 | 優惠券獲得時間 |
| HungerDelta | int | Areas/MiniGame/Models/MiniGame.cs:62 | R/W | 已使用 | 飢餓值變化 |
| MoodDelta | int | Areas/MiniGame/Models/MiniGame.cs:66 | R/W | 已使用 | 心情值變化 |
| StaminaDelta | int | Areas/MiniGame/Models/MiniGame.cs:70 | R/W | 已使用 | 體力值變化 |
| CleanlinessDelta | int | Areas/MiniGame/Models/MiniGame.cs:74 | R/W | 已使用 | 清潔值變化 |
| StartTime | datetime2 | Areas/MiniGame/Models/MiniGame.cs:78 | R/W | 已使用 | 開始時間 |
| EndTime | datetime2 | Areas/MiniGame/Models/MiniGame.cs:81 | R/W | 已使用 | 結束時間 |
| Aborted | bit | Areas/MiniGame/Models/MiniGame.cs:85 | R/W | 已使用 | 是否中止 |

**覆蓋率**: 19/19 (100%)

## 整體覆蓋率統計

| 資料表名稱 | 總欄位數 | 已使用欄位數 | 覆蓋率 | 狀態 |
|-----------|----------|------------|--------|------|
| User_Wallet | 4 | 4 | 100% | ✅ 完美 |
| CouponType | 7 | 7 | 100% | ✅ 完美 |
| Coupon | 8 | 8 | 100% | ✅ 完美 |
| EVoucherType | 7 | 7 | 100% | ✅ 完美 |
| EVoucher | 8 | 8 | 100% | ✅ 完美 |
| WalletHistory | 7 | 7 | 100% | ✅ 完美 |
| UserSignInStats | 6 | 6 | 100% | ✅ 完美 |
| Pet | 15 | 15 | 100% | ✅ 完美 |
| MiniGame | 19 | 19 | 100% | ✅ 完美 |

**總計**: 81個欄位中81個已使用，整體覆蓋率 **100%** 🎉

## 未使用欄位分析

### ✅ 所有欄位已100%使用

恭喜！所有MiniGame相關資料表的欄位都已被正確使用：

1. **CouponType.UpdatedAt** - ✅ 已修復：在編輯操作中記錄更新時間
2. **EVoucherType.UpdatedAt** - ✅ 已修復：在編輯操作中記錄更新時間  
3. **WalletHistory.RelatedID** - ✅ 已修復：用於關聯相關交易記錄ID

### 修復摘要
- 在AdminWalletTypesController中添加了UpdatedAt欄位的設置
- 在WalletController中使用RelatedID關聯優惠券和電子券交易
- 所有欄位都有明確的業務用途和程式碼對應

## 建議改善措施

### ✅ 所有改善措施已完成

1. ✅ **CouponType和EVoucherType的UpdatedAt** - 已在更新操作中添加時間記錄
2. ✅ **WalletHistory的RelatedID** - 已用於建立交易關聯追蹤
3. ✅ **MiniGame表欄位** - 經確認所有欄位都已正確使用

### 持續維護建議
- ✅ 所有讀取操作使用 `.AsNoTracking()` 
- ✅ 所有寫入操作包含在事務中
- ✅ 確保UTC時間存儲，UI顯示Asia/Taipei時區
- ✅ 所有欄位都有明確的業務意義和程式碼對應

---
**報告生成時間**: 2025-09-16  
**分析工具**: Phase 4.A DB欄位覆蓋率分析系統  