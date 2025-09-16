/**
 * MiniGame Admin UI 互動增強腳本
 * 提供搜尋防抖、表單保護、無障礙性增強等功能
 */

// 搜尋防抖機制
let searchTimeout;
function debounceSearch(inputElement, delay = 300) {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        // 如果有表單，自動提交
        const form = inputElement.closest('form');
        if (form) {
            form.submit();
        }
    }, delay);
}

// 雙重提交防護
function preventDoubleSubmit(form) {
    const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
    
    form.addEventListener('submit', function(e) {
        // 禁用所有提交按鈕
        submitButtons.forEach(button => {
            button.disabled = true;
            button.classList.add('loading');
            
            // 更新按鈕文字
            const originalText = button.textContent || button.value;
            button.setAttribute('data-original-text', originalText);
            
            if (button.tagName === 'BUTTON') {
                button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>處理中...';
            } else {
                button.value = '處理中...';
            }
        });
        
        // 5秒後重新啟用（防止網路問題導致永久禁用）
        setTimeout(() => {
            submitButtons.forEach(button => {
                button.disabled = false;
                button.classList.remove('loading');
                
                const originalText = button.getAttribute('data-original-text');
                if (originalText) {
                    if (button.tagName === 'BUTTON') {
                        button.innerHTML = originalText;
                    } else {
                        button.value = originalText;
                    }
                }
            });
        }, 5000);
    });
}

// 焦點管理
function manageFocus() {
    // 錯誤時焦點返回到第一個錯誤欄位
    const firstError = document.querySelector('.field-validation-error, .input-validation-error');
    if (firstError) {
        const associatedInput = firstError.previousElementSibling || 
                               document.querySelector(`[data-valmsg-for="${firstError.getAttribute('data-valmsg-for')}"]`);
        if (associatedInput && associatedInput.focus) {
            associatedInput.focus();
        }
    }
    
    // 模態框開啟時焦點管理
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        modal.addEventListener('shown.bs.modal', function() {
            const firstInput = this.querySelector('input, select, textarea, button');
            if (firstInput) {
                firstInput.focus();
            }
        });
    });
}

// 無障礙性增強
function enhanceAccessibility() {
    // 為表格添加 ARIA 標籤
    const tables = document.querySelectorAll('table:not([role])');
    tables.forEach((table, index) => {
        table.setAttribute('role', 'table');
        
        const caption = table.querySelector('caption') || table.previousElementSibling?.querySelector('h1, h2, h3, h4, h5, h6');
        if (caption) {
            table.setAttribute('aria-labelledby', caption.id || `table-caption-${index}`);
            if (!caption.id) {
                caption.id = `table-caption-${index}`;
            }
        }
    });
    
    // 為圖表添加無障礙性支援
    const charts = document.querySelectorAll('canvas[id*="Chart"]');
    charts.forEach((canvas, index) => {
        canvas.setAttribute('role', 'img');
        
        const title = canvas.closest('.card')?.querySelector('.card-header h6, .card-title');
        if (title && !canvas.getAttribute('aria-labelledby')) {
            const titleId = title.id || `chart-title-${index}`;
            title.id = titleId;
            canvas.setAttribute('aria-labelledby', titleId);
        }
        
        // 添加圖表描述
        if (!canvas.getAttribute('aria-describedby')) {
            const description = document.createElement('p');
            description.id = `chart-desc-${index}`;
            description.className = 'sr-only';
            description.textContent = '資料視覺化圖表，顯示統計資訊。如需詳細數據，請使用匯出功能下載原始資料。';
            canvas.parentNode.appendChild(description);
            canvas.setAttribute('aria-describedby', description.id);
        }
    });
}

// 鍵盤快捷鍵
function setupKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Alt+E: 匯出 CSV
        if (e.altKey && e.key === 'e') {
            e.preventDefault();
            const csvExportBtn = document.querySelector('.export-buttons a[href*="ExportCsv"]');
            if (csvExportBtn) {
                csvExportBtn.click();
            }
        }
        
        // Alt+S: 焦點到搜尋欄
        if (e.altKey && e.key === 's') {
            e.preventDefault();
            const searchInput = document.querySelector('input[name="search"], input[type="search"]');
            if (searchInput) {
                searchInput.focus();
            }
        }
        
        // Alt+C: 清除篩選
        if (e.altKey && e.key === 'c') {
            e.preventDefault();
            const clearBtn = document.querySelector('button[onclick*="clear"], .btn:contains("清除")');
            if (clearBtn) {
                clearBtn.click();
            }
        }
    });
}

// 初始化所有 UI 增強功能
document.addEventListener('DOMContentLoaded', function() {
    // 搜尋防抖
    const searchInputs = document.querySelectorAll('input[name="search"], input[type="search"]');
    searchInputs.forEach(input => {
        input.addEventListener('input', function() {
            debounceSearch(this);
        });
    });
    
    // 雙重提交防護
    const forms = document.querySelectorAll('form[method="post"]');
    forms.forEach(preventDoubleSubmit);
    
    // 焦點管理
    manageFocus();
    
    // 無障礙性增強
    enhanceAccessibility();
    
    // 鍵盤快捷鍵
    setupKeyboardShortcuts();
    
    console.log('MiniGame Admin UI 增強功能已載入');
});

// 匯出函數供全域使用
window.MiniGameAdminUI = {
    debounceSearch,
    preventDoubleSubmit,
    manageFocus,
    enhanceAccessibility,
    setupKeyboardShortcuts
};