using System.Reflection;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    internal interface IDataLoader<TResult>
    {
        TResult LoadData<TSource>(TSource source, PropertyInfo loadedProperty);
    }
}
