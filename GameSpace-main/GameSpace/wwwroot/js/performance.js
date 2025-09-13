/**
 * 效能優化 JavaScript 模組
 * 包含懶載入、虛擬滾動、防抖動、節流等功能
 */

class PerformanceOptimizer {
    constructor() {
        this.observers = new Map();
        this.throttleTimers = new Map();
        this.debounceTimers = new Map();
        this.init();
    }

    init() {
        this.setupLazyLoading();
        this.setupIntersectionObserver();
        this.setupVirtualScrolling();
        this.setupImageOptimization();
        this.setupPreloading();
    }

    /**
     * 設定懶載入
     */
    setupLazyLoading() {
        const lazyImages = document.querySelectorAll('img[data-src]');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        this.loadImage(img);
                        observer.unobserve(img);
                    }
                });
            });

            lazyImages.forEach(img => imageObserver.observe(img));
        } else {
            // 降級處理
            lazyImages.forEach(img => this.loadImage(img));
        }
    }

    /**
     * 載入圖片
     */
    loadImage(img) {
        const src = img.getAttribute('data-src');
        if (src) {
            img.src = src;
            img.classList.add('loaded');
            img.removeAttribute('data-src');
        }
    }

    /**
     * 設定交叉觀察器
     */
    setupIntersectionObserver() {
        const observerOptions = {
            root: null,
            rootMargin: '50px',
            threshold: 0.1
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        // 觀察需要動畫的元素
        document.querySelectorAll('.animate-on-scroll').forEach(el => {
            observer.observe(el);
        });
    }

    /**
     * 設定虛擬滾動
     */
    setupVirtualScrolling() {
        const virtualScrollContainers = document.querySelectorAll('.virtual-scroll');
        
        virtualScrollContainers.forEach(container => {
            this.createVirtualScroll(container);
        });
    }

    /**
     * 建立虛擬滾動
     */
    createVirtualScroll(container) {
        const itemHeight = 60;
        const containerHeight = container.clientHeight;
        const visibleItems = Math.ceil(containerHeight / itemHeight);
        const buffer = 5; // 緩衝區
        
        let scrollTop = 0;
        let startIndex = 0;
        let endIndex = visibleItems + buffer;

        const updateVisibleItems = () => {
            const items = container.querySelectorAll('.virtual-item');
            const totalItems = items.length;
            
            startIndex = Math.max(0, Math.floor(scrollTop / itemHeight) - buffer);
            endIndex = Math.min(totalItems, startIndex + visibleItems + buffer * 2);

            // 隱藏不可見項目
            items.forEach((item, index) => {
                if (index >= startIndex && index < endIndex) {
                    item.style.display = 'block';
                    item.style.transform = `translateY(${index * itemHeight}px)`;
                } else {
                    item.style.display = 'none';
                }
            });
        };

        container.addEventListener('scroll', this.throttle(() => {
            scrollTop = container.scrollTop;
            updateVisibleItems();
        }, 16)); // 60fps

        updateVisibleItems();
    }

    /**
     * 設定圖片優化
     */
    setupImageOptimization() {
        // WebP 支援檢測
        const supportsWebP = this.checkWebPSupport();
        
        if (supportsWebP) {
            document.querySelectorAll('img[data-webp]').forEach(img => {
                const webpSrc = img.getAttribute('data-webp');
                if (webpSrc) {
                    img.src = webpSrc;
                }
            });
        }

        // 響應式圖片
        this.setupResponsiveImages();
    }

    /**
     * 檢查 WebP 支援
     */
    checkWebPSupport() {
        const canvas = document.createElement('canvas');
        canvas.width = 1;
        canvas.height = 1;
        return canvas.toDataURL('image/webp').indexOf('data:image/webp') === 0;
    }

    /**
     * 設定響應式圖片
     */
    setupResponsiveImages() {
        const images = document.querySelectorAll('img[data-sizes]');
        
        images.forEach(img => {
            const sizes = JSON.parse(img.getAttribute('data-sizes'));
            const currentWidth = img.clientWidth;
            
            let bestSrc = img.src;
            for (const [width, src] of Object.entries(sizes)) {
                if (currentWidth >= parseInt(width)) {
                    bestSrc = src;
                }
            }
            
            if (bestSrc !== img.src) {
                img.src = bestSrc;
            }
        });
    }

    /**
     * 設定預載入
     */
    setupPreloading() {
        // 預載入關鍵資源
        this.preloadCriticalResources();
        
        // 預載入下一頁面資源
        this.preloadNextPageResources();
    }

    /**
     * 預載入關鍵資源
     */
    preloadCriticalResources() {
        const criticalResources = [
            '/css/performance.css',
            '/js/performance.js'
        ];

        criticalResources.forEach(resource => {
            const link = document.createElement('link');
            link.rel = 'preload';
            link.href = resource;
            link.as = resource.endsWith('.css') ? 'style' : 'script';
            document.head.appendChild(link);
        });
    }

    /**
     * 預載入下一頁面資源
     */
    preloadNextPageResources() {
        // 預載入用戶可能訪問的頁面
        const nextPageLinks = document.querySelectorAll('a[href^="/"]');
        
        nextPageLinks.forEach(link => {
            link.addEventListener('mouseenter', () => {
                this.preloadPage(link.href);
            });
        });
    }

    /**
     * 預載入頁面
     */
    preloadPage(url) {
        if (this.preloadedPages.has(url)) return;
        
        const link = document.createElement('link');
        link.rel = 'prefetch';
        link.href = url;
        document.head.appendChild(link);
        
        this.preloadedPages.add(url);
    }

    /**
     * 節流函數
     */
    throttle(func, delay) {
        return (...args) => {
            const key = func.toString();
            
            if (this.throttleTimers.has(key)) {
                return;
            }
            
            func.apply(this, args);
            
            this.throttleTimers.set(key, setTimeout(() => {
                this.throttleTimers.delete(key);
            }, delay));
        };
    }

    /**
     * 防抖動函數
     */
    debounce(func, delay) {
        return (...args) => {
            const key = func.toString();
            
            if (this.debounceTimers.has(key)) {
                clearTimeout(this.debounceTimers.get(key));
            }
            
            this.debounceTimers.set(key, setTimeout(() => {
                func.apply(this, args);
                this.debounceTimers.delete(key);
            }, delay));
        };
    }

    /**
     * 效能監控
     */
    setupPerformanceMonitoring() {
        // 監控 Core Web Vitals
        if ('PerformanceObserver' in window) {
            const observer = new PerformanceObserver((list) => {
                list.getEntries().forEach((entry) => {
                    console.log(`${entry.name}: ${entry.value}`);
                });
            });

            observer.observe({ entryTypes: ['largest-contentful-paint', 'first-input', 'layout-shift'] });
        }

        // 監控資源載入時間
        window.addEventListener('load', () => {
            const resources = performance.getEntriesByType('resource');
            resources.forEach(resource => {
                if (resource.duration > 1000) {
                    console.warn(`慢載入資源: ${resource.name} (${resource.duration}ms)`);
                }
            });
        });
    }
}

// 初始化效能優化器
document.addEventListener('DOMContentLoaded', () => {
    window.performanceOptimizer = new PerformanceOptimizer();
});

// 導出供其他模組使用
window.PerformanceOptimizer = PerformanceOptimizer;