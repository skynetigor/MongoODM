using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.Abstracts
{
    public interface IProvider
    {
        void RegisterModel<TModel>() where TModel : class;
        IDbSet<TModel> GetDbSet<TModel>() where TModel : class;
    }
}
