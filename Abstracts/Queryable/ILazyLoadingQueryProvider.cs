using System.Linq;

namespace DbdocFramework.Abstracts.Queryable
{
    interface ILazyLoadingQueryProvider<T>: IQueryProvider
    {
        ILazyLoadingQueryable<T> CreateQuery();
    }
}
