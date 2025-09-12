# GameSpace 專案

一個包含小遊戲、使用者管理和社交功能的綜合遊戲平台。

## 先決條件

- .NET 8.0 SDK
- SQL Server（LocalDB、Express 或完整版）
- Visual Studio 2022 或 VS Code

## 手動資料庫設定

### 1. 建立資料庫

```sql
-- 連接到您的 SQL Server 實例
-- 建立資料庫
CREATE DATABASE GameSpace;
GO

-- 使用資料庫
USE GameSpace;
GO
```

### 2. 執行結構腳本

```bash
# 導航到結構目錄
cd schema

# 執行 database.sql 腳本
sqlcmd -S (localdb)\MSSQLLocalDB -d GameSpace -i database.sql
```

### 3. 更新連線字串

使用您的連線字串更新 `appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GameSpace_Identity;Trusted_Connection=true;MultipleActiveResultSets=true",
    "GameSpace": "Server=(localdb)\\mssqllocaldb;Database=GameSpace;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## 執行應用程式

1. **還原套件**
   ```bash
   dotnet restore
   ```

2. **建置應用程式**
   ```bash
   dotnet build
   ```

3. **執行應用程式**
   ```bash
   dotnet run
   ```

4. **存取應用程式**
   - 在瀏覽器中開啟 `https://localhost:5001`
   - MiniGame 區域：`https://localhost:5001/MiniGame`

## 專案結構

```
GameSpace/
├── Areas/
│   ├── MiniGame/          # 小遊戲功能
│   ├── social_hub/        # 社交功能
│   └── ...
├── Controllers/           # 主要控制器
├── Models/               # 資料模型
├── Data/                 # Database contexts
├── Views/                # Main views
└── schema/               # Database schema files
```

## Features

- **Mini Games**: Memory, Puzzle, Quiz games
- **User Management**: Registration, authentication, profiles
- **Pet System**: Virtual pet management
- **Wallet System**: Points and transaction history
- **Social Hub**: Chat and community features

## Development Notes

- Database schema is defined in `schema/database.sql`
- No EF migrations are used - manual schema management only
- All human-facing text is in English
- Areas are used for UI separation (Admin/Public)
- Serilog is configured for logging
- CorrelationId middleware is enabled for request tracking

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Verify connection strings in `appsettings.json`
- Check database permissions

### Build Errors
- Run `dotnet clean` then `dotnet build`
- Ensure all NuGet packages are restored
- Check .NET 8.0 SDK is installed

### Runtime Errors
- Check logs in `logs/` directory
- Verify database schema matches models
- Ensure all required services are registered