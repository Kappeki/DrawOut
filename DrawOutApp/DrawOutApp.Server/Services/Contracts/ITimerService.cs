using DrawOutApp.Server.Hubs;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface ITimerService
    {
        GameTimers GetOrAdd(string key);
        bool TryGetValue(string key, out GameTimers gameTimers);
    }
}
