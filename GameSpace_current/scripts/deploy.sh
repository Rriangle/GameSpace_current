#!/bin/bash

# GameSpace 部署腳本
# 用法: ./deploy.sh [environment] [version]
# 環境: staging, production
# 版本: 可選，預設為 latest

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 日誌函數
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 參數檢查
ENVIRONMENT=${1:-staging}
VERSION=${2:-latest}

if [[ "$ENVIRONMENT" != "staging" && "$ENVIRONMENT" != "production" ]]; then
    log_error "無效的環境參數: $ENVIRONMENT"
    log_error "請使用 'staging' 或 'production'"
    exit 1
fi

log_info "開始部署 GameSpace 到 $ENVIRONMENT 環境 (版本: $VERSION)"

# 檢查必要工具
check_dependencies() {
    log_info "檢查部署依賴項..."
    
    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安裝"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose 未安裝"
        exit 1
    fi
    
    log_info "依賴項檢查完成"
}

# 環境變量設定
setup_environment() {
    log_info "設定環境變量..."
    
    if [[ "$ENVIRONMENT" == "production" ]]; then
        export COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml
        export DB_PASSWORD=${PROD_DB_PASSWORD:-"YourStrong@Passw0rd123!"}
        export JWT_SECRET_KEY=${PROD_JWT_SECRET_KEY:-"YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256"}
    else
        export COMPOSE_FILE=docker-compose.yml:docker-compose.staging.yml
        export DB_PASSWORD=${STAGING_DB_PASSWORD:-"YourStrong@Passw0rd123!"}
        export JWT_SECRET_KEY=${STAGING_JWT_SECRET_KEY:-"YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256"}
    fi
    
    export ENVIRONMENT=$ENVIRONMENT
    export VERSION=$VERSION
    export DB_SERVER=${DB_SERVER:-"sqlserver"}
    export DB_NAME=${DB_NAME:-"GameSpaceDatabase"}
    export DB_USER=${DB_USER:-"sa"}
    export REDIS_CONNECTION_STRING=${REDIS_CONNECTION_STRING:-"redis:6379"}
    export JWT_ISSUER=${JWT_ISSUER:-"GameSpaceIssuer"}
    export JWT_AUDIENCE=${JWT_AUDIENCE:-"GameSpaceAudience"}
    
    log_info "環境變量設定完成"
}

# 備份現有數據
backup_data() {
    if [[ "$ENVIRONMENT" == "production" ]]; then
        log_info "備份生產環境數據..."
        
        # 創建備份目錄
        BACKUP_DIR="./backups/$(date +%Y%m%d_%H%M%S)"
        mkdir -p "$BACKUP_DIR"
        
        # 備份資料庫
        docker exec gamespace-sqlserver /opt/mssql-tools/bin/sqlcmd \
            -S localhost -U sa -P "$DB_PASSWORD" \
            -Q "BACKUP DATABASE [$DB_NAME] TO DISK = '/var/opt/mssql/backup/gamespace_backup.bak'"
        
        # 複製備份文件
        docker cp gamespace-sqlserver:/var/opt/mssql/backup/gamespace_backup.bak "$BACKUP_DIR/"
        
        log_info "數據備份完成: $BACKUP_DIR"
    fi
}

# 構建和部署
deploy() {
    log_info "開始構建和部署..."
    
    # 停止現有服務
    log_info "停止現有服務..."
    docker-compose down --remove-orphans
    
    # 構建新映像
    log_info "構建 Docker 映像..."
    docker-compose build --no-cache
    
    # 啟動服務
    log_info "啟動服務..."
    docker-compose up -d
    
    # 等待服務就緒
    log_info "等待服務就緒..."
    sleep 30
    
    # 檢查服務狀態
    check_services
}

# 檢查服務狀態
check_services() {
    log_info "檢查服務狀態..."
    
    # 檢查 SQL Server
    if docker exec gamespace-sqlserver /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" -Q "SELECT 1" &> /dev/null; then
        log_info "SQL Server 運行正常"
    else
        log_error "SQL Server 連接失敗"
        exit 1
    fi
    
    # 檢查 Redis
    if docker exec gamespace-redis redis-cli ping | grep -q "PONG"; then
        log_info "Redis 運行正常"
    else
        log_error "Redis 連接失敗"
        exit 1
    fi
    
    # 檢查應用程序
    if curl -f http://localhost/health &> /dev/null; then
        log_info "GameSpace 應用程序運行正常"
    else
        log_error "GameSpace 應用程序健康檢查失敗"
        exit 1
    fi
}

# 運行數據庫遷移
run_migrations() {
    log_info "運行數據庫遷移..."
    
    # 等待資料庫就緒
    sleep 10
    
    # 運行 EF Core 遷移（如果需要）
    # docker exec gamespace-app dotnet ef database update
    
    log_info "數據庫遷移完成"
}

# 清理舊映像
cleanup() {
    log_info "清理舊 Docker 映像..."
    
    # 刪除懸空映像
    docker image prune -f
    
    # 刪除未使用的映像
    docker image prune -a -f
    
    log_info "清理完成"
}

# 主函數
main() {
    log_info "=== GameSpace 部署腳本 ==="
    log_info "環境: $ENVIRONMENT"
    log_info "版本: $VERSION"
    log_info "=========================="
    
    check_dependencies
    setup_environment
    backup_data
    deploy
    run_migrations
    cleanup
    
    log_info "=== 部署完成 ==="
    log_info "應用程序 URL: http://localhost"
    log_info "管理員後台: http://localhost/Admin"
    log_info "健康檢查: http://localhost/health"
    log_info "================"
}

# 執行主函數
main "$@"