# GameSpace 安全性指南

## 概述

本文檔詳細說明 GameSpace 平台的安全性實作，包括輸入驗證、XSS防護、CSRF保護、SQL注入防護等安全措施。

## 安全性架構

### 1. 多層防護策略

GameSpace 採用多層防護策略，確保系統安全：

```
┌─────────────────────────────────────┐
│           應用程式層                │
├─────────────────────────────────────┤
│  • 輸入驗證中間件                   │
│  • 速率限制中間件                   │
│  • CSRF 保護中間件                  │
│  • 安全性標頭中間件                 │
├─────────────────────────────────────┤
│           業務邏輯層                │
├─────────────────────────────────────┤
│  • 密碼強度驗證                     │
│  • 帳號鎖定機制                     │
│  • 安全事件記錄                     │
│  • HTML 內容清理                    │
├─────────────────────────────────────┤
│           資料存取層                │
├─────────────────────────────────────┤
│  • 參數化查詢                       │
│  • Entity Framework 保護            │
│  • 資料庫權限控制                   │
└─────────────────────────────────────┘
```

## 安全性中間件

### 1. SecurityHeadersMiddleware

設定各種安全性 HTTP 標頭：

```csharp
// 防止點擊劫持攻擊
X-Frame-Options: DENY

// 防止 MIME 類型嗅探
X-Content-Type-Options: nosniff

// 啟用 XSS 防護
X-XSS-Protection: 1; mode=block

// 強制 HTTPS
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload

// 內容安全政策
Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline'...

// 引用者政策
Referrer-Policy: strict-origin-when-cross-origin

// 權限政策
Permissions-Policy: geolocation=(), microphone=(), camera=()...
```

### 2. InputValidationMiddleware

防止 XSS 和 SQL 注入攻擊：

```csharp
// 危險字元模式檢測
- <script> 標籤
- javascript: 協議
- on* 事件處理器
- SQL 注入關鍵字 (UNION, SELECT, DROP, INSERT, DELETE, UPDATE, EXEC)
- 存儲過程調用 (sp_)
```

### 3. RateLimitingMiddleware

防止暴力攻擊和 DDoS：

```csharp
// 預設限制
- 每分鐘最多 100 個請求
- 基於 IP 和 User-Agent 識別
- 使用記憶體快取存儲計數器
- 自動重置計數器
```

### 4. CsrfProtectionMiddleware

防止跨站請求偽造攻擊：

```csharp
// CSRF Token 生成和驗證
- 為 GET 請求生成 Token
- 為 POST/PUT/DELETE 請求驗證 Token
- 使用資料保護 API 加密 Token
- 支援表單和 Header 兩種方式
```

## 安全性服務

### 1. SecurityService

提供全面的安全性功能：

#### 密碼強度驗證
```csharp
public async Task<bool> ValidatePasswordStrengthAsync(string password)
{
    // 檢查長度 (最少 8 字元)
    // 檢查複雜度 (大小寫、數字、特殊字元)
    // 檢查常見弱密碼
    // 使用正則表達式驗證
}
```

#### 帳號鎖定機制
```csharp
public async Task<bool> IsAccountLockedAsync(string userAccount)
{
    // 檢查失敗登入次數
    // 檢查鎖定時間
    // 使用快取提高效能
}
```

#### 輸入內容安全驗證
```csharp
public async Task<bool> ValidateInputSafetyAsync(string input)
{
    // 檢測危險字元模式
    // 防止 XSS 攻擊
    // 防止 SQL 注入
}
```

#### HTML 內容清理
```csharp
public async Task<string> SanitizeHtmlAsync(string html)
{
    // 移除危險標籤
    // 移除危險屬性
    // HTML 實體編碼
}
```

## 資料庫安全性

### 1. 參數化查詢

所有資料庫查詢都使用 Entity Framework 的參數化查詢：

```csharp
// 安全的查詢方式
var users = await _context.Users
    .Where(u => u.UserAccount == userAccount)
    .ToListAsync();

// 避免字串拼接
// ❌ 危險：string sql = $"SELECT * FROM Users WHERE UserAccount = '{userAccount}'";
// ✅ 安全：使用 Entity Framework 的 LINQ 查詢
```

### 2. 資料庫權限控制

```sql
-- 建立專用資料庫使用者
CREATE LOGIN GameSpaceApp WITH PASSWORD = 'StrongPassword123!';
CREATE USER GameSpaceApp FOR LOGIN GameSpaceApp;

-- 授予最小必要權限
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO GameSpaceApp;
GRANT EXECUTE ON SCHEMA::dbo TO GameSpaceApp;

-- 拒絕危險權限
DENY CREATE TABLE TO GameSpaceApp;
DENY DROP TABLE TO GameSpaceApp;
DENY ALTER TABLE TO GameSpaceApp;
```

## 認證和授權

### 1. JWT Token 安全性

```csharp
// JWT 配置
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = true,
    ValidIssuer = issuer,
    ValidateAudience = true,
    ValidAudience = audience,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero // 不允許時間偏差
};
```

### 2. 密碼加密

```csharp
// 使用 SHA256 + Salt 加密密碼
public string HashPassword(string password, string salt)
{
    using var sha256 = SHA256.Create();
    var saltedPassword = password + salt;
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
    return Convert.ToBase64String(hashedBytes);
}
```

### 3. 會話管理

```csharp
// Session 配置
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;        // 防止 XSS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 強制 HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // 防止 CSRF
});
```

## 安全事件記錄

### 1. 事件類型

系統記錄以下安全事件：

- `FailedLogin` - 登入失敗
- `AccountLocked` - 帳號鎖定
- `SuspiciousActivity` - 可疑活動
- `DataBreach` - 資料洩露
- `UnauthorizedAccess` - 未授權存取

### 2. 記錄格式

```json
{
  "timestamp": "2025-01-27T16:30:00Z",
  "eventType": "FailedLogin",
  "userId": "12345",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "details": "Failed login attempt from 192.168.1.100",
  "severity": "Warning"
}
```

## 配置安全性

### 1. 環境變數

敏感配置使用環境變數：

```bash
# 資料庫連線字串
ConnectionStrings__DefaultConnection="Server=prod-server;Database=GameSpace;..."

# JWT 密鑰
Jwt__Key="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"

# Redis 連線字串
ConnectionStrings__Redis="prod-redis-server:6379"
```

### 2. 配置檔案安全性

```json
{
  "Security": {
    "MaxFailedLoginAttempts": 5,
    "AccountLockoutDurationMinutes": 15,
    "PasswordMinLength": 8,
    "PasswordRequireUppercase": true,
    "PasswordRequireLowercase": true,
    "PasswordRequireDigit": true,
    "PasswordRequireSpecialChar": true,
    "SessionTimeoutMinutes": 30,
    "RateLimitMaxRequests": 100,
    "RateLimitWindowSeconds": 60
  }
}
```

## 部署安全性

### 1. HTTPS 強制

```csharp
// 強制 HTTPS
app.UseHttpsRedirection();

// HSTS 標頭
app.UseHsts();
```

### 2. 防火牆配置

```bash
# 只開放必要端口
- 80 (HTTP) - 重定向到 HTTPS
- 443 (HTTPS) - 主要服務端口
- 1433 (SQL Server) - 僅內網存取
- 6379 (Redis) - 僅內網存取
```

### 3. 反向代理配置

```nginx
# Nginx 配置範例
server {
    listen 443 ssl http2;
    server_name gamespace.com;
    
    # SSL 配置
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA384:ECDHE-RSA-AES128-GCM-SHA256;
    
    # 安全性標頭
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;
    add_header X-XSS-Protection "1; mode=block";
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload";
    
    # 代理到 ASP.NET Core
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## 安全測試

### 1. 滲透測試檢查清單

- [ ] SQL 注入測試
- [ ] XSS 攻擊測試
- [ ] CSRF 攻擊測試
- [ ] 點擊劫持測試
- [ ] 目錄遍歷測試
- [ ] 檔案上傳安全測試
- [ ] 認證繞過測試
- [ ] 會話管理測試
- [ ] 輸入驗證測試
- [ ] 錯誤處理測試

### 2. 安全掃描工具

```bash
# OWASP ZAP 掃描
zap-cli quick-scan --self-contained --start-options '-config api.disablekey=true' https://gamespace.com

# Nmap 端口掃描
nmap -sS -O -sV gamespace.com

# SSL 測試
testssl.sh gamespace.com
```

## 安全監控

### 1. 即時監控

- 登入失敗率監控
- 異常請求模式檢測
- 資源使用率監控
- 錯誤率監控

### 2. 告警機制

```csharp
// 安全事件告警
if (failedLoginCount > threshold)
{
    await SendSecurityAlertAsync("High failed login rate detected");
}

if (suspiciousActivityDetected)
{
    await SendSecurityAlertAsync("Suspicious activity detected");
}
```

## 安全最佳實踐

### 1. 開發階段

- 使用參數化查詢
- 驗證所有輸入
- 使用 HTTPS
- 定期更新依賴套件
- 進行安全代碼審查

### 2. 部署階段

- 使用強密碼
- 限制資料庫權限
- 配置防火牆
- 啟用日誌記錄
- 定期備份

### 3. 運維階段

- 監控安全事件
- 定期安全掃描
- 更新安全補丁
- 審查存取日誌
- 進行安全培訓

## 應急響應

### 1. 安全事件處理流程

1. **檢測** - 識別安全事件
2. **分析** - 評估事件影響
3. **遏制** - 限制事件擴散
4. **根除** - 移除威脅
5. **恢復** - 恢復正常服務
6. **學習** - 改進安全措施

### 2. 聯絡資訊

- 安全團隊：security@gamespace.com
- 緊急聯絡：+886-2-1234-5678
- 事件報告：incident@gamespace.com

## 結論

GameSpace 平台採用了多層防護策略，從應用程式層到資料庫層都實施了相應的安全措施。通過持續的安全監控和定期安全測試，確保平台的安全性和用戶資料的保護。

定期更新安全措施和進行安全培訓是維護平台安全的重要環節。