# GameSpace 部署腳本說明

本目錄包含 GameSpace 專案的所有部署和管理腳本。

## 腳本列表

### 核心管理腳本

- **`setup.sh`** - 初始設定腳本
  - 用途：設定 GameSpace 環境
  - 用法：`./scripts/setup.sh [environment]`
  - 範例：`./scripts/setup.sh development`

- **`start.sh`** - 啟動腳本
  - 用途：啟動 GameSpace 服務
  - 用法：`./scripts/start.sh [environment]`
  - 範例：`./scripts/start.sh production`

- **`stop.sh`** - 停止腳本
  - 用途：停止 GameSpace 服務
  - 用法：`./scripts/stop.sh [environment]`
  - 範例：`./scripts/stop.sh production`

- **`restart.sh`** - 重啟腳本
  - 用途：重啟 GameSpace 服務
  - 用法：`./scripts/restart.sh [environment]`
  - 範例：`./scripts/restart.sh production`

- **`status.sh`** - 狀態檢查腳本
  - 用途：檢查服務狀態和系統資源
  - 用法：`./scripts/status.sh [environment]`
  - 範例：`./scripts/status.sh production`

### 監控和日誌腳本

- **`monitor.sh`** - 即時監控腳本
  - 用途：即時監控服務狀態
  - 用法：`./scripts/monitor.sh [environment]`
  - 範例：`./scripts/monitor.sh production`

- **`logs.sh`** - 日誌查看腳本
  - 用途：查看服務日誌
  - 用法：`./scripts/logs.sh [service] [lines]`
  - 範例：`./scripts/logs.sh app 100`

- **`health-check.sh`** - 健康檢查腳本
  - 用途：檢查服務健康狀態
  - 用法：`./scripts/health-check.sh [environment]`
  - 範例：`./scripts/health-check.sh production`

### 備份和還原腳本

- **`backup.sh`** - 完整備份腳本
  - 用途：建立完整系統備份
  - 用法：`./scripts/backup.sh [environment]`
  - 範例：`./scripts/backup.sh production`

- **`restore.sh`** - 完整還原腳本
  - 用途：從備份還原系統
  - 用法：`./scripts/restore.sh [backup_name] [environment]`
  - 範例：`./scripts/restore.sh 20240127_143022 production`

- **`backup-db.sh`** - 資料庫備份腳本
  - 用途：備份資料庫
  - 用法：`./scripts/backup-db.sh [environment] [backup_name]`
  - 範例：`./scripts/backup-db.sh production daily_backup`

- **`restore-db.sh`** - 資料庫還原腳本
  - 用途：還原資料庫
  - 用法：`./scripts/restore-db.sh [backup_file] [environment]`
  - 範例：`./scripts/restore-db.sh daily_backup.bak production`

### 部署和更新腳本

- **`deploy.sh`** - 部署腳本
  - 用途：部署新版本
  - 用法：`./scripts/deploy.sh [environment]`
  - 範例：`./scripts/deploy.sh production`

- **`update.sh`** - 更新腳本
  - 用途：更新到最新版本
  - 用法：`./scripts/update.sh [environment]`
  - 範例：`./scripts/update.sh production`

### 維護腳本

- **`cleanup.sh`** - 清理腳本
  - 用途：清理未使用的資源
  - 用法：`./scripts/cleanup.sh [environment]`
  - 範例：`./scripts/cleanup.sh production`

- **`ssl-setup.sh`** - SSL 設定腳本
  - 用途：設定 SSL 憑證
  - 用法：`./scripts/ssl-setup.sh [domain] [email]`
  - 範例：`./scripts/ssl-setup.sh gamespace.example.com admin@example.com`

## 環境變數

所有腳本都支援環境變數配置。請在執行前設定適當的環境變數：

```bash
# 載入環境變數
export $(cat .env.production | grep -v '^#' | xargs)

# 或使用環境檔案
source .env.production
```

## 常用工作流程

### 1. 初始設定
```bash
# 設定開發環境
./scripts/setup.sh development

# 設定生產環境
./scripts/setup.sh production
```

### 2. 日常管理
```bash
# 檢查狀態
./scripts/status.sh production

# 查看日誌
./scripts/logs.sh app 100

# 重啟服務
./scripts/restart.sh production
```

### 3. 備份和還原
```bash
# 建立備份
./scripts/backup.sh production

# 還原備份
./scripts/restore.sh 20240127_143022 production
```

### 4. 更新部署
```bash
# 更新到最新版本
./scripts/update.sh production

# 或重新部署
./scripts/deploy.sh production
```

### 5. 監控和維護
```bash
# 即時監控
./scripts/monitor.sh production

# 清理資源
./scripts/cleanup.sh production
```

## 注意事項

1. **權限**：所有腳本都已設定為可執行，無需額外設定權限
2. **環境**：請確保 Docker 和 Docker Compose 已正確安裝
3. **備份**：定期執行備份腳本，建議設定自動化排程
4. **監控**：使用監控腳本定期檢查服務狀態
5. **日誌**：定期清理舊日誌檔案以節省磁碟空間

## 故障排除

### 常見問題

1. **服務無法啟動**
   - 檢查 Docker 是否運行
   - 檢查端口是否被占用
   - 查看日誌：`./scripts/logs.sh app`

2. **資料庫連接失敗**
   - 檢查資料庫容器狀態
   - 檢查連接字串配置
   - 執行健康檢查：`./scripts/health-check.sh`

3. **備份失敗**
   - 檢查備份目錄權限
   - 檢查磁碟空間
   - 檢查資料庫容器狀態

4. **SSL 設定失敗**
   - 檢查域名解析
   - 檢查防火牆設定
   - 檢查 certbot 安裝

### 日誌位置

- 應用程式日誌：`./logs/`
- 容器日誌：`docker-compose logs`
- 系統日誌：`/var/log/`

### 支援

如有問題，請檢查：
1. 腳本執行權限
2. 環境變數設定
3. Docker 服務狀態
4. 網路連接
5. 磁碟空間