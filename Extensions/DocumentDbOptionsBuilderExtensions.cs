using System;
using System.Collections.Generic;
using System.Text;
using DbdocFramework.Abstracts;
using DbdocFramework.MongoDbProvider.Settings;

namespace DbdocFramework.Extensions
{
    public static class DocumentDbOptionsBuilderExtensions
    {
        public static IProvider UseMongoDb(this DocumentDbOptionsBuilder builder, string connectionString)
        {
            return new MongoDbProvider.MongoDbProvider(new MongoDbContextSettings(connectionString));
        }
    }
}
