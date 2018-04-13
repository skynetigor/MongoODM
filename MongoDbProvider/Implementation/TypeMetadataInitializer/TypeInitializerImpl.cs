using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Implementation.TypeMetadataInitializer
{
    internal class TypeInitializerImpl : ITypeInitializer
    {
        public void RegisterType<T>()
        {
            TypeInitializerStatic.RegisterType(typeof(T));
        }

        public void RegisterType(Type type)
        {
            TypeInitializerStatic.RegisterType(type);
        }

        public bool IsTypeRegistered(Type type)
        {
            return TypeInitializerStatic.IsTypeRegistered(type);
        }

        public TypeMetadata GetTypeMetadata<T>()
        {
            return TypeInitializerStatic.GetTypeMetadata(typeof(T));
        }

        public TypeMetadata GetTypeMetadata(Type type)
        {
            return TypeInitializerStatic.GetTypeMetadata(type);
        }
    }
}
