using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoODM.Extensions;

namespace MongoODM.Helpers
{
    public static class TranslationHelper
    {
        private static FieldInfo ModelField { get; set; }

        private static MethodInfo BuildPlanMethod { get; set; }

        private static FieldInfo StagesField { get; set; }

        public static Expression BuildPlan<T>(Expression provider, object translation, IList<BsonDocument> pipeline)
        {
            if (ModelField == null && BuildPlanMethod == null)
            {
                var assembly = typeof(IMongoDatabase).Assembly;
                ModelField = translation.GetPrivateField("_model");
                BuildPlanMethod = assembly.GetTypes()
                    .FirstOrDefault(type => type.Name == "ExecutionPlanBuilder")
                    .GetMethod("BuildPlan", BindingFlags.Public | BindingFlags.Static);
            }

            var modelvalue = ModelField.GetValue(translation);

            StagesField = modelvalue.GetPrivateField("_stages");

            pipeline = pipeline.Concat((IEnumerable<BsonDocument>)StagesField.GetValue(modelvalue)).ToList().AsReadOnly();

            StagesField.SetValue(modelvalue, pipeline);

            return (Expression)BuildPlanMethod.Invoke(null, new object[] { provider, translation });
        }
    }
}
