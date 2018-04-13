namespace DbdocFramework.MongoDbProvider.Abstracts
{
    internal interface IDataLoadersProvider
    {
        IDataLoader<TResult> GetDataLoader<TResult>();
    }
}
