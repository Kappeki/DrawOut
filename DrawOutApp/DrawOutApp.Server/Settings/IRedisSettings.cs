namespace DrawOutApp.Server.Settings
{
    public interface IRedisSettings
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
    }
}
