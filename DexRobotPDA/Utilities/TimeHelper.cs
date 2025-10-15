namespace DexRobotPDA.Utilities;

public class TimeHelper
{
    /// <summary>
    /// 计算两个时间之间的间隔（单位：秒）
    /// </summary>
    public static double GetSecondsDifference(DateTime start, DateTime end)
    {
        return (end - start).TotalSeconds;
    }

    /// <summary>
    /// 计算两个时间之间的间隔（单位：毫秒）
    /// </summary>
    public static double GetMillisecondsDifference(DateTime start, DateTime end)
    {
        return (end - start).TotalMilliseconds;
    }

    /// <summary>
    /// 计算两个时间之间的间隔（单位：分钟）
    /// </summary>
    public static double GetMinutesDifference(DateTime start, DateTime end)
    {
        return (end - start).TotalMinutes;
    }

    /// <summary>
    /// 计算两个时间之间的间隔（单位：小时）
    /// </summary>
    public static double GetHoursDifference(DateTime start, DateTime end)
    {
        return (end - start).TotalHours;
    }

    /// <summary>
    /// 计算两个时间之间的间隔（单位：天）
    /// </summary>
    public static double GetDaysDifference(DateTime start, DateTime end)
    {
        return (end - start).TotalDays;
    }

    /// <summary>
    /// 返回一个友好的时间差文本（例如：1小时25分钟）
    /// </summary>
    public static string GetFriendlyTimeDifference(DateTime start, DateTime end)
    {
        var diff = end - start;

        if (diff.TotalSeconds < 60)
            return $"{diff.Seconds} 秒";
        if (diff.TotalMinutes < 60)
            return $"{diff.Minutes} 分钟 {diff.Seconds} 秒";
        if (diff.TotalHours < 24)
            return $"{diff.Hours} 小时 {diff.Minutes} 分钟";
        else
            return $"{diff.Days} 天 {diff.Hours} 小时";
    }

    /// <summary>
    /// 返回当前时间与指定时间的差值（单位：秒）
    /// </summary>
    public static double GetSecondsFromNow(DateTime target)
    {
        return Math.Abs((DateTime.Now - target).TotalSeconds);
    }

    /// <summary>
    /// 返回当前时间与指定时间的友好文本（例如：刚刚 / 3分钟前 / 2小时前）
    /// </summary>
    public static string GetRelativeTimeText(DateTime target)
    {
        var span = DateTime.Now - target;
        if (span.TotalSeconds < 60)
            return "刚刚";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} 分钟前";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} 小时前";
        if (span.TotalDays < 30)
            return $"{(int)span.TotalDays} 天前";
        if (span.TotalDays < 365)
            return $"{(int)(span.TotalDays / 30)} 个月前";
        return $"{(int)(span.TotalDays / 365)} 年前";
    }
}