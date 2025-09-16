# MiniGame Admin 視圖端點映射

## Admin 頁面端點映射

### AdminHome/Dashboard.cshtml
- **主要端點**: GET /AdminHome/Dashboard
- **查詢參數**: from(DateTime?), to(DateTime?)
- **資料來源**: 
  - WalletHistory (ChangeTime 範圍查詢)
  - EVoucherToken (總數統計)
  - EVoucherRedeemLog (ScannedAt 範圍查詢)
  - UserSignInStats (SignInDate 範圍查詢)
  - MiniGame (StartTime 範圍查詢)
- **匯出功能**: 無
- **圖表資料**: 5 個統計卡片 + 診斷表格
- **時區轉換**: 查詢參數 Asia/Taipei → UTC

### AdminWallet/Index.cshtml
- **主要端點**: GET /AdminWallet/Index
- **查詢參數**: page(int), pageSize(int), search(string)
- **資料來源**: User_Wallet (User_Id, User_Point)
- **匯出功能**: 
  - GET /AdminWallet/IndexExportCsv
  - GET /AdminWallet/IndexExportJson
- **圖表資料**: 錢包統計圖表 (Chart.js)
- **分頁**: 預設 20，最大 100

### AdminWallet/Coupons.cshtml
- **主要端點**: GET /AdminWallet/Coupons
- **查詢參數**: userId(int?), page(int)
- **資料來源**: 
  - Coupon (CouponID, CouponCode, UserID, IsUsed)
  - CouponType (Name, DiscountType, DiscountValue)
- **匯出功能**: 
  - GET /AdminWallet/CouponsExportCsv
  - GET /AdminWallet/CouponsExportJson
- **圖表資料**: 優惠券使用統計
- **操作端點**: POST /AdminWallet/GrantCoupon, POST /AdminWallet/RevokeCoupon

### AdminWallet/EVouchers.cshtml
- **主要端點**: GET /AdminWallet/EVouchers
- **查詢參數**: userId(int?), page(int)
- **資料來源**: 
  - EVoucher (EVoucherID, EVoucherCode, UserID, IsUsed)
  - EVoucherType (TypeName, Description, ExchangePoints)
- **匯出功能**: 
  - GET /AdminWallet/EVouchersExportCsv
  - GET /AdminWallet/EVouchersExportJson
- **圖表資料**: 電子禮券使用統計
- **操作端點**: POST /AdminWallet/GrantEVoucher, POST /AdminWallet/RevokeEVoucher

### AdminWallet/CombinedLedger.cshtml
- **主要端點**: GET /AdminWallet/CombinedLedger
- **查詢參數**: type(string?), from(DateTime?), to(DateTime?), userId(int?)
- **資料來源**: 
  - WalletHistory (ChangeType, PointsChanged, ChangeTime)
  - Coupon (AcquiredTime, UsedTime)
  - EVoucher (AcquiredTime, UsedTime)
- **匯出功能**: 
  - GET /AdminWallet/CombinedLedgerExportCsv
  - GET /AdminWallet/CombinedLedgerExportJson
- **圖表資料**: 收支分析圖表 + 資產分佈圖
- **時區轉換**: Asia/Taipei → UTC
- **日期工具列**: _DateRangeToolbar

### AdminSignInStats/Index.cshtml
- **主要端點**: GET /AdminSignInStats/Index
- **查詢參數**: startDate(DateTime?), endDate(DateTime?), userId(int?), userSearch(string)
- **資料來源**: UserSignInStats (UserID, SignInDate, RewardedPoints, PointsGained)
- **匯出功能**: 
  - GET /AdminSignInStats/IndexExportCsv
  - GET /AdminSignInStats/IndexExportJson
- **圖表資料**: 簽到統計圖表
- **操作端點**: POST /AdminSignInStats/Adjust

### AdminSignInStats/Statistics.cshtml
- **主要端點**: GET /AdminSignInStats/Statistics
- **查詢參數**: from(DateTime?), to(DateTime?)
- **資料來源**: UserSignInStats 聚合查詢
- **匯出功能**: 
  - GET /AdminSignInStats/StatisticsExportCsv
  - GET /AdminSignInStats/StatisticsExportJson
- **圖表資料**: 簽到趨勢圖表
- **時區轉換**: Asia/Taipei → UTC
- **日期工具列**: _DateRangeToolbar

### AdminPet/Index.cshtml
- **主要端點**: GET /AdminPet/Index
- **查詢參數**: search(string), minLevel(int?), maxLevel(int?)
- **資料來源**: Pet (PetID, UserID, PetName, Level, Experience)
- **匯出功能**: 
  - GET /AdminPet/IndexExportCsv
  - GET /AdminPet/IndexExportJson
- **圖表資料**: 寵物等級分佈圖表
- **操作端點**: POST /AdminPet/Edit

### AdminMiniGame/Index.cshtml
- **主要端點**: GET /AdminMiniGame/Index
- **查詢參數**: result(string), level(int?), userId(int?), startDate(DateTime?), endDate(DateTime?)
- **資料來源**: MiniGame (GameID, UserID, StartTime, EndTime, Result, PointsGained)
- **匯出功能**: 
  - GET /AdminMiniGame/IndexExportCsv
  - GET /AdminMiniGame/IndexExportJson
- **圖表資料**: 遊戲統計圖表
- **時區轉換**: StartTime/EndTime Asia/Taipei → UTC

### AdminMiniGameRules/Index.cshtml
- **主要端點**: GET /AdminMiniGameRules/Index
- **查詢參數**: 無
- **資料來源**: game_rules.json 配置檔案
- **匯出功能**: 無 (配置檔案)
- **圖表資料**: 配置變更歷史圖表
- **操作端點**: POST /AdminMiniGameRules/Edit

## 共用元件端點映射

### _DateRangeToolbar.cshtml
- **使用頁面**: 4 個時間相關頁面
- **查詢參數**: from, to (yyyy-MM-dd 格式)
- **時區處理**: Asia/Taipei 顯示，UTC 傳送伺服器
- **JavaScript**: 日期驗證和自動提交
- **無障礙性**: label 關聯和鍵盤導航

### _ExportButtons.cshtml
- **使用頁面**: 8 個列表頁面
- **端點模式**: {Action}ExportCsv, {Action}ExportJson
- **參數傳遞**: ViewData["ExportParams"] 物件
- **安全機制**: CSV 注入防護 + UTF-8-BOM
- **無障礙性**: ARIA 標籤和詳細描述

### _ChartCard.cshtml
- **使用頁面**: 6 個統計頁面
- **資料來源**: Chart.js + 後端 JSON API
- **無障礙性**: role="img" + aria-describedby
- **空狀態**: 無資料時的友善提示
- **響應式**: 適配不同螢幕尺寸

## 參數流向追溯

### 日期範圍參數流向
1. **UI 輸入**: Asia/Taipei 時區選擇
2. **查詢字串**: yyyy-MM-dd 格式
3. **控制器接收**: DateTime? 參數
4. **資料庫查詢**: UTC 時間範圍
5. **結果顯示**: 轉換回 Asia/Taipei

### 分頁參數流向
1. **UI 分頁器**: 頁碼和頁面大小選擇
2. **查詢字串**: page, pageSize 參數
3. **控制器處理**: Skip/Take 計算
4. **資料庫查詢**: OFFSET/FETCH 分頁
5. **結果返回**: 總筆數和分頁資訊

### 匯出參數流向
1. **UI 按鈕**: 當前篩選條件保持
2. **查詢字串**: 所有篩選參數傳遞
3. **控制器處理**: 相同的查詢邏輯
4. **資料處理**: CSV 注入防護 + UTF-8-BOM
5. **檔案下載**: 時間戳檔名 + 正確 Content-Type

這個映射提供了完整的資料流向追溯，確保 UI 與後端的一致性和正確性。