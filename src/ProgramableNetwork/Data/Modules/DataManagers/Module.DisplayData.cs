namespace ProgramableNetwork
{
    public partial class Module
    {
        public class DisplayData
        {
            private Module module;

            internal DisplayData(Module module)
            {
                this.module = module;
            }

            public string this[string name, string defaultValue]
            {
                get => module.StringData.TryGetValue("display__" + name, out string data)
                    ? data ?? defaultValue : defaultValue;
            }

            public string this[string name]
            {
                set
                {
                    module.StringData["display__" + name] = value;
                }
            }
        }
    }
}