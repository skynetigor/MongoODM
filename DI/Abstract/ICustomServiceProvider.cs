using System;

namespace MongoODM.DI.Abstract
{
    public interface ICustomServiceProvider: IServiceProvider
    {
        object CreateInstance(Type instanceType);
    }
}
