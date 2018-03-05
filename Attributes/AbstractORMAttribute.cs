using MongoODM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoODM.Attributes
{
    public abstract class AbstractORMAttribute : Attribute
    {
        protected abstract void Map(TypeModel model);
    }
}
