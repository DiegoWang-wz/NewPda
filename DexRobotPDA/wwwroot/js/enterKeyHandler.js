// 存储键盘事件监听器的引用，以便后续移除
let enterKeyListener = null;
let currentDotNetRef = null;

// 初始化回车键监听器
window.enterKeyHandler = {
    initialize: (dotNetRef) => {
        currentDotNetRef = dotNetRef;

        // 定义键盘按下事件的处理函数
        enterKeyListener = (event) => {
            // 检查是否按下的是回车键 (keyCode 13)
            if (event.keyCode === 13) {
                // 调用Blazor组件中的方法切换按钮状态
                currentDotNetRef.invokeMethodAsync('OnEnterKeyPressed');
            }
        };

        // 为整个文档添加键盘按下事件监听器
        document.addEventListener('keydown', enterKeyListener);
    },

    // 移除回车键监听器，用于组件销毁时清理
    dispose: () => {
        if (enterKeyListener) {
            document.removeEventListener('keydown', enterKeyListener);
            enterKeyListener = null;
            currentDotNetRef = null;
        }
    }
};
    