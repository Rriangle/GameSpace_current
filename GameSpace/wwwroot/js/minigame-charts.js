/**
 * MiniGame Admin 圖表初始化輔助工具
 * 提供統一的 Chart.js 配置和初始化方法
 */

// 確保 Chart.js 已載入
function ensureChartJs(callback) {
    if (typeof Chart !== 'undefined') {
        callback();
        return;
    }
    
    // 動態載入 Chart.js CDN
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/chart.js';
    script.onload = callback;
    script.onerror = function() {
        console.error('無法載入 Chart.js，請檢查網路連線');
    };
    document.head.appendChild(script);
}

// 標準顏色配置
const CHART_COLORS = {
    primary: '#4e73df',
    success: '#1cc88a', 
    info: '#36b9cc',
    warning: '#f6c23e',
    danger: '#e74a3b',
    secondary: '#858796',
    light: '#f8f9fc',
    dark: '#5a5c69'
};

const CHART_COLORS_ALPHA = {
    primary: 'rgba(78, 115, 223, 0.1)',
    success: 'rgba(28, 200, 138, 0.1)',
    info: 'rgba(54, 185, 204, 0.1)', 
    warning: 'rgba(246, 194, 62, 0.1)',
    danger: 'rgba(231, 74, 59, 0.1)',
    secondary: 'rgba(133, 135, 150, 0.1)'
};

// 標準圖表配置
function getChartOptions(type = 'line', title = '', yAxisLabel = '') {
    const baseOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: true,
                position: 'bottom'
            },
            tooltip: {
                mode: 'index',
                intersect: false,
                callbacks: {
                    title: function(context) {
                        return '日期：' + context[0].label;
                    },
                    label: function(context) {
                        const value = typeof context.parsed.y === 'number' 
                            ? context.parsed.y.toLocaleString() 
                            : context.parsed.y;
                        return context.dataset.label + '：' + value;
                    }
                }
            }
        },
        scales: {
            x: {
                title: {
                    display: true,
                    text: '日期'
                },
                grid: {
                    display: false
                }
            },
            y: {
                title: {
                    display: !!yAxisLabel,
                    text: yAxisLabel
                },
                beginAtZero: true,
                grid: {
                    borderDash: [2, 2]
                }
            }
        }
    };
    
    // 甜甜圈圖表特殊配置
    if (type === 'doughnut' || type === 'pie') {
        baseOptions.scales = undefined;
        baseOptions.plugins.legend.position = 'bottom';
        baseOptions.cutout = type === 'doughnut' ? '60%' : '0%';
    }
    
    return baseOptions;
}

// 欄位標籤對應
function getFieldLabel(field) {
    const fieldLabels = {
        'count': '筆數',
        'pointsSum': '點數加總',
        'sessions': '場次',
        'expSum': 'EXP 加總',
        'couponCount': '優惠券數',
        'signInCount': '簽到數',
        'rewardPointsSum': '獎勵點數',
        'rewardExpSum': '獎勵 EXP',
        'pointsGainedSum': '獲得點數',
        'walletHistoryCount': '錢包異動',
        'evoucherTokenTotal': 'EVoucher Tokens',
        'evoucherRedeemLogCount': '兌換記錄',
        'userSignInStatsCount': '簽到筆數',
        'miniGameCount': 'MiniGame 場次'
    };
    return fieldLabels[field] || field;
}

// 建立資料集
function createDataset(label, data, colorKey = 'primary', type = 'line') {
    const color = CHART_COLORS[colorKey] || CHART_COLORS.primary;
    const alphaColor = CHART_COLORS_ALPHA[colorKey] || CHART_COLORS_ALPHA.primary;
    
    return {
        label: getFieldLabel(label),
        data: data,
        borderColor: color,
        backgroundColor: type === 'line' ? alphaColor : color,
        borderWidth: 2,
        fill: type !== 'line',
        type: type
    };
}

// 快速建立圖表
function createQuickChart(canvasId, endpoint, yField = 'count', chartType = 'line', title = '') {
    ensureChartJs(function() {
        fetch(endpoint)
            .then(response => response.json())
            .then(data => {
                if (data.status === 'ok' && data.series) {
                    const canvas = document.getElementById(canvasId);
                    if (!canvas) {
                        console.error('找不到圖表容器：' + canvasId);
                        return;
                    }
                    
                    const ctx = canvas.getContext('2d');
                    const labels = data.series.map(s => s.date);
                    const values = data.series.map(s => s[yField] || 0);
                    
                    new Chart(ctx, {
                        type: chartType,
                        data: {
                            labels: labels,
                            datasets: [createDataset(yField, values, 'primary', chartType)]
                        },
                        options: getChartOptions(chartType, title, getFieldLabel(yField))
                    });
                } else {
                    console.error('圖表資料載入失敗：', data.message || '未知錯誤');
                }
            })
            .catch(error => {
                console.error('圖表載入失敗：', error);
            });
    });
}

// 多系列圖表
function createMultiSeriesChart(canvasId, endpoint, yFields = ['count'], chartType = 'line', title = '') {
    ensureChartJs(function() {
        fetch(endpoint)
            .then(response => response.json())
            .then(data => {
                if (data.status === 'ok' && data.series) {
                    const canvas = document.getElementById(canvasId);
                    if (!canvas) {
                        console.error('找不到圖表容器：' + canvasId);
                        return;
                    }
                    
                    const ctx = canvas.getContext('2d');
                    const labels = data.series.map(s => s.date);
                    
                    const datasets = yFields.map((field, index) => {
                        const values = data.series.map(s => s[field] || 0);
                        const colorKeys = ['primary', 'success', 'info', 'warning', 'danger'];
                        const colorKey = colorKeys[index % colorKeys.length];
                        return createDataset(field, values, colorKey, chartType);
                    });
                    
                    new Chart(ctx, {
                        type: chartType,
                        data: {
                            labels: labels,
                            datasets: datasets
                        },
                        options: getChartOptions(chartType, title)
                    });
                } else {
                    console.error('圖表資料載入失敗：', data.message || '未知錯誤');
                }
            })
            .catch(error => {
                console.error('圖表載入失敗：', error);
            });
    });
}

// 甜甜圈圖表（用於分佈統計）
function createDonutChart(canvasId, data, title = '') {
    ensureChartJs(function() {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('找不到圖表容器：' + canvasId);
            return;
        }
        
        const ctx = canvas.getContext('2d');
        const colors = [
            CHART_COLORS.primary,
            CHART_COLORS.success,
            CHART_COLORS.info,
            CHART_COLORS.warning,
            CHART_COLORS.danger,
            CHART_COLORS.secondary
        ];
        
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.labels,
                datasets: [{
                    data: data.values,
                    backgroundColor: colors,
                    borderColor: colors,
                    borderWidth: 2
                }]
            },
            options: getChartOptions('doughnut', title)
        });
    });
}

// 載入狀態管理
function showChartLoading(containerId) {
    const container = document.getElementById(containerId);
    if (container) {
        container.innerHTML = `
            <div class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">載入中...</span>
                </div>
                <div class="mt-2 text-muted">載入圖表資料中...</div>
            </div>
        `;
    }
}

function showChartError(containerId, message) {
    const container = document.getElementById(containerId);
    if (container) {
        container.innerHTML = `
            <div class="alert alert-warning">
                <i class="fas fa-exclamation-triangle me-2"></i>
                圖表載入失敗：${message}
            </div>
        `;
    }
}

// 全域可用的工具函數
window.MiniGameCharts = {
    ensureChartJs,
    createQuickChart,
    createMultiSeriesChart,
    createDonutChart,
    showChartLoading,
    showChartError,
    CHART_COLORS,
    getFieldLabel
};