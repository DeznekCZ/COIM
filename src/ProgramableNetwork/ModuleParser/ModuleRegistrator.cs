using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class ModuleRegistrator
    {
        public static void Register(ProtoRegistrator registrator, Block block)
        {
            foreach (Class classEntry in block.classes.Values)
            {
                if (!classEntry.baseClasses.Any(b => b.Concat == "Module"))
                    continue;

                var builder = registrator.ModuleBuilderStart(classEntry.Name);
                var variables = classEntry.Variables;

                AddIO(builder, variables, "inputs", "Input", builder.AddInput);
                AddIO(builder, variables, "outputs", "Output", builder.AddOutput);

                // TODO search for variable of device and categories
                builder.SetName(classEntry.Name);
                builder.SetSymbol(classEntry.Name.Substring(3));
                builder.SetGfx(Assets.Base.Products.Icons.Vegetables_svg);
                builder.AddControllerDevice();

                builder.BuildAndAdd();
            }
        }

        private static void AddIO(ModuleProto.Builder builder, Dictionary<string, IExpression> variables, string listName, string constructorName, Func<string, string, ModuleProto.Builder> add)
        {
            if (variables.TryGetValue(listName, out IExpression expression) && expression is ListValue list)
            {
                foreach (IExpression variable in list)
                {
                    if (variable is Call call && call.Name == constructorName)
                    {
                        List<IExpression> arguments = call.Arguments;
                        add(arguments[0].StringValue, arguments[1].StringValue);
                    }
                }
            }
        }
    }
}
