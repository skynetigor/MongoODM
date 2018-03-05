using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace MongoODM.Models
{
    public class TypeModel
    {
        public TypeModel()
        {
            this.QueryDictionary = new Dictionary<string, IEnumerable<BsonDocument>>();
        }

        public string CollectionName { get; set; }

        public string IdName { get; set; }

        public Type CurrentType { get; set; }

        public BsonDocument[] Query { get; set; }

        public IDictionary<string, IEnumerable<BsonDocument>> QueryDictionary { get; set; }
    }
}
