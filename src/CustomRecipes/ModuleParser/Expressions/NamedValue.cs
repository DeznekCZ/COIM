namespace CustomRecipes.Python
{
    public class NamedValue : IArgumentValue
    {
        private string name;
        private object value;

        public NamedValue(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public object Value => value;
        public string Name => name;
    }
}