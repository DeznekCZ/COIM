namespace PythonAPI.Expressions
{
    public interface IComparison
    {
        IExpression Left { get; }
        IExpression Right { get; }
    }
}