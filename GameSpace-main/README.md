# GameSpace 遊戲論壇平台

## 專案概述

GameSpace 是一個結合遊戲討論與社群互動的綜合平台，採用 ASP.NET Core MVC 8.0 架構開發。

## 文檔架構說明

### 文檔分層管理

本專案採用分層文檔管理架構，確保文檔的單一真實來源（Single Source of Truth）：

#### 根目錄 `docs/` - 專案管理文檔
- **用途**: 專案層級的稽核、進度追蹤和管理文檔
- **檔案**:
  - `AUDIT.md` - 專案稽核報告
  - `WIP.md` - 工作進度記錄
  - `progress.json` - 進度追蹤資料
- **管理原則**: 由專案管理流程維護，包含稽核發現、修復狀態、進度追蹤

#### `GameSpace/docs/` - 技術規格文檔
- **用途**: 模組技術規格、部署指南、運維文檔
- **檔案**:
  - `DATABASE.md` - 資料庫設計文檔
  - `MODULES.md` - 模組架構說明
  - `DEPLOYMENT.md` - 部署指南
  - `SECURITY.md` - 安全性文檔
  - `PERFORMANCE.md` - 效能優化文檔
  - `MONITORING.md` - 監控文檔
- **管理原則**: 由技術團隊維護，包含詳細的技術規格和實作指南

### 文檔交叉引用

- 專案管理文檔 (`docs/`) 引用技術文檔 (`GameSpace/docs/`) 中的相關規格
- 技術文檔引用專案管理文檔中的修復狀態和進度資訊
- 兩層文檔相互補充，形成完整的專案文檔體系

## 快速開始

### 環境要求
- **作業系統**: Windows/Linux/macOS
- **.NET 版本**: .NET 8.0
- **資料庫**: SQL Server 2019/2022
- **Shell 環境**: 
  - **Linux/macOS**: 原生 bash
  - **Windows**: WSL2 或 Git Bash 或 PowerShell
  - **容器**: Docker Desktop

### 開發環境設定

#### 1. 克隆專案
```bash
git clone <repository-url>
cd GameSpace-main
```

#### 2. 資料庫設定
```bash
# 建立資料庫
sqlcmd -S localhost -E -Q "CREATE DATABASE GameSpaceDB"

# 或使用 PowerShell (Windows)
sqlcmd -S localhost -E -Q "CREATE DATABASE GameSpaceDB"
```

#### 3. 執行專案
```bash
# 使用 bash (Linux/macOS/WSL)
cd GameSpace
dotnet restore
dotnet build
dotnet run

# 或使用 PowerShell (Windows)
cd GameSpace
dotnet restore
dotnet build
dotnet run
```

#### 4. 腳本執行
```bash
# Linux/macOS/WSL
./scripts/quick-deploy.sh
./scripts/setup-database.sh

# Windows PowerShell
.\scripts\quick-deploy.ps1
.\scripts\setup-database.ps1
```

### 跨平台腳本說明

本專案包含多個 shell 腳本，支援以下執行環境：

- **Linux/macOS**: 原生 bash 環境
- **Windows WSL2**: 推薦使用 WSL2 執行 bash 腳本
- **Windows PowerShell**: 提供 PowerShell 等價腳本
- **Docker**: 容器化環境，無需考慮平台差異

詳細的技術文檔請參考 [GameSpace/docs/](GameSpace/docs/) 目錄。

## 專案狀態

當前專案狀態請參考 [docs/AUDIT.md](docs/AUDIT.md) 和 [docs/WIP.md](docs/WIP.md)。

---

**文檔版本**: v1.0  
**最後更新**: 2025-09-14