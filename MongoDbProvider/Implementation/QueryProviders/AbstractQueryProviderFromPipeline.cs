using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders
{
    abstract class AbstractQueryProviderFromPipeline<T> : IQueryProviderFromPipeline<T>
    {
        private IList<BsonDocument> Pipeline { get; set; }
        private Expression DefaultExpression { get; }

        protected IQueryProvider Provider { get; }

        protected AbstractQueryProviderFromPipeline(IMongoDatabase database, ITypeInitializer typeInitializer)
        {
            var currentTypeMetadata = typeInitializer.GetTypeMetadata<T>();
            var queryable = database.GetCollection<T>(currentTypeMetadata.CollectionName).AsQueryable();
            Provider = queryable.Provider;
            DefaultExpression = queryable.Expression;
        }

        public IQueryable<T> CreateQueryFromPipeline(IList<BsonDocument> pipeLine)
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
            return new MongoDbQueryable<TElement>(this, expression);
        }

        public virtual object Execute(Expression expression)
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
