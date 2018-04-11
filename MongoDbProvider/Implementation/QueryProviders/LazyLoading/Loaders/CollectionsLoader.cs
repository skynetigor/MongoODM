using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders
{
    class CollectionsLoader<T> : AbstractDataLoader<T, ICollection<T>>
    {
        public CollectionsLoader(ICustomServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override ICollection<T> LoadData<TSource>(TSource source, PropertyInfo loadedProperty)
        {
            return TrackingList<T>.CreateExistingTrackingList(this.EnumerableDataLoader.LoadData(source, loadedProperty));
        }
    }
}
