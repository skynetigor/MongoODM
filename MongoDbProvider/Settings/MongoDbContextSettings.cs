namespace DbdocFramework.MongoDbProvider.Settings
{
    public class MongoDbContextSettings
    {
        public MongoDbContextSettings(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public MongoDbContextSettings(string connectionString, bool dropCollectionsEachTime): this(connectionString)
        {
            DropCollectionsEachTime = dropCollectionsEachTime;
        }

        public string ConnectionString { get; }

        public bool DropCollectionsEachTime { get; }
    }
}
