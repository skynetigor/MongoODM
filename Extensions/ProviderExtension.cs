using System;
using System.Reflection;
using DbdocFramework.Abstracts;

namespace DbdocFramework.Extensions
{
    public static class ProviderExtension
    {
        private const string ProviderNull = "Provider is null.";

        public static object GetDbSet(this IProvider provider, Type modelType)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(ProviderNull);
            }

            MethodInfo getDbSet = provider.GetType()
                .GetMethod(nameof(provider.GetDbSet))
                .MakeGenericMethod(modelType);
            return getDbSet.Invoke(provider, null);
        }

        public static void RegisterModel(this IProvider provider, Type modelType)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(ProviderNull);
            }

            MethodInfo registerModelMethod = provider.GetType()
                .GetMethod(nameof(provider.RegisterModel))
                .MakeGenericMethod(modelType);

            registerModelMethod.Invoke(provider, null);
        }
    }
}
