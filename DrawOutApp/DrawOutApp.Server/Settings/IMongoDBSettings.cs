namespace DrawOutApp.Server.Settings
{
    public interface IMongoDBSettings
    { 
        string RoomsCollectionName { get; set; }
        string WordPacksCollectionName { get; set; }
        string NicknamesCollectionName { get; set; }
        string IconsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
