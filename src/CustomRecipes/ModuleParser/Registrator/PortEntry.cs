using Mafi.Core.Products;

namespace CustomRecipes.ModuleParser.Registrator
{
    public class PortEntry
    {
        public PortEntry(char v, ProductType allowedProductType)
        {
            this.Name = v.ToString();
            this.Type = allowedProductType;
        }

        public ProductType Type { get; }
        public string Name { get; }
        public bool Used { get; set; }
    }
}