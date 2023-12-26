namespace DrawOutApp.Server.DataLayer
{
    public interface IMongoDBSettings
    { 
        string RoomsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
