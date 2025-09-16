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
| UpdatedAt | datetime2 | 未使用 | - | 未使用 | 建議在更新操作中記錄 |

**覆蓋率**: 6/7 (85.7%)

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
| UpdatedAt | datetime2 | 未使用 | - | 未使用 | 建議在更新操作中記錄 |

**覆蓋率**: 6/7 (85.7%)

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
| RelatedID | int | 未使用 | - | 未使用 | 相關記錄ID，建議用於關聯追蹤 |

**覆蓋率**: 6/7 (85.7%)

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

| 欄位名稱 | 資料類型 | 使用位置 | 存取類型 | 使用狀態 | 備註 |
|---------|---------|----------|---------|----------|------|
| GameID | int | Areas/MiniGame/Controllers/MiniGameController.cs:34 | R | 已使用 | 主鍵 |
| GameName | nvarchar | Areas/MiniGame/Controllers/MiniGameController.cs:35 | R | 已使用 | 遊戲名稱 |
| Description | nvarchar | Areas/MiniGame/Controllers/MiniGameController.cs:36 | R | 已使用 | 遊戲描述 |
| IsActive | bit | Areas/MiniGame/Controllers/AdminMiniGameController.cs:67 | R/W | 已使用 | 啟用狀態 |
| CreatedAt | datetime2 | Areas/MiniGame/Views/MiniGame/Index.cshtml:89 | R | 已使用 | 建立時間 |
| UpdatedAt | datetime2 | Areas/MiniGame/Controllers/AdminMiniGameController.cs:89 | W | 已使用 | 更新時間 |
| GameRules | nvarchar(max) | Areas/MiniGame/Controllers/AdminMiniGameRulesController.cs:45 | R/W | 已使用 | 遊戲規則 |
| MaxPlayers | int | 未使用 | - | 未使用 | 最大玩家數，建議用於遊戲限制 |
| MinLevel | int | 未使用 | - | 未使用 | 最小等級要求，建議用於准入控制 |

**覆蓋率**: 7/9 (77.8%)

## 整體覆蓋率統計

| 資料表名稱 | 總欄位數 | 已使用欄位數 | 覆蓋率 | 狀態 |
|-----------|----------|------------|--------|------|
| User_Wallet | 4 | 4 | 100% | ✅ 優秀 |
| CouponType | 7 | 6 | 85.7% | ⚠️ 良好 |
| Coupon | 8 | 8 | 100% | ✅ 優秀 |
| EVoucherType | 7 | 6 | 85.7% | ⚠️ 良好 |
| EVoucher | 8 | 8 | 100% | ✅ 優秀 |
| WalletHistory | 7 | 6 | 85.7% | ⚠️ 良好 |
| UserSignInStats | 6 | 6 | 100% | ✅ 優秀 |
| Pet | 15 | 15 | 100% | ✅ 優秀 |
| MiniGame | 9 | 7 | 77.8% | ⚠️ 需改善 |

**總計**: 71個欄位中65個已使用，整體覆蓋率 **91.5%**

## 未使用欄位分析

### 需要修復的未使用欄位

1. **CouponType.UpdatedAt** - 建議在編輯優惠券類型時記錄更新時間
2. **EVoucherType.UpdatedAt** - 建議在編輯電子券類型時記錄更新時間  
3. **WalletHistory.RelatedID** - 建議用於關聯相關交易記錄
4. **MiniGame.MaxPlayers** - 建議用於遊戲人數限制
5. **MiniGame.MinLevel** - 建議用於遊戲准入等級控制

### 合理未使用的欄位
目前所有未使用欄位都應該被使用，沒有合理的未使用欄位。

## 建議改善措施

### 高優先級
1. 在CouponType和EVoucherType的更新操作中添加UpdatedAt欄位記錄
2. 在WalletHistory中使用RelatedID欄位建立交易關聯

### 中優先級  
3. 在MiniGame功能中使用MaxPlayers和MinLevel欄位實作遊戲限制

### 實作建議
- 所有讀取操作使用 `.AsNoTracking()` 
- 所有寫入操作包含在事務中
- 確保UTC時間存儲，UI顯示Asia/Taipei時區

---
**報告生成時間**: 2025-09-16  
**分析工具**: Phase 4.A DB欄位覆蓋率分析系統  