# GameSpace 遊戲論壇平台

一個整合遊戲討論、寵物養成、簽到獎勵、商城交易的綜合性遊戲社群平台。

## 專案概述

GameSpace 是一個基於 ASP.NET Core MVC 的現代化遊戲社群平台，提供以下核心功能：

- 🎮 **遊戲論壇** - 多遊戲討論區、文章發布、社群互動
- 🐾 **寵物養成** - 史萊姆寵物系統、五維屬性管理、互動功能
- 🎯 **小遊戲** - 每日冒險挑戰、獎勵系統、排行榜
- 📅 **每日簽到** - 連續簽到獎勵、優惠券發放
- 💰 **錢包系統** - 會員點數管理、優惠券/禮券兌換
- 🛒 **官方商城** - 商品展示、訂單管理、支付整合
- 👥 **社群功能** - 好友系統、群組聊天、即時通訊
- 📊 **管理後台** - 用戶管理、內容審核、數據統計

## 技術架構

### 後端技術
- **框架**: ASP.NET Core MVC 8.0
- **資料庫**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **認證**: JWT Token + RBAC
- **日誌**: Serilog
- **架構**: 三層式架構 (Presentation/Business/Data)

### 前端技術
- **模板引擎**: Razor
- **UI 框架**: Bootstrap 5 + 玻璃風設計
- **JavaScript**: 原生 JS + Canvas (寵物系統)
- **樣式**: CSS3 + 響應式設計

### 資料庫設計
- **75個資料表** - 完整覆蓋所有業務需求
- **關聯設計** - 外鍵約束確保資料完整性
- **索引優化** - 提升查詢效能
- **種子資料** - 自動初始化測試資料

## 專案結構

```
GameSpace/
├── Areas/                    # 區域控制器
│   ├── Admin/               # 管理員後台
│   ├── MiniGame/            # 前台功能
│   └── Public/              # 公開頁面
├── Controllers/             # 控制器
│   └── Api/                 # API 控制器
├── Models/                  # 資料模型 (75個)
├── Services/                # 業務服務層
├── Data/                    # 資料存取層
├── Middleware/              # 中間件
├── wwwroot/                 # 靜態資源
├── docs/                    # 專案文檔
└── schema/                  # 規格文件
```

## 快速開始

### 環境需求
- .NET 8.0 SDK
- SQL Server 2019/2022
- Visual Studio 2022 或 VS Code

### 安裝步驟

1. **克隆專案**
   ```bash
   git clone https://github.com/Rriangle/GameSpace_current.git
   cd GameSpace_current/GameSpace
   ```

2. **還原套件**
   ```bash
   dotnet restore
   ```

3. **設定資料庫連線**
   - 修改 `appsettings.json` 中的 `ConnectionStrings:DefaultConnection`
   - 確保 SQL Server 服務正在運行

4. **執行專案**
   ```bash
   dotnet run
   ```

5. **訪問應用程式**
   - 前台: https://localhost:5001
   - 管理後台: https://localhost:5001/Admin

### 預設帳號

**管理員帳號**:
- 帳號: `zhang_zhiming_01` / 密碼: `Password001@`
- 帳號: `li_xiaohua_02` / 密碼: `Password001@`

**測試用戶**:
- 帳號: `dragonknight88` / 密碼: `Password001@`

## 核心功能

### 1. 用戶認證系統
- JWT Token 認證
- 用戶註冊/登入
- 密碼加密 (SHA256 + Salt)
- 會話管理 (Refresh Token)
- 角色權限控制 (RBAC)

### 2. 寵物養成系統
- 史萊姆寵物模型
- 五維屬性 (飢餓、心情、體力、清潔、健康)
- 互動功能 (餵食、玩耍、洗澡、休息)
- 等級系統和經驗值
- 外觀自訂 (膚色、背景)

### 3. 小遊戲系統
- 每日 3 次冒險挑戰
- 關卡難度設計
- 獎勵系統 (點數、經驗、優惠券)
- 排行榜功能

### 4. 錢包系統
- 會員點數管理
- 優惠券/禮券兌換
- 交易記錄追蹤
- 收支明細查詢

### 5. 論壇系統
- 多遊戲討論區
- 文章發布和回覆
- 按讚和收藏功能
- 搜尋和篩選

### 6. 商城系統
- 商品展示和管理
- 購物車功能
- 訂單處理
- 優惠券使用

### 7. 社群功能
- 好友系統
- 群組聊天
- 即時通訊
- 通知系統

### 8. 管理後台
- 用戶管理
- 內容審核
- 數據統計
- 系統設定

## API 文檔

### 認證 API
- `POST /api/auth/register` - 用戶註冊
- `POST /api/auth/login` - 用戶登入
- `POST /api/auth/refresh` - 刷新 Token
- `POST /api/auth/logout` - 用戶登出
- `GET /api/auth/profile` - 取得用戶資料

### 寵物 API
- `GET /api/pet/status` - 取得寵物狀態
- `POST /api/pet/care` - 寵物互動
- `POST /api/pet/adventure` - 開始冒險

### 錢包 API
- `GET /api/wallet/balance` - 查詢餘額
- `POST /api/wallet/exchange` - 兌換優惠券

## 資料庫設計

專案使用 75 個資料表，涵蓋所有業務需求：

### 核心資料表
- `Users` - 用戶主檔
- `UserIntroduce` - 用戶詳細資料
- `UserRights` - 用戶權限
- `UserWallet` - 用戶錢包
- `Pet` - 寵物資料
- `MiniGame` - 小遊戲記錄

### 論壇相關
- `Forums` - 論壇版面
- `Threads` - 討論串
- `ThreadPosts` - 回覆內容
- `Reactions` - 按讚記錄
- `Bookmarks` - 收藏記錄

### 商城相關
- `ProductInfo` - 商品資訊
- `OrderInfo` - 訂單主檔
- `OrderItems` - 訂單明細
- `Coupon` - 優惠券
- `EVoucher` - 電子禮券

## 開發規範

### 程式碼風格
- 使用繁體中文註解
- 遵循 C# 命名慣例
- 實作 SOLID 原則
- 完整的錯誤處理

### 資料庫規範
- 所有欄位使用繁體中文註解
- 外鍵約束確保資料完整性
- 適當的索引設計
- 遵循 database.json 規格

### 安全規範
- 密碼加密儲存
- JWT Token 認證
- 輸入驗證和輸出編碼
- SQL 注入防護

## 部署說明

### 開發環境
1. 確保 SQL Server 運行
2. 修改連線字串
3. 執行 `dotnet run`

### 生產環境
1. 設定環境變數
2. 配置 HTTPS 憑證
3. 設定資料庫連線
4. 部署到 IIS 或 Docker

## 貢獻指南

1. Fork 專案
2. 建立功能分支
3. 提交變更
4. 建立 Pull Request

## 授權

本專案採用 MIT 授權條款。

## 聯絡資訊

- 專案維護者: GameSpace Team
- 問題回報: GitHub Issues
- 技術支援: 請參考專案文檔

---

**版本**: v1.0.0  
**最後更新**: 2025-01-27  
**狀態**: 開發完成，所有核心功能已實作