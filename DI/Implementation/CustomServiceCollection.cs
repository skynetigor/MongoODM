using System;
using Microsoft.Extensions.DependencyInjection;
using DbdocFramework.DI.Abstract;

namespace DbdocFramework.DI.Implementation
{
    internal class CustomServiceCollection: ServiceCollection, ICustomServiceCollection
    {
        public ICustomServiceProvider BuildServiceProvider()
        {
            return new CustomServiceProvider(this);
        }
    }
}
