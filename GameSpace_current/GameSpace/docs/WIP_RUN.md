# WIP_RUN（追加式記錄）

2025-09-14 目前執行事項：
- 建立唯 main githooks（pre-commit、pre-push、pre-rebase）
- 清除 `database.json` meta 來源中的 legacy `database.sql` 參照（以 `database.json` 為唯一權威）

Next（下階段小計畫）：
- 套用唯 main 策略的 git 設定（refspec、push、pull.ff）
- dotnet restore/build/test 綠燈驗證
- 完成單一提交與推送 proof
