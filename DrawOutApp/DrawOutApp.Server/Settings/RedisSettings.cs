namespace DrawOutApp.Server.Settings
{
    public class RedisSettings : IRedisSettings
    {
        public string ConnectionString { get; set; } = String.Empty;
        public string InstanceName { get; set; } = String.Empty;
    }
}
