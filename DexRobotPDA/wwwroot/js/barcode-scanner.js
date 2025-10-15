// 条码扫描器模块 - 修复版
window.BarcodeScanner = {
    currentBarcode: '',
    barcodeTimer: null,
    scanThreshold: 50,
    dotNetRef: null,
    inputElement: null,

    setup: function(dotNetRef, element) {
        try {
            this.dotNetRef = dotNetRef;
            this.inputElement = element;

            if (!element) {
                throw new Error('未找到输入元素');
            }

            // 清除所有现有事件监听器
            element.onkeydown = null;
            element.onkeyup = null;
            element.onkeypress = null;

            // 监听keydown事件来捕获正确的字符
            element.addEventListener('keydown', (e) => {
                // 阻止默认行为，但允许字符输入
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.processCompleteScan();
                    return false;
                }

                // 处理字符输入
                if (!e.ctrlKey && !e.altKey && !e.metaKey) {
                    const char = this.getCharacterFromKeyEvent(e);
                    if (char) {
                        e.preventDefault(); // 阻止默认输入
                        this.handleCharacterInput(char);
                    }
                }
            });

            // 聚焦到输入框
            element.focus();

        } catch (error) {
            console.error('初始化错误:', error);
            this.logToBlazor(`扫码枪错误: ${error.message}`);
        }
    },

    getCharacterFromKeyEvent: function(event) {
        // 处理特殊键
        if (event.key === 'Enter' || event.key === 'Tab' || event.key === 'Escape') {
            return null;
        }

        // 处理功能键
        if (event.key.length > 1) {
            return null;
        }

        // 直接使用key属性（现代浏览器更可靠）
        if (event.key && event.key.length === 1) {
            return event.key;
        }

        // 备用方案：使用keyCode和shift状态
        if (event.keyCode) {
            // 字母键 (A-Z)
            if (event.keyCode >= 65 && event.keyCode <= 90) {
                return event.shiftKey ?
                    String.fromCharCode(event.keyCode) :
                    String.fromCharCode(event.keyCode + 32);
            }

            // 数字键 (0-9)
            if (event.keyCode >= 48 && event.keyCode <= 57) {
                return String.fromCharCode(event.keyCode);
            }

            // 数字小键盘 (0-9)
            if (event.keyCode >= 96 && event.keyCode <= 105) {
                return String.fromCharCode(event.keyCode - 48);
            }
        }

        return null;
    },

    handleCharacterInput: function(char) {
        if (this.barcodeTimer) {
            clearTimeout(this.barcodeTimer);
        }

        this.currentBarcode += char;
        console.log('当前条码:', this.currentBarcode); // 调试用

        this.barcodeTimer = setTimeout(() => {
            this.processCompleteScan();
        }, this.scanThreshold);
    },

    processCompleteScan: function() {
        if (this.currentBarcode && this.currentBarcode.length > 0) {
            console.log('完成扫描:', this.currentBarcode); // 调试用

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('ReceiveBarcode', this.currentBarcode)
                    .catch((error) => {
                        console.error('传递扫描结果失败:', error);
                        this.logToBlazor('传递扫描结果失败');
                    });
            }
        }

        // 重置状态
        this.currentBarcode = '';
        if (this.inputElement) this.inputElement.value = '';
        if (this.barcodeTimer) {
            clearTimeout(this.barcodeTimer);
            this.barcodeTimer = null;
        }

        // 重新聚焦
        if (this.inputElement) {
            this.inputElement.focus();
        }
    },

    logToBlazor: function(message) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('LogFromJS', message).catch(() => {});
        }
    },

    cleanup: function() {
        if (this.inputElement) {
            this.inputElement.onkeydown = null;
            this.inputElement.onkeyup = null;
            this.inputElement.onkeypress = null;
        }
        if (this.barcodeTimer) {
            clearTimeout(this.barcodeTimer);
            this.barcodeTimer = null;
        }
        this.dotNetRef = null;
        this.inputElement = null;
        this.currentBarcode = '';
    }
};