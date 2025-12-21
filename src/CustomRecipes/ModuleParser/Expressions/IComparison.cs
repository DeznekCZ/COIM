namespace CustomAssets.Python
{
    public interface IComparison
    {
        IExpression Left { get; }
        IExpression Right { get; }
    }
}