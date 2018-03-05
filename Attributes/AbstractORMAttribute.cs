using MongoORM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoORM.Attributes
{
    public abstract class AbstractORMAttribute : Attribute
    {
        protected abstract void Map(TypeModel model);
    }
}
