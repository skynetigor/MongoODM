using System;
using System.Collections.Generic;
using System.Text;
using DbdocFramework.DI.Abstract;

namespace DbdocFramework.DI.Extensions
{
    public  static class ICustomServiceProviderExtensions
    {
        public static T CreateInstance<T>(this ICustomServiceProvider serviceProvider)
        {
            return (T)serviceProvider.CreateInstance(typeof(T));
        }
    }
}
