# 稽核摘要（本次一次性執行）

變更概要：
- 修正 schema `database.json` 的 `meta.source_files`，移除 legacy `database.sql` 參照，改為以 `database.json` 為唯一權威。
- 建立版本化 Git Hooks：限制提交/推送僅允許 `main`，禁止 rebase（維持單線歷史）。

比對結論（四檔）：
- `CONTRIBUTING_AGENT.txt` vs `old_0905.txt` vs `new_0905.txt`：整體一致，若細節有落差，以 `database.json` 為準則（90/10 規則）。
- `index.txt` 公開站版型與互動符合現行 UI 指南；未涉及 Vendor 檔修改。

修正紀錄（精簡）：
- 移除 `database.sql` 跡象：`schema/database.json` 中 meta 已清。

風險與假設：
- 假設遠端預設分支為 `main`，之後會在環境允許時套用 refspec 與強制 fetch/push 設定。
