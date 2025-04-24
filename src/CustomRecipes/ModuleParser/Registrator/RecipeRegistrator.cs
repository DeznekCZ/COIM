using CustomRecipes.Data.Mod;
using CustomRecipes.Python;
using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.Terrain.Generation;
using Mafi.Core.UnlockingTree;
using Mafi.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
                ["ResearchCostsTpl"] = typeof(ResearchCostsTpl),
                #endregion

                ["add_texture"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    string basePath = typeof(RecipeRegistrator).Assembly.Location;
                    string assetPath = GetArgument<string>("path").ElseRequiredThrow();
                    byte[] image = File.ReadAllBytes(Path.Combine(basePath, "..", assetPath));
                    if (!texture2D.LoadImage(image))
                    {
                        throw new ArgumentException($"Could not load an image: {assetPath}");
                    }

                    if (GetArgument<string>("replace").ElseNotExists(out string replacementAssetPath))
                        assetPath = replacementAssetPath;

                    CustomAssetManager.Alternations.Add(assetPath, texture2D);
                    return null;
                }, new[] { "path", "replace" }),

                #region Build research TODO
                ["build_research"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    var builder = registrator.ResearchNodeProtoBuilder.Start(
                        name: GetArgument<string>("name")
                                  .ElseRequiredThrow(),
                        nodeId: GetArgument<ResearchNodeProto.ID>("researchId")
                                    .When<string>(s => new ResearchNodeProto.ID(s))
                                    .ElseRequiredThrow());

                    builder.SetCosts(GetArgument<ResearchCostsTpl>("costs")
                        .When<int>(diff => new ResearchCostsTpl.Builder().SetDifficulty(diff))
                        .When<List<object>>(list => throw new NotImplementedException())
                        .ElseDefault(new ResearchCostsTpl.Builder().SetDifficulty(1)));

                    builder.SetGridPosition(
                            GetArgument<Vector2i>("position")
                                .When<(int x, int y)>(pt => new Vector2i(pt.x, pt.y))
                                .ElseDefault(Vector2i.Zero)
                        );

                    if (GetArgument<List<ResearchNodeProto>>("parents")
                        .When<List<object>>(o =>
                        {
                            List<ResearchNodeProto> protosCollector = new List<ResearchNodeProto>();
                            foreach (object item in o)
                            {
                                if (item is ResearchNodeProto proto)
                                    protosCollector.Add(proto);
                                else if (item is ResearchNodeProto.ID id)
                                    protosCollector.Add(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id));
                                else if (item is string sid)
                                    protosCollector.Add(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(sid)));
                                else
                                    throw new ProtoBuilderException($"Proto '{item}' was not found.");
                            }
                            return protosCollector;
                        }).ElseNotExists(out var protos))
                    {
                        builder.AddParents(protos.ToArray());
                    }

                    if (GetArgument<string>("icon").ElseNotExists(out string path))
                    {
                        builder.AddIcon(Option.None, path);
                    }

                    return builder.BuildAndAdd();
                }, new[] { "researchId", "name", "description", "difficulty", "position", "parents" }),
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
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    RecipeProto recipe = GetArgument<RecipeProto>("recipe")
                                            .When<RecipeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<RecipeProto>(id))
                                            .When<string>(id => registrator.PrototypesDb.GetOrThrow<RecipeProto>(new RecipeProto.ID(id)))
                                            .ElseRequiredThrow();
                    MachineProto machine = GetArgument<MachineProto>("machine")
                                               .When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
                                               .When<string>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(id)))
                                               .ElseRequiredThrow();
                    ResearchNodeProto research = GetArgument<ResearchNodeProto>("research")
                                                    .When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
                                                    .When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
                                                    .ElseRequiredThrow();

                    typeof(ResearchNodeProto).GetField("Units", BindingFlags.Public | BindingFlags.Instance)
                                             .SetValue(research, research.Units
                                                                         .AsEnumerable()
                                                                         .Concat(new IUnlockNodeUnit[] { new RecipeUnlock(recipe, machine, false) })
                                                                         .ToImmutableArray());
                    return null;
                }, new[] { "research", "machine", "recipe" }),

                ["add_unlock_machine"] = new Constructor((args) =>
                {
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

                    ResearchNodeProto research = GetArgument<ResearchNodeProto>("research")
                                                    .When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
                                                    .When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
                                                    .ElseRequiredThrow();

                    typeof(ResearchNodeProto).GetField("Units", BindingFlags.Public | BindingFlags.Instance)
                                             .SetValue(research, research.Units
                                                                         .AsEnumerable()
                                                                         .Concat(new IUnlockNodeUnit[] { new ProtoWithIconUnlock(machine, false) })
                                                                         .ToImmutableArray());

                    typeof(ResearchNodeProto.Gfx).GetField("<Icons>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                                                 .SetValue(research.Graphics, research.Graphics.Icons
                                                                                               .AsEnumerable()
                                                                                               .Concat(new KeyValuePair<Option<Proto>, string>[] { new KeyValuePair<Option<Proto>, string>(machine, machine.IconPath) })
                                                                                               .ToImmutableArray());
                    return null;
                }, new[] { "research", "machine" }),
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
