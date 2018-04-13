using DbdocFramework.Abstracts;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    internal interface IDbsetContainer
    {
        IDbSet<T> GetDbSet<T>() where T: class;
    }
}
