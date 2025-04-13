using System;
using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public class Class
    {
        public readonly string name;
        public readonly Type[] baseTypes;
        public readonly IDictionary<string, object> classContext;

        public Class(string name, Type[] baseTypes, IDictionary<string, object> classContext)
        {
            this.name = name;
            this.baseTypes = baseTypes;
            this.classContext = classContext;
        }
    }
}