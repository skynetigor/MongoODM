using DbdocFramework.Abstracts;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface IState<in TEntity>: IDataProcessor<TEntity>, IChangesSaver where TEntity: class 
    {

    }
}
