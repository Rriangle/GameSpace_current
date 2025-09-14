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

詳細的技術文檔請參考 [GameSpace/docs/](GameSpace/docs/) 目錄。

## 專案狀態

當前專案狀態請參考 [docs/AUDIT.md](docs/AUDIT.md) 和 [docs/WIP.md](docs/WIP.md)。

---

**文檔版本**: v1.0  
**最後更新**: 2025-09-14