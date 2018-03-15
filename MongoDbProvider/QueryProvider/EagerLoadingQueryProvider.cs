using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DbdocFramework.Abstracts.Queryable;
using MongoDB.Bson;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Helpers;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.QueryProvider
{
    internal class EagerLoadingQueryProvider<T> : IEagerLoadingQueryProvider<T>
    {
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeInitializer { get; }
        private TypeMetadata CurrentTypeMetadata { get; }
        private IList<BsonDocument> Pipeline { get; set; }
        private IQueryProvider Provider { get; }
        private Expression DefaultExpression { get; }

        public EagerLoadingQueryProvider(IMongoDatabase database, ITypeInitializer typeInitializer)
        {
            this.Database = database;
            this.TypeInitializer = typeInitializer;
            CurrentTypeMetadata = typeInitializer.GetTypeMetadata<T>();
            var queryable = this.Database.GetCollection<T>(this.CurrentTypeMetadata.CollectionName).AsQueryable();
            Provider = queryable.Provider;
            DefaultExpression = queryable.Expression;
        }

        public IQueryable<T> CreateQuery(IList<BsonDocument> pipeLine)
        {
            this.Pipeline = pipeLine;
            return CreateQuery<T>(DefaultExpression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return this.CreateQuery<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new MongoQueryable<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            if (this.Pipeline == null)
            {
                return Provider.Execute(expression);
            }

            var t = Provider.GetMethod("Translate", BindingFlags.NonPublic | BindingFlags.Instance);

            var queryTranslation = Provider.GetMethod("Translate", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(Provider, new object[] { expression });

            var executionPlan = TranslationHelper.BuildPlan<T>(
                Expression.Constant(Provider),
                queryTranslation, this.Pipeline);

            var lambda = Expression.Lambda(executionPlan);

            try
            {
                return lambda.Compile().DynamicInvoke(null);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)this.Execute(expression);
        }
    }
}
