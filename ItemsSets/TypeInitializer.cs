using MongoODM.Abstracts;
using MongoODM.Attributes;
using MongoODM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoODM.ItemsSets
{
    internal class TypeInitializer : ITypeInitializer
    {
        const string CollectionPostfix = "s";
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
                method.Invoke(attr, new object[] { model, type });
            }

            this.SetProperties(type, model);
            return model;
        }

        private void SetProperties(Type type, TypeModel model)
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
