using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Threading.Tasks;
using DexRobotPDA.Utilities;
using Microsoft.AspNetCore.Components;

namespace DexRobotPDA.Services;

public class BarcodeScannerService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ISnackbar _snackbar;
    private DotNetObjectReference<BarcodeScannerService>? _dotNetRef;
    private ElementReference _barcodeInput;
    private bool _isAutoMode = true;
    private int _manualFocusIndex = 1;
    private bool _autoFocusActive = true;
    private bool _isActive;
    private string? _currentFocusedInputId;
    
    // 扫描结果回调
    public event Action<string>? OnBarcodeScanned;
    // 聚焦状态变化回调
    public event Action<bool>? OnAutoFocusChanged;
    
    public BarcodeScannerService(IJSRuntime jsRuntime, ISnackbar snackbar)
    {
        _jsRuntime = jsRuntime;
        _snackbar = snackbar;
    }
    
    // 初始化扫描器
    public async Task InitializeAsync(ElementReference barcodeInput)
    {
        _barcodeInput = barcodeInput;
        _dotNetRef = DotNetObjectReference.Create(this);
        
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "BarcodeScanner.setup",
                _dotNetRef,
                _barcodeInput
            );
            await EnsureAutoFocusAsync();
        }
        catch (Exception ex)
        {
            _isActive = false;
            SnackbarHelper.Show(_snackbar, $"扫描器初始化失败: {ex.Message}", Severity.Error);
        }
    }
    
    // 切换自动/手动模式
    public async Task ToggleModeAsync()
    {
        _isAutoMode = !_isAutoMode;
        
        if (_isAutoMode)
        {
            await EnterAutoFocusModeAsync();
            SnackbarHelper.Show(_snackbar, "已切换到自动匹配模式", Severity.Success);
        }
        else
        {
            await ExitAutoFocusModeAsync();
            SnackbarHelper.Show(_snackbar, "已切换到手动聚焦模式", Severity.Info);
        }
        
        OnAutoFocusChanged?.Invoke(_isAutoMode);
    }
    
    public async Task ToggleModeAsync2(bool isAutoMode)
    {
        _isAutoMode = isAutoMode;
        
        if (_isAutoMode)
        {
            await EnterAutoFocusModeAsync();
            SnackbarHelper.Show(_snackbar, "已切换到自动匹配模式", Severity.Success);
        }
        else
        {
            await ExitAutoFocusModeAsync();
            SnackbarHelper.Show(_snackbar, "已切换到手动聚焦模式", Severity.Info);
        }
        
        OnAutoFocusChanged?.Invoke(_isAutoMode);
    }
    
    #region 聚焦管理
    public async Task EnterAutoFocusModeAsync()
    {
        _autoFocusActive = true;
        await EnsureAutoFocusAsync();
        OnAutoFocusChanged?.Invoke(true);
    }
    
    public Task ExitAutoFocusModeAsync()
    {
        _autoFocusActive = false;
        OnAutoFocusChanged?.Invoke(false);
        return Task.CompletedTask;
    }
    
    // 优化自动聚焦：增加防抖机制
    private DateTime _lastFocusAttempt = DateTime.MinValue;
    private const int FocusDebounceMs = 100; // 防抖时间
    
    public async Task EnsureAutoFocusAsync()
    {
        // 防抖处理：防止短时间内多次尝试聚焦
        var now = DateTime.Now;
        if (now.Subtract(_lastFocusAttempt).TotalMilliseconds < FocusDebounceMs)
            return;
            
        _lastFocusAttempt = now;
            
        if (_autoFocusActive && !IsExcludedInputFocused())
        {
            try
            {
                await _barcodeInput.FocusAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动聚焦失败: {ex.Message}");
            }
        }
    }
    
    public void SetManualFocusIndex(int index)
    {
        if (!_isAutoMode)
        {
            _manualFocusIndex = index;
        }
    }
    
    public int GetManualFocusIndex() => _manualFocusIndex;
    
    // 记录当前聚焦的输入框ID
    public void SetCurrentFocusedInputId(string? inputId)
    {
        _currentFocusedInputId = inputId;
    }
    
    // 获取当前聚焦的输入框ID（新增）
    public string? CurrentFocusedInputId => _currentFocusedInputId;
    
    // 检查当前聚焦的是否是需要排除自动聚焦的输入框
    private bool IsExcludedInputFocused()
    {
        return !string.IsNullOrEmpty(_currentFocusedInputId) &&
               (_currentFocusedInputId.Contains("taskId") || 
                _currentFocusedInputId.Contains("remarks"));
    }
    #endregion
    
    // 处理输入框焦点
    public void HandleInputFocus() => _isActive = true;
    
    public async Task HandleInputBlurAsync()
    {
        _isActive = false;

        if (_autoFocusActive && !IsExcludedInputFocused())
        {
            await Task.Delay(100); // 延迟聚焦，避免快速切换
            await EnsureAutoFocusAsync();
            _isActive = true;
        }
    }
    
    // 接收条码数据（优化：移除不必要的提示）
    [JSInvokable]
    public Task ReceiveBarcode(string barcodeData)
    {
        if (string.IsNullOrWhiteSpace(barcodeData)) return Task.CompletedTask;
        
        // 移除扫描成功提示，减少UI更新
        // SnackbarHelper.Show(_snackbar, $"扫描到条码: {barcodeData}", Severity.Success);
        
        OnBarcodeScanned?.Invoke(barcodeData);
        return Task.CompletedTask;
    }
    
    // 清理资源
    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        try
        {
            await _jsRuntime.InvokeVoidAsync("BarcodeScanner.cleanup");
        }
        catch
        {
            // 忽略清理时的错误
        }

        GC.SuppressFinalize(this);
    }
    
    public bool IsAutoMode => _isAutoMode;
    public bool IsActive => _isActive;
    public bool AutoFocusActive => _autoFocusActive;
}
