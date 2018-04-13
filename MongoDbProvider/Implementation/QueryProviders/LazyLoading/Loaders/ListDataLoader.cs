using System.Collections.Generic;
using System.Reflection;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders
{
    class ListDataLoader<T>: AbstractDataLoader<T, IList<T>>
    {
        public ListDataLoader(ICustomServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IList<T> LoadData<TSource>(TSource source, PropertyInfo loadedProperty)
        {
            return TrackingList<T>.CreateExistingTrackingList(this.EnumerableDataLoader.LoadData(source, loadedProperty));
        }
    }
}
