using System;
using Microsoft.Extensions.DependencyInjection;

namespace DbdocFramework.DI.Abstract
{
    interface ICustomServiceCollection: IServiceCollection
    {
        ICustomServiceProvider BuildServiceProvider();
    }
}
