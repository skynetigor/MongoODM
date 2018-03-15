using System;
using Castle.DynamicProxy;
using DbdocFramework.MongoDbProvider.Abstracts;

namespace DbdocFramework.MongoDbProvider.LazyLoading
{
    internal class LazyLoadingInterceptor : IMongoDbLazyLoadingInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.ReturnValue =
                invocation.MethodInvocationTarget.Invoke(invocation.InvocationTarget, invocation.Arguments);
        }
    }
}
