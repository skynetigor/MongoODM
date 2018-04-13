using System;
using System.Collections.Generic;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    abstract class AbstractCacheableServiceProvider<TServiceType> where TServiceType :class 
    {
        protected abstract IDictionary<string, Type> TypesDictionary { get; }
        protected IDictionary<Type, TServiceType> CachedInstances { get; }

        protected AbstractCacheableServiceProvider()
        {
            CachedInstances = new Dictionary<Type, TServiceType>();
        }

        protected abstract object CreateInstance(Type instanceType);

        protected TServiceType GetServiceInstance(Type type, Type defaultType)
        {
            Type currentType = type;

            if (CachedInstances.TryGetValue(currentType, out var loaderInstance))
            {
                return loaderInstance as TServiceType;
            }

            if (currentType.IsGenericType)
            {
                loaderInstance = GetServiceForGeneric(currentType) as TServiceType;
            }
            else
            {
                loaderInstance = CreateInstance(defaultType) as TServiceType;
            }

            if (loaderInstance != null)
            {
                CachedInstances.Add(currentType, loaderInstance);
                return (TServiceType)loaderInstance;
            }

            return null;
        }

        private object GetServiceForGeneric(Type discriminator)
        {
            Type genericTypeDefinition = discriminator.GetGenericTypeDefinition();

            if (TypesDictionary.TryGetValue(genericTypeDefinition.ToString(), out Type type))
            {
                return CreateInstance(type.MakeGenericType(discriminator.GetGenericArguments()[0]));
            }

            return null;
        }
    }
}
