using PythonAPI.Expressions;

namespace PythonAPI.Arguments
{
    public interface IArgument
    {
        IExpression Expression { get; }
    }
}