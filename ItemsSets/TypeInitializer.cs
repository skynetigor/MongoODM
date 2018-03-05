using MongoORM.Abstracts;
using MongoORM.Attributes;
using MongoORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoORM.ItemsSets
{
    internal class TypeInitializer : ITypeInitializer
    {
        const string CollectionPrefix = "s";
        private static Dictionary<Type, TypeModel> dictionary = new Dictionary<Type, TypeModel>();

        public TypeModel InitializeType<T>()
        {
            return this.InitializeType(typeof(T));
        }

        public TypeModel InitializeType(Type type)
        {
            TypeModel typeModel;

            if (!dictionary.TryGetValue(type, out typeModel))
            {
                typeModel = Setup(type);
                typeModel.CurrentType = type;
                dictionary[type] = typeModel;
            }

            return typeModel;
        }

        public TypeModel GetTypeModel<T>()
        {
            return this.GetTypeModel(typeof(T));
        }

        public TypeModel GetTypeModel(Type type)
        {
            TypeModel typeModel;

            if (!dictionary.TryGetValue(type, out typeModel))
            {
                return null;
            }

            return typeModel;
        }

        private TypeModel Setup(Type type)
        {
            var model = new TypeModel();

            var attributes = type.GetCustomAttributes<AbstractORMAttribute>();
            foreach (var attr in attributes)
            {
                var method = attr.GetType().GetMethod("Map", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(attr, new[] { model });
            }
            this.SetProperties(type, model);
            return model;
        }

        private void SetProperties(Type type, TypeModel model)
        {
            if (string.IsNullOrEmpty(model.CollectionName))
            {
                model.CollectionName = type.Name + CollectionPrefix;
            }

            if (string.IsNullOrEmpty(model.IdName))
            {
                var props = type.GetProperties();
                var id = type.GetProperties().FirstOrDefault(
                    prop => prop.Name.ToLower() == (type.Name + "id").ToLower()
                || prop.Name.ToLower() == "id".ToLower()).Name;

                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new MissingMemberException();
                }
                model.IdName = id;
            }
        }
    }
}
