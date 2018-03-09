using System;
using Microsoft.Extensions.DependencyInjection;

namespace MongoODM.DI.Abstract
{
    interface ICustomServiceCollection: IServiceCollection
    {
        ICustomServiceProvider BuildServiceProvider();
    }
}
