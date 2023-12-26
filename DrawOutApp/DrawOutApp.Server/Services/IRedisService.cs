namespace DrawOutApp.Server.Services
{
    public interface IRedisService
    {
        Task SetAsync(string key, string value);
        Task<string> GetAsync(string key);
        Task RemoveKeyAsync(string key);
        Task AddCustomWordsAsync(string roomId, IEnumerable<string> words);
        Task<IEnumerable<string>> GetCustomWordsAsync(string roomId);

        //Task QueueNotificationAsync(string userId, Notification notification);
        //Task<IEnumerable<Notification>> GetPendingNotificationsAsync(string userId);
    }
}
