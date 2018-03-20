using System.Reflection;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface IDataLoader<TResult>
    {
        TResult LoadData<TSource>(TSource source, PropertyInfo loadedProperty);
    }
}
