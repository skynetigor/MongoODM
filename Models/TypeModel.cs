using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MongoODM.Models
{
    public class TypeMetadata
    {
        public TypeMetadata()
        {
            this.QueryDictionary = new Dictionary<string, IEnumerable<BsonDocument>>();
        }

        public string CollectionName { get; set; }

        public PropertyInfo IdProperty { get; set; }

        public Type CurrentType { get; set; }

        public BsonDocument[] Query { get; set; }

        public IDictionary<string, IEnumerable<BsonDocument>> QueryDictionary { get; set; }
    }
}
