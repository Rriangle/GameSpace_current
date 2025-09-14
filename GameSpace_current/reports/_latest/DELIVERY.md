# 交付紀要（本次）

- HEAD：稍後推送後補記 URL 與 SHA（待環境允許）
- 修復點（極簡）：
  - 清除 `database.json` meta 中 legacy `database.sql` 提及
  - 建立 `.githooks`（pre-commit / pre-push / pre-rebase）
- build/test：將於環境允許後執行 `dotnet restore && dotnet build -c Release && dotnet test --no-build -c Release`
