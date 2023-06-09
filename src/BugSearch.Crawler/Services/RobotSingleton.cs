namespace BugSearch.Crawler.Services;

public class RobotSingleton
{
    private RobotSingleton() { }

    private static RobotSingleton _instance = default!;

    private static readonly object _lock = new object();

    private IEnumerable<string> _url = default!;

    public bool PersistData { get; set; } = true;

    public static RobotSingleton GetInstance()
    {
        if (_instance is null)
        {
            lock (_lock)
            {
                if (_instance is null)
                {
                    _instance = new RobotSingleton();
                }
            }
        }

        return _instance;
    }

    public void SetUrls(IEnumerable<string> url)
    {
        _url = url;
    }

    public IEnumerable<string> GetUrls()
    {
        return _url;
    }
}