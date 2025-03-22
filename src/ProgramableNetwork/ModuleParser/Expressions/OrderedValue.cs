namespace ProgramableNetwork.Python
{
    public class OrderedValue : IArgumentValue
    {
        private object value;

        public OrderedValue(object value)
        {
            this.value = value;
        }

        public object Value => value;
    }
}