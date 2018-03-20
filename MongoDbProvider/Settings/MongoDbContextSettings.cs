namespace DbdocFramework.MongoDbProvider.Settings
{
    public class MongoDbContextSettings
    {
        public MongoDbContextSettings(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
