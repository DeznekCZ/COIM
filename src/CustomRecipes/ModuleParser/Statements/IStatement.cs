using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public interface IStatement
    {
        void Execute(IDictionary<string, object> context);
    }
}