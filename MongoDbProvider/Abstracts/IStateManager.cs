using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface IStateManager: IChangesSaver
    {
        IState<T> GetOrCreateState<T>() where T : class;
    }
}
