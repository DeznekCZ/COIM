using CustomRecipes.Python;
using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.UnlockingTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomRecipes.ModuleParser.Registrator
{
    internal class RecipeRegistrator
    {
        public static void Register(ProtoRegistrator registrator, string file)
        {
            Token[] tokens = Tokenizer.ParseFile(file);
            Block block = Lexer.Parse(tokens);

            Dictionary<string, object> context = new Dictionary<string, object>
            {
                #region Types
                ["RecipeProto"] = typeof(RecipeProto),
                ["MachineProto"] = typeof(MachineProto),
                ["ProductProto"] = typeof(ProductProto),
                ["ResearchNodeProto"] = typeof(ResearchNodeProto),
                ["Duration"] = typeof(Duration),
                ["Quantity"] = typeof(Quantity),
                ["Proto"] = typeof(Proto),
                #endregion

                #region Build research TODO
                ["build_research"] = new Constructor((args) =>
                {
                    throw new NotImplementedException();
                }, new[] { "research" }),
                #endregion

                #region Define product
                ["Product"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    return new Product()
                    {
                        product = GetArgument<ProductProto>("product")
                                      .When<ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ProductProto>((Proto.ID)id))
                                      .When<string>(id => registrator.PrototypesDb.GetOrThrow<ProductProto>((Proto.ID)new ProductProto.ID(id)))
                                      .ElseRequiredThrow(),
                        quantity = GetArgument<Quantity>("quantity")
                                       .When<int>(v => new Quantity(v))
                                       .ElseRequiredThrow(),
                        port = GetArgument<string>("port")
                                   .ElseDefault("*"),
                    };
                }, new[] { "product", "quantity", "port" }),
                #endregion

                #region Build recipe
                ["build_recipe"] = new Constructor((args) =>
                {
                    string[] positional = new string[]
                    {
                        "recipeId",
                        "name",
                        "description",
                        "machine",
                        "research",
                        "duration",
                        "ingredients",
                        "products"
                    };

                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    MachineProto machine = GetArgument<MachineProto>("machine")
                                               .When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
                                               .When<string>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(id)))
                                               .ElseRequiredThrow();
                    RecipeProtoBuilder.State builder = registrator.RecipeProtoBuilder
                        .Start(
                            name: GetArgument<string>("name").ElseRequiredThrow(),
                            recipeId: 
                                GetArgument<RecipeProto.ID>("recipeId")
                                    .When<string>(v => new RecipeProto.ID(v))
                                    .ElseRequiredThrow(),
                            machine: machine
                        );

                    if (GetArgument<string>("description").ElseNotExists(out var description))
                        builder.Description(description);

                    if (GetArgument<List<object>>("ingredients").ElseNotExists(out var ingredientsList))
                        ingredientsList.Select(e => (Product)e)
                                       .Call(e => builder.AddInput(
                                           portSelector: e.port,
                                           product: e.product,
                                           quantity: e.quantity
                                       ))
                                       .ToArray(); // invoke ling actions

                    if (GetArgument<List<object>>("products").ElseNotExists(out var productsList))
                    {
                        PortEntry[] ports = machine.Ports
                            .Where(p => p.Spec.Type == IoPortType.Output)
                            .Select(p => new PortEntry(p.Name, p.Shape.AllowedProductType))
                            .ToArray();
                        productsList.Select(e => (Product)e)
                                    .Call(e => builder.AddOutput(
                                        portSelector: e.port != "*"
                                            ? ports
                                                .Where(p => p.Name == e.port)
                                                .Where(p => p.Type == e.product.Type)
                                                .Where(p => !p.Used)
                                                .Select(p =>
                                                {
                                                    p.Used = true;
                                                    return p.Name;
                                                })
                                                .FirstOrDefault() ?? throw new ArgumentException($"Port '{e.port}' is already used")
                                            : ports
                                                .Where(p => p.Type == e.product.Type)
                                                .Where(p => !p.Used)
                                                .Select(p =>
                                                {
                                                    p.Used = true;
                                                    return p.Name;
                                                })
                                                .FirstOrDefault() ?? throw new ArgumentException($"Cannot get empty port for product: {e.product.Id.Value}"),
                                        product: e.product,
                                        quantity: e.quantity
                                    ))
                                    .ToArray(); // invoke ling actions
                    }

                    builder.SetDuration(GetArgument<Duration>("duration").ElseDefault(Duration.FromSec(60)));

                    RecipeProto recipe = builder.BuildAndAdd();
                    if (GetArgument<ResearchNodeProto>("research")
                        .When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
                        .When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
                        .ElseNotExists(out ResearchNodeProto research))
                    {
                        typeof(ResearchNodeProto).GetField("Units", BindingFlags.Public | BindingFlags.Instance)
                                                 .SetValue(research, research.Units
                                                                             .AsEnumerable()
                                                                             .Concat(new IUnlockNodeUnit[] { new RecipeUnlock(recipe, machine, false) })
                                                                             .ToImmutableArray());
                    }

                    return recipe;
                }, new string[]
                {
                    "recipeId",
                    "name",
                    "description",
                    "machine",
                    "research",
                    "duration",
                    "ingredients",
                    "products"
                }),
                #endregion

                #region Unlock
                ["add_unlock_recipe"] = new Constructor((args) =>
                {
                    RecipeProto recipe = (RecipeProto)args[1].Value;
                    MachineProto machine = (MachineProto)args[2].Value;

                    ResearchNodeProto research = registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>((ResearchNodeProto.ID)args[0].Value);
                    typeof(ResearchNodeProto).GetField("Units", BindingFlags.Public | BindingFlags.Instance)
                                             .SetValue(research, research.Units
                                                                         .AsEnumerable()
                                                                         .Concat(new IUnlockNodeUnit[] { new RecipeUnlock(recipe, machine, false) })
                                                                         .ToImmutableArray());
                    return null;
                }, new[] { "research", "machine", "recipe" }),
                #endregion

                ["recipe_id"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    return GetArgument<RecipeProto.ID>("recipeId")
                               .When<RecipeProto>(r => r.Id)
                               .When<string>(id => new RecipeProto.ID(id))
                               .ElseRequiredThrow();
                }, new[] { "recipeId" })
            };

            foreach (IStatement statement in block.statements)
            {
                statement.Execute(context);
            }
        }
    }

    public struct Product
    {
        public string port;
        public ProductProto product;
        public Quantity quantity;
    }
}
