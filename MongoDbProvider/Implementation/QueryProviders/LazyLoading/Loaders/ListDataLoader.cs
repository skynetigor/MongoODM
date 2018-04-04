using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders
{
    class ListDataLoader<T>: AbstractDataLoader<T, ICollection<T>>
    {
        public ListDataLoader(ICustomServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override ICollection<T> LoadData<TSource>(TSource source, PropertyInfo loadedProperty)
        {
            return new TrackingList<T>(this.EnumerableDataLoader.LoadData(source, loadedProperty));
        }
    }

}
