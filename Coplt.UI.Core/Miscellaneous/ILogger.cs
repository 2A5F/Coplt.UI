namespace Coplt.UI.Miscellaneous;

public interface ILogger
{
    public bool IsEnabled(LogLevel level);
    public void Log(LogLevel level, string message);
}

public sealed class ActionLogger(Action<LogLevel, string> logger, Func<LogLevel, bool>? is_enable) : ILogger
{
    public bool IsEnabled(LogLevel level)
    {
        return is_enable?.Invoke(level) ?? true;
    }
    public void Log(LogLevel level, string message)
    {
        logger(level, message);
    }
}