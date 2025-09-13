#!/bin/bash

# GameSpace 測試執行腳本
# 此腳本用於執行所有測試並生成測試報告

echo "🚀 開始執行 GameSpace 測試套件..."
echo "=================================="

# 檢查是否在正確的目錄
if [ ! -f "GameSpace.sln" ]; then
    echo "❌ 錯誤：請在包含 GameSpace.sln 的目錄中執行此腳本"
    exit 1
fi

# 清理之前的測試結果
echo "🧹 清理之前的測試結果..."
rm -rf TestResults/
rm -rf coverage/

# 還原 NuGet 套件
echo "📦 還原 NuGet 套件..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ 套件還原失敗"
    exit 1
fi

# 建置解決方案
echo "🔨 建置解決方案..."
dotnet build --configuration Release --no-restore

if [ $? -ne 0 ]; then
    echo "❌ 建置失敗"
    exit 1
fi

# 執行單元測試
echo "🧪 執行單元測試..."
dotnet test GameSpace.Tests/GameSpace.Tests.csproj \
    --configuration Release \
    --no-build \
    --verbosity normal \
    --logger "trx;LogFileName=test-results.trx" \
    --results-directory TestResults \
    --collect:"XPlat Code Coverage" \
    --settings coverlet.runsettings

# 檢查測試結果
TEST_EXIT_CODE=$?

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "✅ 所有測試通過！"
else
    echo "❌ 部分測試失敗"
fi

# 生成測試報告
echo "📊 生成測試報告..."

# 檢查是否有測試結果文件
if [ -d "TestResults" ]; then
    echo "📁 測試結果已保存到 TestResults/ 目錄"
    
    # 列出測試結果文件
    echo "📋 測試結果文件："
    find TestResults -name "*.trx" -o -name "*.coverage" | head -10
fi

# 生成覆蓋率報告（如果可用）
if command -v reportgenerator &> /dev/null; then
    echo "📈 生成代碼覆蓋率報告..."
    reportgenerator \
        -reports:"TestResults/**/*.coverage" \
        -targetdir:"coverage" \
        -reporttypes:"Html" \
        -verbosity:Info
    echo "📊 覆蓋率報告已生成到 coverage/ 目錄"
else
    echo "ℹ️  未安裝 ReportGenerator，跳過覆蓋率報告生成"
    echo "   安裝命令：dotnet tool install -g dotnet-reportgenerator-globaltool"
fi

# 顯示測試摘要
echo ""
echo "📋 測試執行摘要："
echo "=================="
echo "退出代碼: $TEST_EXIT_CODE"
echo "測試結果目錄: TestResults/"
if [ -d "coverage" ]; then
    echo "覆蓋率報告: coverage/"
fi

# 返回測試退出代碼
exit $TEST_EXIT_CODE