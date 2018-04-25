using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.Abstracts
{
    public interface IProvider
    {
        void RegisterModel<TModel>() where TModel : class;

        void SaveChanges();

        IDbSet<TModel> GetDbSet<TModel>() where TModel : class;
    }
}
