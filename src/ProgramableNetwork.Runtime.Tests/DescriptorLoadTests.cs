using System.Linq;
using ProgramableNetwork.Runtime.Sim;
using Xunit;

namespace ProgramableNetwork.Runtime.Tests
{
    /// <summary>
    /// Validates that an `ids.py`-style descriptor (a Module-derived class WITHOUT an action, using the
    /// normal constructors) loads into a full ModuleDefinition — this is how the C#-defined modules
    /// reach the web app. No game required.
    /// </summary>
    public class DescriptorLoadTests
    {
        private const string Ids = @"
from Core.module import Module
from Core.categories import DefaultCategories
from Core.io import Input, Output, Display
from Core.fields import Int32Field, Fix32Field, Int64Field, BooleanField, StringField

class Sum(Module):
    name = ""Sum: A + B""
    symbol = ""A+B""
    width = 1
    description = ""Adds inputs""
    categories = [DefaultCategories.Arithmetic]
    inputs = [Input(""a"", ""A""), Input(""b"", ""B"")]
    outputs = [Output(""c"", ""Sum"")]
    fields = [Int32Field(""k"", ""K"", """", 5), BooleanField(""flag"", ""Flag"", """", True)]
    displays = [Display.LED(""led"", ""On""), Display.Text(""t"", ""Val"", """")]
";

        [Fact]
        public void Descriptor_module_loads_as_metadata_only()
        {
            var defs = ModuleLoader.LoadFile("ids.py", Ids);
            ModuleDefinition sum = Assert.Single(defs);

            Assert.Equal("Sum", sum.Id);
            Assert.Equal("Sum: A + B", sum.Name);
            Assert.Equal("A+B", sum.Symbol);
            Assert.Equal(1, sum.Width);
            Assert.False(sum.HasBehavior); // no action -> preview-only / inert in sim

            Assert.Equal(new[] { "a", "b" }, sum.Inputs.Select(p => p.Id).ToArray());
            Assert.Equal(new[] { "c" }, sum.Outputs.Select(p => p.Id).ToArray());
            Assert.Contains(sum.Fields, f => f.Id == "k" && f.Kind == FieldKind.Int32);
            Assert.Contains(sum.Fields, f => f.Id == "flag" && f.Kind == FieldKind.Boolean);
            Assert.Contains(sum.Displays, d => d.Id == "led" && d.Kind == ProgramableNetwork.ModuleProto.DisplayKind.Led);
            Assert.Contains(sum.Categories, c => c == "arithmetic");
        }
    }
}
