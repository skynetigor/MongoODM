using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MongoODM.DI.Abstract;

namespace MongoODM.DI.Implementation
{
    internal class CustomServiceProvider: ICustomServiceProvider
    {
        private const string UnKnownParameterError = "Unable to find suitable constructor for type \"{0}\"";

        private IServiceProvider ServiceProvider { get; }

        public CustomServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ICustomServiceProvider>(this);
            this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public object GetService(Type serviceType)
        {
            return this.ServiceProvider.GetService(serviceType);
        }

        public object CreateInstance(Type instanceType)
        {
            var constructorInfo = instanceType.GetConstructors();

            object instance = null;

            for (int i = constructorInfo.Length - 1; i >= 0; i--)
            {
                instance = this.CreateInstance(constructorInfo[i], instanceType);

                if (instance != null)
                {
                    return instance;
                }
            }

            throw new InvalidOperationException(string.Format(UnKnownParameterError, instanceType.Name));
        }

        private object CreateInstance(ConstructorInfo constructorInfo, Type instanceType)
        {
            var valuesList = new List<object>();
            var ctorParameterInfos = constructorInfo.GetParameters();
            var ctorParametersValues = new object[ctorParameterInfos.Length];

            for (int i = 0; i < ctorParameterInfos.Length; i++)
            {
                var ctorParameter = ctorParameterInfos[i];
                var instance = this.ServiceProvider.GetService(ctorParameter.ParameterType);

                if (instance == null)
                {
                    return null;
                }

                ctorParametersValues[i] = instance;
            }

            return Activator.CreateInstance(instanceType, ctorParametersValues);
        }
    }
}
