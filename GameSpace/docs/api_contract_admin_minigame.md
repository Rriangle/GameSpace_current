# MiniGame Admin API 合約規範

## API 設計原則

### 認證與授權
- **基礎認證**: `[Authorize(Roles = "Admin")]`
- **權限檢查**: `[MiniGameAdminAuthorize]` 檢查 Pet_Rights_Management
- **無權限處理**: 返回 403 Forbidden 或 NoPermission 頁面

### 冪等性要求
- **適用方法**: POST/PUT/PATCH/DELETE
- **必需 Header**: `X-Idempotency-Key` (UUID 格式建議)
- **防重時間**: 60秒內相同金鑰拒絕重複請求
- **回應**: 409 Conflict 當檢測到重複請求

### 錯誤處理標準
- **成功回應**: 200 OK, 201 Created, 204 No Content
- **客戶端錯誤**: 400 Bad Request, 401 Unauthorized, 403 Forbidden, 409 Conflict
- **伺服器錯誤**: 500 Internal Server Error
- **錯誤格式**: RFC 7807 ProblemDetails (zh-TW 訊息)

## Admin 端點規範

### 會員點數管理 (AdminWallet)

#### 查詢會員點數
```
GET /MiniGame/AdminWallet/Index?userId={userId}&search={search}&page={page}
Authorization: Admin + Pet_Rights_Management
Response: 200 OK (HTML) 或 JSON
```

#### 調整會員點數
```
POST /MiniGame/AdminWallet/Adjust
Headers: X-Idempotency-Key, X-RequestVerificationToken
Body: { userId: int, delta: int, reason: string }
Authorization: Admin + Pet_Rights_Management
Transaction: Required
Response: 200 OK 或 409 Conflict (重複) 或 400 Bad Request (驗證失敗)
```

#### 發放優惠券
```
POST /MiniGame/AdminWallet/GrantCoupon
Headers: X-Idempotency-Key, X-RequestVerificationToken
Body: { userId: int, couponTypeId: int, count: int, reason: string }
Authorization: Admin + Pet_Rights_Management
Transaction: Required
Response: 200 OK 或 409 Conflict 或 400 Bad Request
```

### 簽到管理 (AdminSignInStats)

#### 查詢簽到記錄
```
GET /MiniGame/AdminSignInStats/Index?startDate={date}&endDate={date}&userId={userId}
Authorization: Admin + Pet_Rights_Management
Response: 200 OK (HTML) 或 JSON
```

#### 手動調整簽到
```
POST /MiniGame/AdminSignInStats/Adjust
Headers: X-Idempotency-Key, X-RequestVerificationToken
Body: { action: string, userId: int, reason: string }
Authorization: Admin + Pet_Rights_Management
Transaction: Required
Response: 200 OK 或 409 Conflict 或 400 Bad Request
```

### 寵物管理 (AdminPet)

#### 查詢寵物清單
```
GET /MiniGame/AdminPet/Index?search={search}&minLevel={level}&maxLevel={level}
Authorization: Admin + Pet_Rights_Management
Response: 200 OK (HTML) 或 JSON
```

#### 編輯寵物設定
```
POST /MiniGame/AdminPet/Edit
Headers: X-Idempotency-Key, X-RequestVerificationToken
Body: { id: int, petName: string, level: int, experience: int, ... }
Authorization: Admin + Pet_Rights_Management
Transaction: Required
Response: 200 OK 或 409 Conflict 或 400 Bad Request
```

### 小遊戲管理 (AdminMiniGame)

#### 查詢遊戲記錄
```
GET /MiniGame/AdminMiniGame/Index?result={result}&level={level}&userId={userId}&startDate={date}&endDate={date}
Authorization: Admin + Pet_Rights_Management
Response: 200 OK (HTML) 或 JSON
```

#### 更新遊戲規則
```
POST /MiniGame/AdminMiniGameRules/Edit
Headers: X-Idempotency-Key, X-RequestVerificationToken
Body: GameRulesOptions (JSON)
Authorization: Admin + Pet_Rights_Management
Transaction: File Lock
Response: 200 OK 或 409 Conflict 或 400 Bad Request
```

## 資料匯出規範

### CSV 匯出
- **編碼**: UTF-8 with BOM
- **檔名**: `{module}_{yyyyMMdd_HHmmss}.csv`
- **安全性**: 值開頭為 =,+,-,@ 時加上單引號前綴
- **Content-Type**: `text/csv; charset=utf-8`
- **Content-Disposition**: `attachment; filename="{filename}"`

### JSON 匯出
- **編碼**: UTF-8
- **檔名**: `{module}_{yyyyMMdd_HHmmss}.json`
- **格式**: 標準 JSON 陣列
- **Content-Type**: `application/json; charset=utf-8`
- **Content-Disposition**: `attachment; filename="{filename}"`

## 驗證規則

### 通用驗證
- **必填欄位**: 不可為 null 或空字串
- **長度限制**: 依據 database.json 欄位定義
- **數值範圍**: 正整數、合理的日期範圍
- **字元集**: 中文、英文、數字、基本符號

### 業務驗證
- **會員存在性**: 操作前驗證 UserId 存在
- **權限範圍**: 僅能操作有權限的資料
- **狀態一致性**: 避免無效的狀態轉換
- **業務規則**: 遵循遊戲規則和限制

## 監控與記錄

### 必需記錄事件
- **權限檢查**: 成功/失敗的權限驗證
- **資料變更**: 變更前後的關鍵欄位值
- **錯誤事件**: 異常和驗證失敗
- **效能指標**: 查詢執行時間和資源使用

### 記錄格式
```json
{
  "timestamp": "2024-01-01T00:00:00Z",
  "level": "Information",
  "messageTemplate": "Admin 調整會員點數: UserId={UserId}, Delta={Delta}, Reason={Reason}, TraceID={TraceID}",
  "properties": {
    "UserId": 123,
    "Delta": 100,
    "Reason": "活動獎勵",
    "TraceID": "abc123",
    "SourceContext": "GameSpace.Areas.MiniGame.Controllers.AdminWalletController"
  }
}
```

## 合規性檢查清單

- ✅ 所有 Admin 端點有權限檢查
- ✅ 所有讀取查詢使用 AsNoTracking
- ✅ 所有寫入操作使用 Transaction
- ⚠️ 冪等性 header 檢查 (待實作)
- ✅ Serilog 審計記錄完整
- ✅ CSRF 保護覆蓋所有 POST
- ✅ zh-TW 本地化完整
- ✅ 資料對應精確符合 database.json