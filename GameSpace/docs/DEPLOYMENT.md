# GameSpace 部署指南

## 部署環境需求

### 系統需求
- **作業系統**: Windows Server 2019/2022 或 Linux (Ubuntu 20.04+)
- **.NET Runtime**: .NET 8.0 Runtime
- **資料庫**: Microsoft SQL Server 2019/2022
- **記憶體**: 最少 4GB RAM (建議 8GB+)
- **儲存空間**: 最少 10GB 可用空間

### 軟體需求
- IIS 10.0+ (Windows) 或 Nginx (Linux)
- SQL Server Management Studio
- Visual Studio 2022 (開發環境)

## 部署步驟

### 1. 準備環境

#### Windows Server 部署
```powershell
# 安裝 .NET 8.0 Runtime
winget install Microsoft.DotNet.Runtime.8

# 安裝 IIS 和必要模組
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DirectoryBrowsing
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45
```

#### Linux 部署
```bash
# 安裝 .NET 8.0 Runtime
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-8.0

# 安裝 Nginx
sudo apt-get install -y nginx
sudo systemctl enable nginx
sudo systemctl start nginx
```

### 2. 資料庫設定

#### 建立資料庫
```sql
-- 在 SQL Server 中建立資料庫
CREATE DATABASE GameSpaceDB;
GO

-- 建立登入帳號
CREATE LOGIN [GameSpaceUser] WITH PASSWORD = 'YourStrongPassword123!';
GO

-- 建立資料庫使用者
USE GameSpaceDB;
CREATE USER [GameSpaceUser] FOR LOGIN [GameSpaceUser];
GO

-- 授與權限
ALTER ROLE db_owner ADD MEMBER [GameSpaceUser];
GO
```

#### 設定連線字串
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GameSpaceDB;User Id=GameSpaceUser;Password=YourStrongPassword123!;TrustServerCertificate=true;"
  }
}
```

### 3. 應用程式部署

#### 發佈應用程式
```bash
# 發佈應用程式
dotnet publish -c Release -o ./publish

# 或使用 Visual Studio
# 右鍵專案 -> 發佈 -> 選擇資料夾
```

#### Windows IIS 部署
1. 將發佈的檔案複製到 `C:\inetpub\wwwroot\GameSpace`
2. 在 IIS 管理員中建立新網站
3. 設定應用程式集區為 "No Managed Code"
4. 設定網站根目錄為發佈資料夾
5. 設定預設文件為 `index.html`

#### Linux Nginx 部署
1. 將發佈的檔案複製到 `/var/www/gamespace`
2. 設定 Nginx 反向代理
3. 建立 systemd 服務檔案

### 4. Nginx 設定 (Linux)

#### 建立 Nginx 設定檔
```nginx
# /etc/nginx/sites-available/gamespace
server {
    listen 80;
    server_name your-domain.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

#### 啟用網站
```bash
sudo ln -s /etc/nginx/sites-available/gamespace /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### 5. 建立 Systemd 服務 (Linux)

#### 建立服務檔案
```ini
# /etc/systemd/system/gamespace.service
[Unit]
Description=GameSpace Web Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/gamespace/GameSpace.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=gamespace
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

#### 啟用服務
```bash
sudo systemctl enable gamespace
sudo systemctl start gamespace
sudo systemctl status gamespace
```

### 6. SSL 憑證設定

#### 使用 Let's Encrypt (Linux)
```bash
# 安裝 Certbot
sudo apt-get install -y certbot python3-certbot-nginx

# 取得 SSL 憑證
sudo certbot --nginx -d your-domain.com

# 設定自動更新
sudo crontab -e
# 加入: 0 12 * * * /usr/bin/certbot renew --quiet
```

#### Windows IIS SSL
1. 在 IIS 管理員中選擇網站
2. 右鍵 -> 編輯網站 -> 繫結
3. 新增 HTTPS 繫結
4. 選擇 SSL 憑證

### 7. 環境變數設定

#### 生產環境設定
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "GameSpace",
    "Audience": "GameSpaceUsers",
    "Salt": "YourRandomSaltString"
  }
}
```

### 8. 資料庫初始化

#### 執行資料庫遷移
```bash
# 在應用程式目錄中執行
dotnet ef database update
```

#### 執行資料種子
```bash
# 應用程式會自動執行資料種子
# 或手動執行資料種子服務
```

### 9. 監控和日誌

#### 設定 Serilog
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/gamespace/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

#### 設定健康檢查
```csharp
// 在 Program.cs 中
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddCheck<DatabaseHealthCheck>("database");

app.MapHealthChecks("/health");
```

### 10. 效能優化

#### 資料庫優化
```sql
-- 建立索引
CREATE INDEX IX_Users_User_Account ON Users(User_Account);
CREATE INDEX IX_Forums_Game_ID ON Forums(Game_ID);
CREATE INDEX IX_Threads_Forum_ID ON Threads(Forum_ID);
CREATE INDEX IX_ThreadPosts_Thread_ID ON ThreadPosts(Thread_ID);
```

#### 應用程式優化
```csharp
// 在 Program.cs 中
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

app.UseResponseCaching();
app.UseMemoryCache();
```

### 11. 備份策略

#### 資料庫備份
```sql
-- 建立備份作業
BACKUP DATABASE GameSpaceDB 
TO DISK = 'C:\Backup\GameSpaceDB.bak'
WITH FORMAT, INIT, NAME = 'GameSpaceDB Full Backup';
```

#### 應用程式備份
```bash
# 建立備份腳本
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
tar -czf /backup/gamespace_$DATE.tar.gz /var/www/gamespace
```

### 12. 故障排除

#### 常見問題
1. **應用程式無法啟動**
   - 檢查 .NET Runtime 是否安裝
   - 檢查連線字串是否正確
   - 檢查資料庫是否可連線

2. **資料庫連線失敗**
   - 檢查 SQL Server 服務是否運行
   - 檢查防火牆設定
   - 檢查使用者權限

3. **效能問題**
   - 檢查資料庫索引
   - 檢查記憶體使用量
   - 檢查 CPU 使用率

#### 日誌檢查
```bash
# 檢查應用程式日誌
sudo journalctl -u gamespace -f

# 檢查 Nginx 日誌
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### 13. 安全設定

#### 防火牆設定
```bash
# 開放必要連接埠
sudo ufw allow 80
sudo ufw allow 443
sudo ufw allow 22
sudo ufw enable
```

#### 資料庫安全
```sql
-- 限制資料庫存取
CREATE LOGIN [GameSpaceApp] WITH PASSWORD = 'AppPassword123!';
USE GameSpaceDB;
CREATE USER [GameSpaceApp] FOR LOGIN [GameSpaceApp];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [GameSpaceApp];
```

## 部署檢查清單

- [ ] 環境需求確認
- [ ] 資料庫建立和設定
- [ ] 應用程式發佈
- [ ] Web 伺服器設定
- [ ] SSL 憑證安裝
- [ ] 環境變數設定
- [ ] 資料庫初始化
- [ ] 監控和日誌設定
- [ ] 效能優化
- [ ] 備份策略
- [ ] 安全設定
- [ ] 功能測試

## 維護建議

1. **定期更新**: 保持 .NET Runtime 和依賴套件最新
2. **監控效能**: 定期檢查應用程式和資料庫效能
3. **備份資料**: 定期備份資料庫和應用程式檔案
4. **安全掃描**: 定期進行安全漏洞掃描
5. **日誌分析**: 定期分析日誌檔案，發現潛在問題

---

**注意**: 本部署指南僅供參考，實際部署時請根據您的環境和需求進行調整。