namespace DrawOutApp.Server.Settings
{
    public class MongoDBSettings : IMongoDBSettings
    {
        public string RoomsCollectionName { get; set; } = String.Empty;
        public string WordPacksCollectionName { get; set; } = String.Empty;
        public string NicknamesCollectionName { get; set; } = String.Empty;
        public string IconsCollectionName { get; set; } = String.Empty;
        public string ConnectionString { get; set; } = String.Empty;
        public string DatabaseName { get; set; } = String.Empty;
    }
}
