# SSMS 資料庫建立與連線字串設定指南

## 概要
本指南說明如何在 SQL Server Management Studio (SSMS) 中建立資料庫、匯入種子資料，以及設定應用程式連線字串。

**重要原則**: 
- 不使用 EF Migrations
- database.json 為唯一資料結構權威
- 所有資料表手動建立或透過腳本建立

## 步驟 1: 在 SSMS 中建立資料庫

### 1.1 連線到 SQL Server
1. 開啟 SQL Server Management Studio (SSMS)
2. 連線到你的 SQL Server 實例
   - 伺服器名稱: `localhost\SQLEXPRESS` (本機) 或遠端伺服器位址
   - 驗證方式: Windows 驗證 或 SQL Server 驗證
   - 如使用 SQL 驗證，輸入使用者名稱和密碼

### 1.2 建立資料庫
```sql
-- 建立 GameSpaceDb 資料庫
CREATE DATABASE GameSpaceDb
COLLATE Chinese_Taiwan_Stroke_CI_AS;

-- 切換到新建立的資料庫
USE GameSpaceDb;
```

## 步驟 2: 根據 database.json 建立資料表

### 2.1 建立 MiniGame 相關資料表

**重要**: 請嚴格按照 `GameSpace/schema/database.json` 中的定義建立資料表，包括：
- 欄位名稱和資料類型
- 主鍵和外鍵約束
- 唯一鍵約束
- 預設值
- CHECK 約束

#### User_Wallet 資料表
```sql
CREATE TABLE [User_Wallet] (
    [UserID] int NOT NULL PRIMARY KEY,
    [Points] decimal(18,2) NOT NULL DEFAULT 0,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE()
);
```

#### CouponType 資料表
```sql
CREATE TABLE [CouponType] (
    [CouponTypeID] int IDENTITY(1,1) PRIMARY KEY,
    [TypeName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [PointsRequired] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE()
);
```

#### Coupon 資料表
```sql
CREATE TABLE [Coupon] (
    [CouponID] int IDENTITY(1,1) PRIMARY KEY,
    [CouponCode] nvarchar(50) NOT NULL UNIQUE,
    [CouponTypeID] int NOT NULL,
    [UserID] int NOT NULL,
    [IsUsed] bit NOT NULL DEFAULT 0,
    [UsedAt] datetime2(7) NULL,
    [ExpiresAt] datetime2(7) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([CouponTypeID]) REFERENCES [CouponType]([CouponTypeID]),
    FOREIGN KEY ([UserID]) REFERENCES [User_Wallet]([UserID])
);
```

#### EVoucherType 資料表
```sql
CREATE TABLE [EVoucherType] (
    [EVoucherTypeID] int IDENTITY(1,1) PRIMARY KEY,
    [TypeName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Value] decimal(18,2) NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE()
);
```

#### EVoucher 資料表
```sql
CREATE TABLE [EVoucher] (
    [EVoucherID] int IDENTITY(1,1) PRIMARY KEY,
    [EVoucherCode] nvarchar(50) NOT NULL UNIQUE,
    [EVoucherTypeID] int NOT NULL,
    [UserID] int NOT NULL,
    [IsRedeemed] bit NOT NULL DEFAULT 0,
    [RedeemedAt] datetime2(7) NULL,
    [ExpiresAt] datetime2(7) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([EVoucherTypeID]) REFERENCES [EVoucherType]([EVoucherTypeID]),
    FOREIGN KEY ([UserID]) REFERENCES [User_Wallet]([UserID])
);
```

#### EVoucherToken 資料表
```sql
CREATE TABLE [EVoucherToken] (
    [TokenID] int IDENTITY(1,1) PRIMARY KEY,
    [EVoucherID] int NOT NULL,
    [TokenValue] nvarchar(255) NOT NULL UNIQUE,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([EVoucherID]) REFERENCES [EVoucher]([EVoucherID])
);
```

#### EVoucherRedeemLog 資料表
```sql
CREATE TABLE [EVoucherRedeemLog] (
    [LogID] int IDENTITY(1,1) PRIMARY KEY,
    [EVoucherID] int NOT NULL,
    [UserID] int NOT NULL,
    [RedeemedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    [RedemptionMethod] nvarchar(50) NULL,
    [Notes] nvarchar(500) NULL,
    FOREIGN KEY ([EVoucherID]) REFERENCES [EVoucher]([EVoucherID]),
    FOREIGN KEY ([UserID]) REFERENCES [User_Wallet]([UserID])
);
```

#### WalletHistory 資料表
```sql
CREATE TABLE [WalletHistory] (
    [HistoryID] int IDENTITY(1,1) PRIMARY KEY,
    [UserID] int NOT NULL,
    [TransactionType] nvarchar(100) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NULL,
    [RelatedID] int NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserID]) REFERENCES [User_Wallet]([UserID])
);
```

#### UserSignInStats 資料表
```sql
CREATE TABLE [UserSignInStats] (
    [UserID] int NOT NULL PRIMARY KEY,
    [ConsecutiveDays] int NOT NULL DEFAULT 0,
    [TotalSignIns] int NOT NULL DEFAULT 0,
    [LastSignInDate] date NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserID]) REFERENCES [User_Wallet]([UserID])
);
```

#### Pet 資料表
```sql
CREATE TABLE [Pet] (
    [PetID] int IDENTITY(1,1) PRIMARY KEY,
    [UserID] int NOT NULL,
    [PetName] nvarchar(100) NOT NULL,
    [PetType] nvarchar(50) NOT NULL,
    [Level] int NOT NULL DEFAULT 1,
    [Experience] int NOT NULL DEFAULT 0,
    [Health] int NOT NULL DEFAULT 100,
    [Hunger] int NOT NULL DEFAULT 100,
    [Happiness] int NOT NULL DEFAULT 100,
    [Energy] int NOT NULL DEFAULT 100,
    [Cleanliness] int NOT NULL DEFAULT 100,
    [SkinColor] nvarchar(20) NOT NULL DEFAULT '#FFFFFF',
    [BackgroundTheme] nvarchar(50) NOT NULL DEFAULT 'default',
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserID]) REFERENCES [User_Wallet]([UserID])
);
```

#### MiniGame 資料表
```sql
CREATE TABLE [MiniGame] (
    [GameID] int IDENTITY(1,1) PRIMARY KEY,
    [GameName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [GameRules] nvarchar(max) NULL,
    [MaxPlayers] int NOT NULL DEFAULT 1,
    [MinLevel] int NOT NULL DEFAULT 1,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE()
);
```

### 2.2 建立索引（效能優化）
```sql
-- 常用查詢索引
CREATE INDEX IX_Coupon_UserID ON [Coupon]([UserID]);
CREATE INDEX IX_Coupon_CouponTypeID ON [Coupon]([CouponTypeID]);
CREATE INDEX IX_EVoucher_UserID ON [EVoucher]([UserID]);
CREATE INDEX IX_EVoucher_EVoucherTypeID ON [EVoucher]([EVoucherTypeID]);
CREATE INDEX IX_WalletHistory_UserID ON [WalletHistory]([UserID]);
CREATE INDEX IX_WalletHistory_CreatedAt ON [WalletHistory]([CreatedAt]);
CREATE INDEX IX_Pet_UserID ON [Pet]([UserID]);
```

## 步驟 3: 匯入種子資料

### 3.1 使用 OPENJSON 匯入 seedMiniGameArea.json

```sql
-- 讀取並匯入種子資料的範例
DECLARE @json NVARCHAR(MAX)

-- 方法 1: 如果檔案在伺服器可存取位置
SELECT @json = BulkColumn
FROM OPENROWSET (BULK 'C:\path\to\seedMiniGameArea.json', SINGLE_CLOB) AS j

-- 方法 2: 直接貼上 JSON 內容（適合小量資料）
-- SET @json = N'{ "User_Wallet": [...], "CouponType": [...], ... }'

-- 匯入 User_Wallet 資料
INSERT INTO [User_Wallet] ([UserID], [Points], [CreatedAt], [UpdatedAt])
SELECT [UserID], [Points], [CreatedAt], [UpdatedAt]
FROM OPENJSON(@json, '$.User_Wallet')
WITH (
    [UserID] int,
    [Points] decimal(18,2),
    [CreatedAt] datetime2(7),
    [UpdatedAt] datetime2(7)
)

-- 匯入 CouponType 資料
INSERT INTO [CouponType] ([TypeName], [Description], [PointsRequired], [IsActive], [CreatedAt], [UpdatedAt])
SELECT [TypeName], [Description], [PointsRequired], [IsActive], [CreatedAt], [UpdatedAt]
FROM OPENJSON(@json, '$.CouponType')
WITH (
    [TypeName] nvarchar(100),
    [Description] nvarchar(500),
    [PointsRequired] int,
    [IsActive] bit,
    [CreatedAt] datetime2(7),
    [UpdatedAt] datetime2(7)
)

-- 繼續其他資料表...
```

### 3.2 驗證資料匯入
```sql
-- 檢查各資料表的記錄數量
SELECT 'User_Wallet' AS TableName, COUNT(*) AS RecordCount FROM [User_Wallet]
UNION ALL
SELECT 'CouponType', COUNT(*) FROM [CouponType]
UNION ALL
SELECT 'Coupon', COUNT(*) FROM [Coupon]
UNION ALL
SELECT 'EVoucherType', COUNT(*) FROM [EVoucherType]
UNION ALL
SELECT 'EVoucher', COUNT(*) FROM [EVoucher]
UNION ALL
SELECT 'EVoucherToken', COUNT(*) FROM [EVoucherToken]
UNION ALL
SELECT 'EVoucherRedeemLog', COUNT(*) FROM [EVoucherRedeemLog]
UNION ALL
SELECT 'WalletHistory', COUNT(*) FROM [WalletHistory]
UNION ALL
SELECT 'UserSignInStats', COUNT(*) FROM [UserSignInStats]
UNION ALL
SELECT 'Pet', COUNT(*) FROM [Pet]
UNION ALL
SELECT 'MiniGame', COUNT(*) FROM [MiniGame];

-- 抽樣檢查資料
SELECT TOP 3 * FROM [User_Wallet];
SELECT TOP 3 * FROM [CouponType];
SELECT TOP 3 * FROM [Pet];
```

## 步驟 4: 設定應用程式連線字串

### 4.1 本機開發環境 (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=GameSpaceDb;User ID=sa;Password=YourStrong!Passw0rd;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30"
  }
}
```

### 4.2 一般 SQL Server 連線
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-sql-server.database.windows.net,1433;Database=GameSpaceDb;User ID=your-username;Password=your-password;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;Connection Timeout=30"
  }
}
```

### 4.3 Azure SQL Database 連線
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Database=GameSpaceDb;User ID=your-username@your-server;Password=your-password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"
  }
}
```

### 4.4 環境變數設定（推薦用於生產環境）

#### Windows
```cmd
setx ConnectionStrings__DefaultConnection "Server=your-server;Database=GameSpaceDb;User ID=app_user;Password=***;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

#### Linux/macOS
```bash
export ConnectionStrings__DefaultConnection="Server=your-server;Database=GameSpaceDb;User ID=app_user;Password=***;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

#### Docker
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=sql-server;Database=GameSpaceDb;User ID=sa;Password=YourStrong!Passw0rd;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

## 步驟 5: 執行 DbSmoke 測試

### 5.1 本機執行
```bash
cd tools/DbSmoke
dotnet restore
dotnet run --configuration Release
```

### 5.2 CI/CD 環境執行
```bash
# 設定連線字串環境變數
export ConnectionStrings__DefaultConnection="your-connection-string"

# 執行測試
dotnet run --project tools/DbSmoke/ --configuration Release
```

### 5.3 預期輸出範例
```
=== MiniGame DbSmoke 資料庫連線與資料檢查工具 ===
執行時間: 2025-09-16 10:30:15

🔍 開始檢查 MiniGame 相關資料表...

✅ 資料庫連線成功
伺服器: localhost\SQLEXPRESS
資料庫: GameSpaceDb

✅ PASS - User_Wallet: 有資料 (4 欄位) - 範例: UserID=1, Points=1500.00, CreatedAt=2024-09-01
✅ PASS - CouponType: 有資料 (7 欄位) - 範例: CouponTypeID=1, TypeName=新手禮包, Description=新手專用優惠券
✅ PASS - Coupon: 有資料 (8 欄位) - 範例: CouponID=1, CouponCode=NEWBIE001, CouponTypeID=1
... (其他資料表)

📄 報告已寫入: ../../reports/_latest/db_smoke.txt

=== 檢查結果摘要 ===
成功: 11/11
失敗: 0/11
成功率: 100.0%
```

## 步驟 6: 驗證 Admin 頁面

### 6.1 啟動應用程式
```bash
cd GameSpace
dotnet run --configuration Release
```

### 6.2 訪問 Admin 頁面
- 基本路徑: `https://localhost:5001/MiniGame/`
- 錢包管理: `https://localhost:5001/MiniGame/AdminWallet/Index`
- 寵物管理: `https://localhost:5001/MiniGame/AdminPet/Index`
- 簽到統計: `https://localhost:5001/MiniGame/AdminSignInStats/Index`

### 6.3 驗證種子資料顯示
確認以下頁面能正確顯示種子資料：
- [ ] 錢包餘額和歷史記錄
- [ ] 優惠券和電子券列表
- [ ] 寵物資訊和屬性
- [ ] 簽到統計和歷史
- [ ] 小遊戲列表和規則

## 常見問題排解

### Q1: 連線字串測試失敗
**解決方案**:
1. 檢查 SQL Server 服務是否啟動
2. 確認防火牆設定允許連線
3. 驗證使用者帳號和密碼正確
4. 檢查資料庫名稱是否存在

### Q2: 資料表建立失敗
**解決方案**:
1. 確保按照正確順序建立資料表（先建主表，後建外鍵關聯表）
2. 檢查資料類型是否與 database.json 一致
3. 確認外鍵參照的資料表已存在

### Q3: 種子資料匯入失敗
**解決方案**:
1. 檢查 JSON 格式是否正確
2. 確認資料符合資料表約束（主鍵、外鍵、唯一鍵等）
3. 分批匯入，每次不超過 1000 筆記錄

### Q4: DbSmoke 測試失敗
**解決方案**:
1. 確認連線字串設定正確
2. 檢查資料表是否存在
3. 驗證應用程式有足夠的資料庫存取權限

## 安全注意事項

1. **連線字串安全**: 
   - 生產環境不要在設定檔中明文儲存密碼
   - 使用環境變數或 Azure Key Vault

2. **資料庫權限**: 
   - 應用程式帳號只給予必要的權限
   - 避免使用 sa 帳號連線

3. **資料備份**: 
   - 定期備份資料庫
   - 測試還原程序

---
**文件版本**: 1.0  
**最後更新**: 2025-09-16  
**適用版本**: .NET 8.0, SQL Server 2019+