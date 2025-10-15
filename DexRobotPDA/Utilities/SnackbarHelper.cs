using MudBlazor;

namespace DexRobotPDA.Utilities;

public class SnackbarHelper
{
    public static void Show(ISnackbar snackbar, string message, Severity severity)
    {
        // 设置全局位置
        snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
        
        // 添加消息并应用统一配置
        snackbar.Add(message, severity, config =>
        {
            config.ShowTransitionDuration = 300;
            config.HideTransitionDuration = 300;
            config.VisibleStateDuration = 2000;
            config.RequireInteraction = false;
            config.ShowCloseIcon = true;
            config.SnackbarVariant = Variant.Filled;
        });
    }
    
    public static void ShowBottom(ISnackbar snackbar, string message, Severity severity)
    {
        // 设置全局位置
        snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomCenter;
        
        // 添加消息并应用统一配置
        snackbar.Add(message, severity, config =>
        {
            config.ShowTransitionDuration = 300;
            config.HideTransitionDuration = 300;
            config.VisibleStateDuration = 2000;
            config.RequireInteraction = false;
            config.ShowCloseIcon = true;
            config.SnackbarVariant = Variant.Filled;
        });
    }
}