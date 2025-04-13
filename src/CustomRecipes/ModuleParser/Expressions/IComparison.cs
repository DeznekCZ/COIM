namespace CustomRecipes.Python
{
    public interface IComparison
    {
        IExpression Left { get; }
        IExpression Right { get; }
    }
}