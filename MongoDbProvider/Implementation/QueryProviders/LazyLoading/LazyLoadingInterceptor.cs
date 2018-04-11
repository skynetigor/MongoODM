using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;
using DbdocFramework.DI.Abstract;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading
{
    internal class LazyLoadingInterceptor : ILazyLoadingInterceptor
    {
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeMetadata { get; }
        private ICustomServiceProvider ServiceProvider { get; }
        private ProxyGenerator ProxyGenerator { get; }
        private IDataLoadersProvider DataLoadersProvider { get; }
        private MethodInfo LoadDataGenericMethodInfo { get; }

        public LazyLoadingInterceptor(IMongoDatabase database, ITypeInitializer typeMetadata, ICustomServiceProvider serviceProvider, IDataLoadersProvider dataLoadersProvider)
        {
            Database = database;
            TypeMetadata = typeMetadata;
            ServiceProvider = serviceProvider;
            this.DataLoadersProvider = dataLoadersProvider;
            ProxyGenerator = new ProxyGenerator();

            LoadDataGenericMethodInfo = this.GetPrivateMethod(nameof(LoadDataGeneric));
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("get_") && !invocation.Method.ReturnType.IsPrimitive)
            {
                var value = invocation.MethodInvocationTarget.Invoke(invocation.InvocationTarget, invocation.Arguments);

                if (IsTypeRegistered(invocation.Method.ReturnType))
                {
                    if (value == null || !ProxyUtil.IsProxy(value))
                    {
                        var invokedProperty = invocation.TargetType.GetProperty(invocation.Method.Name.Substring(4));
                        value = this.LoadData(invocation.InvocationTarget, invokedProperty);
                        invokedProperty.SetValue(invocation.InvocationTarget, value);
                    }
                }
            }

            invocation.Proceed();
        }

        private object LoadData(object obj, PropertyInfo invokedProp)
        {
            return LoadDataGenericMethodInfo
                 .MakeGenericMethod(obj.GetType(), invokedProp.PropertyType)
                 .Invoke(this,
                     new object[] { obj, invokedProp });
        }

        private TResult LoadDataGeneric<TSource, TResult>(TSource source, PropertyInfo loadedProperty)
        {
            return this.DataLoadersProvider.GetDataLoader<TResult>().LoadData(source, loadedProperty);
        }

        private bool IsTypeRegistered(Type type)
        {
            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();

                if (genericDefinition == typeof(ICollection<>) || genericDefinition == typeof(IList<>))
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            return TypeMetadata.IsTypeRegistered(type);
        }
    }
}
