using System;
using Microsoft.Extensions.DependencyInjection;
using MongoODM.DI.Abstract;

namespace MongoODM.DI.Implementation
{
    internal class CustomServiceCollection: ServiceCollection, ICustomServiceCollection
    {
        public ICustomServiceProvider BuildServiceProvider()
        {
            return new CustomServiceProvider(this);
        }
    }
}
