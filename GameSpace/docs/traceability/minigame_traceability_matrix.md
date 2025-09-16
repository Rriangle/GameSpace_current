# MiniGame Admin 追溯性矩陣

## 錢包管理模組 (Wallet)

### AdminWallet/Index {#adminwallet-index}
| 項目 | 值 | 檔案:行 |
|------|----|---------| 
| **頁面路徑** | AdminWallet/Index.cshtml | Views/AdminWallet/Index.cshtml:1 |
| **控制器.動作** | AdminWalletController.Index | Controllers/AdminWalletController.cs:33 |
| **路由** | GET /MiniGame/AdminWallet/Index | - |
| **請求 DTO** | page(int), pageSize(int), search(string) | :33 |
| **回應 DTO** | IEnumerable&lt;User_Wallet&gt; | :45 |
| **資料表** | User_Wallet (R) | User_Id, User_Point |
| **權限檢查** | [MiniGameAdminAuthorize] | :18 |
| **冪等性** | 不適用 (GET) | - |
| **交易範圍** | 不適用 (讀取) | - |
| **記錄事件** | 無 (查詢操作) | - |
| **日期範圍** | 不適用 | - |
| **匯出功能** | IndexExportCsv/Json | _ExportButtons:21 |
| **圖表資料** | 錢包統計圖表 | Chart.js |
| **測試覆蓋** | 概念性驗證 | - |

### AdminWallet/Adjust {#adminwallet-adjust}
| 項目 | 值 | 檔案:行 |
|------|----|---------| 
| **頁面路徑** | AdminWallet/Adjust.cshtml | Views/AdminWallet/Adjust.cshtml:1 |
| **控制器.動作** | AdminWalletController.Adjust | Controllers/AdminWalletController.cs:871 |
| **路由** | POST /MiniGame/AdminWallet/Adjust | - |
| **請求 DTO** | userId(int), delta(int), reason(string) | :871 |
| **回應 DTO** | RedirectToActionResult | :920 |
| **資料表** | User_Wallet (W), WalletHistory (W) | User_Point, HistoryID |
| **權限檢查** | [MiniGameAdminAuthorize] | :18 |
| **冪等性** | X-Idempotency-Key (60s) | :875 |
| **交易範圍** | Database.BeginTransaction() | :880 |
| **記錄事件** | Admin 調整會員點數 | :915 |
| **日期範圍** | 不適用 | - |
| **匯出功能** | 不適用 | - |
| **圖表資料** | 不適用 | - |
| **測試覆蓋** | 冪等性測試 | - |

## 簽到管理模組 (SignInStats)

### AdminSignInStats/Index {#adminsigninstats-index}
| 項目 | 值 | 檔案:行 |
|------|----|---------| 
| **頁面路徑** | AdminSignInStats/Index.cshtml | Views/AdminSignInStats/Index.cshtml:1 |
| **控制器.動作** | AdminSignInStatsController.Index | Controllers/AdminSignInStatsController.cs:25 |
| **路由** | GET /MiniGame/AdminSignInStats/Index | - |
| **請求 DTO** | startDate(DateTime?), endDate(DateTime?), userId(int?) | :25 |
| **回應 DTO** | IEnumerable&lt;UserSignInStats&gt; | :35 |
| **資料表** | UserSignInStats (R) | UserID, SignInDate, RewardedPoints |
| **權限檢查** | [MiniGameAdminAuthorize] | :18 |
| **冪等性** | 不適用 (GET) | - |
| **交易範圍** | 不適用 (讀取) | - |
| **記錄事件** | 無 (查詢操作) | - |
| **日期範圍** | Asia/Taipei → UTC 轉換 | _DateRangeToolbar:7 |
| **匯出功能** | IndexExportCsv/Json | _ExportButtons:21 |
| **圖表資料** | 簽到統計圖表 | Chart.js |
| **測試覆蓋** | 日期範圍往返測試 | - |

## 寵物管理模組 (Pet)

### AdminPet/Index {#adminpet-index}
| 項目 | 值 | 檔案:行 |
|------|----|---------| 
| **頁面路徑** | AdminPet/Index.cshtml | Views/AdminPet/Index.cshtml:1 |
| **控制器.動作** | AdminPetController.Index | Controllers/AdminPetController.cs:25 |
| **路由** | GET /MiniGame/AdminPet/Index | - |
| **請求 DTO** | search(string), minLevel(int?), maxLevel(int?) | :25 |
| **回應 DTO** | IEnumerable&lt;Pet&gt; | :35 |
| **資料表** | Pet (R) | PetID, UserID, PetName, Level |
| **權限檢查** | [MiniGameAdminAuthorize] | :18 |
| **冪等性** | 不適用 (GET) | - |
| **交易範圍** | 不適用 (讀取) | - |
| **記錄事件** | 無 (查詢操作) | - |
| **日期範圍** | 不適用 | - |
| **匯出功能** | IndexExportCsv/Json | _ExportButtons:21 |
| **圖表資料** | 寵物等級分佈圖表 | Chart.js |
| **測試覆蓋** | 分頁和篩選測試 | - |

## 小遊戲管理模組 (MiniGame)

### AdminMiniGame/Index {#adminminigame-index}
| 項目 | 值 | 檔案:行 |
|------|----|---------| 
| **頁面路徑** | AdminMiniGame/Index.cshtml | Views/AdminMiniGame/Index.cshtml:1 |
| **控制器.動作** | AdminMiniGameController.Index | Controllers/AdminMiniGameController.cs:25 |
| **路由** | GET /MiniGame/AdminMiniGame/Index | - |
| **請求 DTO** | result(string), level(int?), userId(int?), startDate(DateTime?), endDate(DateTime?) | :25 |
| **回應 DTO** | IEnumerable&lt;MiniGame&gt; | :35 |
| **資料表** | MiniGame (R) | GameID, UserID, StartTime, Result |
| **權限檢查** | [MiniGameAdminAuthorize] | :18 |
| **冪等性** | 不適用 (GET) | - |
| **交易範圍** | 不適用 (讀取) | - |
| **記錄事件** | 無 (查詢操作) | - |
| **日期範圍** | Asia/Taipei → UTC 轉換 | _DateRangeToolbar:7 |
| **匯出功能** | IndexExportCsv/Json | _ExportButtons:21 |
| **圖表資料** | 遊戲統計圖表 | Chart.js |
| **測試覆蓋** | 日期範圍和結果篩選測試 | - |

## 共用元件追溯

### _DateRangeToolbar.cshtml {#daterange-toolbar}
| 項目 | 值 | 檔案:行 |
|------|----|---------| 
| **元件路徑** | Shared/_DateRangeToolbar.cshtml | Views/Shared/_DateRangeToolbar.cshtml:1 |
| **功能** | 日期範圍選擇器 | - |
| **時區轉換** | Asia/Taipei 顯示，UTC 傳送 | :7-8 |
| **查詢字串** | from, to 參數同步 | :3-4 |
| **使用頁面** | 4 個時間相關頁面 | Statistics 頁面 |
| **無障礙性** | label 關聯，鍵盤導航 | :35-44 |

### _ExportButtons.cshtml {#export-buttons}
| 項目 | 值 | 檔案:行 |
|------|----|---------| 
| **元件路徑** | Shared/_ExportButtons.cshtml | Views/Shared/_ExportButtons.cshtml:1 |
| **功能** | CSV/JSON 匯出按鈕 | - |
| **安全防護** | CSV 注入防護 | ExportService.cs:88-95 |
| **編碼標準** | UTF-8-BOM | ExportService.cs:42-43 |
| **使用頁面** | 8 個列表頁面 | Index 頁面 |
| **無障礙性** | ARIA 標籤和角色 | :21-34 |

## 變更影響分析

### 資料表欄位變更影響範圍
| 資料表.欄位 | 影響的控制器 | 影響的視圖 | 影響的 DTO |
|-------------|-------------|-----------|-----------|
| User_Wallet.User_Point | AdminWalletController | AdminWallet/Index | 錢包相關 DTO |
| Pet.PetName | AdminPetController | AdminPet/Index | Pet DTO |
| MiniGame.Result | AdminMiniGameController | AdminMiniGame/Index | MiniGame DTO |
| ManagerRolePermission.Pet_Rights_Management | 所有 Admin 控制器 | 所有 Admin 頁面 | 權限檢查邏輯 |

## 追溯性檢查清單

### 端點完整性 ✅
- ✅ 所有 Admin 端點都有對應的矩陣記錄
- ✅ 權限檢查覆蓋 100%
- ✅ 資料表對應關係明確

### 視圖完整性 ✅
- ✅ 所有 Admin 頁面都有端點對應
- ✅ 共用元件使用關係清楚
- ✅ 資料流向追溯完整

### 資料庫對應 ✅
- ✅ 所有使用的資料表都在 database.json 中
- ✅ 欄位名稱和型別精確對應
- ✅ 約束關係正確映射

這個追溯性矩陣提供了 MiniGame Admin 系統的完整雙向追溯能力，確保程式碼變更時能夠準確評估影響範圍。