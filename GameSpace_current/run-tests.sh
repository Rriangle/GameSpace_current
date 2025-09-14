#!/bin/bash

# GameSpace 測試執行腳本
# 此腳本用於執行所有類型的測試並生成報告

echo "🚀 開始執行 GameSpace 測試套件..."

# 設定顏色
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 建立測試結果目錄
mkdir -p TestResults
mkdir -p coverage

# 清理舊的測試結果
echo "🧹 清理舊的測試結果..."
rm -rf TestResults/*
rm -rf coverage/*

# 執行所有測試
echo "📋 執行所有測試..."
dotnet test GameSpace.Tests/GameSpace.Tests.csproj \
    --collect:"XPlat Code Coverage" \
    --logger "trx;LogFileName=test-results.trx" \
    --results-directory TestResults \
    --verbosity normal

# 檢查測試結果
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ 所有測試通過！${NC}"
else
    echo -e "${RED}❌ 部分測試失敗！${NC}"
    exit 1
fi

# 生成覆蓋率報告
echo "📊 生成覆蓋率報告..."
if command -v reportgenerator &> /dev/null; then
    reportgenerator \
        -reports:"TestResults/**/*.coverage" \
        -targetdir:"coverage" \
        -reporttypes:"Html" \
        -verbosity:Info
    echo -e "${GREEN}✅ 覆蓋率報告已生成：coverage/index.html${NC}"
else
    echo -e "${YELLOW}⚠️  ReportGenerator 未安裝，跳過覆蓋率報告生成${NC}"
    echo "安裝命令：dotnet tool install -g dotnet-reportgenerator-globaltool"
fi

# 顯示測試摘要
echo ""
echo "📈 測試摘要："
echo "================"

# 計算測試總數
TOTAL_TESTS=$(find TestResults -name "*.trx" -exec grep -o 'testName=' {} \; | wc -l)
echo "總測試數：$TOTAL_TESTS"

# 顯示測試結果文件位置
echo "測試結果：TestResults/"
echo "覆蓋率報告：coverage/index.html"

echo ""
echo -e "${GREEN}🎉 測試執行完成！${NC}"