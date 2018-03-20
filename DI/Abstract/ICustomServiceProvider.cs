using System;
using DbdocFramework.Abstracts;

namespace DbdocFramework.DI.Abstract
{
    public interface ICustomServiceProvider: IServiceProvider
    {
        object CreateInstance(Type instanceType);
    }
}
