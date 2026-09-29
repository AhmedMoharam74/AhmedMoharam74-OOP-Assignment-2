namespace PatternsLab.Problems.Singleton;

public sealed class AppConfig
{
    private static readonly Lazy<AppConfig> InstanceHolder = new(() => new AppConfig(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static int LoadCount { get; private set; }
    public static AppConfig Instance => InstanceHolder.Value;
    public string DbConnection { get; set; }
    public string Theme { get; set; }

    private AppConfig()
    {
        LoadCount++;
        Console.WriteLine($"[AppConfig] Loading settings from disk... (load #{LoadCount})");
        Thread.Sleep(300);
        DbConnection = "Server=localhost;Db=School";
        Theme = "Light";
    }
}

public class DatabaseService
{
    public AppConfig Config => AppConfig.Instance;
    public void Connect() => Console.WriteLine($"Connecting to {Config.DbConnection}");
}

public class UiService
{
    public AppConfig Config => AppConfig.Instance;
    public void Render() => Console.WriteLine($"UI is using theme: {Config.Theme}");
}
