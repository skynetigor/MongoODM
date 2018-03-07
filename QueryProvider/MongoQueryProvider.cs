using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoODM.Extensions;
using MongoODM.Helpers;

namespace MongoODM.QueryProvider
{
    public class MongoQueryProvider<T> : IQueryProvider
    {
        private static MethodInfo ProviderMethod { get; set; }

        private IList<BsonDocument> Pipeline { get; }

        private IQueryProvider Provider { get; }

        public MongoQueryProvider(IQueryProvider queryProvider, IList<BsonDocument> pipeLine)
        {
            this.Provider = queryProvider;
            this.Pipeline = pipeLine;
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
                return this.Provider.Execute(expression);
            }

            var queryTranslation = this.Provider.GetMethod("Translate", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(Provider, new object[] { expression });

            var executionPlan = TranslationHelper.BuildPlan<T>(
                Expression.Constant(this.Provider),
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
