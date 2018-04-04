using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Implementation
{
    internal class TypeInitializer : ITypeInitializer
    {
        const string CollectionPostfix = "s";

        private Dictionary<Type, TypeMetadata> DictionaryTypeMetadata { get; } = new Dictionary<Type, TypeMetadata>();
        private ICustomServiceProvider ServiceProvider { get; }

        public TypeInitializer(ICustomServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public TypeMetadata RegisterType<T>()
        {
            return this.RegisterType(typeof(T));
        }

        public TypeMetadata RegisterType(Type type)
        {
            if (!DictionaryTypeMetadata.TryGetValue(type, out var typeModel))
            {
                typeModel = Setup(type);
                typeModel.CurrentType = type;
                DictionaryTypeMetadata[type] = typeModel;
            }

            return typeModel;
        }

        public bool IsTypeRegistered(Type type)
        {
            return this.GetTypeMetadata(type) != null;
        }

        public TypeMetadata GetTypeMetadata<T>()
        {
            return this.GetTypeMetadata(typeof(T));
        }

        public TypeMetadata GetTypeMetadata(Type type)
        {
            if (!DictionaryTypeMetadata.TryGetValue(type, out var typeModel))
            {
                return null;
            }

            return typeModel;
        }

        private TypeMetadata Setup(Type type)
        {
            var model = new TypeMetadata();

            var attributes = type.GetCustomAttributes<Attribute>().OfType<ITypeMetadata>();

            foreach (var attr in attributes)
            {
                attr.Map(model, type, this.ServiceProvider);
            }

            this.SetProperties(type, model);
            return model;
        }

        private void SetProperties(Type type, TypeMetadata model)
        {
            if (string.IsNullOrEmpty(model.CollectionName))
            {
                model.CollectionName = type.Name + CollectionPostfix;
            }

            if (model.IdProperty == null)
            {
                model.IdProperty = type.GetProperties().FirstOrDefault(
                    prop => prop.Name.ToLower() == (type.Name + "id").ToLower()
                            || prop.Name.ToLower() == "id".ToLower());
                return; ;
            }

            throw new MissingMemberException();
        }
    }
}
