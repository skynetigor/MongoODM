using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbdocFramework.DI.Abstract;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using Microsoft.Extensions.DependencyInjection;

namespace DbdocFramework.MongoDbProvider.Implementation.State
{
    internal class StateManager: IStateManager
    {
        private readonly IList<IChangesSaver> _states = new List<IChangesSaver>();

        private ICustomServiceProvider ServiceProvider { get; }

        public StateManager(ICustomServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void SaveChanges()
        {
            _states.ForEach(st => st.SaveChanges());
        }

        public IState<T> GetOrCreateState<T>() where T : class
        {
            var state = _states.FirstOrDefault(st => st is IState<T>);

            if (state == null)
            {
                state = ServiceProvider.GetService<IState<T>>();
                _states.Add(state);
            }

            return (IState<T>)state;
        }
    }
}
