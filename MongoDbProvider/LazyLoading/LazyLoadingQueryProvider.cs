using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using DbdocFramework.Abstracts.Queryable;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.LazyLoading
{
    internal class LazyLoadingQueryProvider<T> : ILazyLoadingQueryProvider<T> where T : class
    {
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeMetadata { get; }
        private ICustomServiceProvider ServiceProvider { get; }
        private IProxyGenerator ProxyGenerator { get; }
        private TypeMetadata CurrentTypeMetadata { get; }
        private IQueryProvider Provider { get; }
        private Expression DefaultExpression { get; }

        public LazyLoadingQueryProvider(IMongoDatabase database, ITypeInitializer typeMetadata, ICustomServiceProvider serviceProvider)
        {
            Database = database;
            TypeMetadata = typeMetadata;
            ServiceProvider = serviceProvider;
            ProxyGenerator = new ProxyGenerator();
            CurrentTypeMetadata = typeMetadata.GetTypeMetadata<T>();
            var queryable = Database.GetCollection<T>(this.CurrentTypeMetadata.CollectionName)
                .AsQueryable();
            Provider = queryable.Provider;
            DefaultExpression = queryable.Expression;
        }

        public ILazyLoadingQueryable<T> CreateQuery()
        {
            return this.CreateLazyLoadingQuery<T>(this.DefaultExpression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return this.CreateQuery<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return this.CreateLazyLoadingQuery<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            var data = this.Provider
                .Execute(expression);

            if (data is IEnumerable<T> enumerable)
            {
                return enumerable.Select(this.CreateProxy);
            }

            return this.CreateProxy((T)data);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)this.Execute(expression);
        }

        private ILazyLoadingQueryable<TElement> CreateLazyLoadingQuery<TElement>(Expression expression)
        {
            return new LazyLoadingQueryable<TElement>(this, expression);
        }

        private T CreateProxy(T model)
        {
            //return ProxyGenerator.CreateClassProxyWithTarget(model, new LazyLoadingInterceptor());
            return ProxyGenerator.CreateClassProxy<T>(new LazyLoadingInterceptor());
        }
    }
}
