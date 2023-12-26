namespace DrawOutApp.Server.DataLayer
{
    public interface IRedisSettings
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
    }
}
