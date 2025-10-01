namespace ImageManager.Services;

public interface ILoggerService
{
    public void LogInfo(string message);
    public void LogWarning(string message);
    public void LogError(string message);
}

public class LoggerService : ILoggerService
{
    public void LogInfo(string message)
    {
        Console.WriteLine($"[INFO]{DateTime.Now.ToLongTimeString()}: {message}");
    }
    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING]{DateTime.Now.ToLongTimeString()}: {message}");
    }
    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR]{DateTime.Now.ToLongTimeString()}: {message}");
    }
}