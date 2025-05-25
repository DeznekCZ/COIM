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
using Mafi.Core.Population.Edicts;
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
                ["Vector3i"] = typeof(Vector3i),
                ["Vector3f"] = typeof(Vector3f),
                ["Vector2i"] = typeof(Vector2i),
                ["Vector2f"] = typeof(Vector2f),
                ["Prefab"] = typeof(Prefab), //TODO
                ["Mat"] = typeof(Mat), //TODO
                ["Tex"] = typeof(Tex), //TODO
                #endregion

                #region Texture
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

                    texture2D.name = assetPath;
                    CustomAssetManager.Alternations.Add(assetPath, texture2D);
                    return new Tex { path = assetPath };
                }, new[] { "path", "replace" }),
                #endregion

                #region Texture_Material
                ["add_texture_material"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    Tex texture = GetArgument<Tex>("texture")
                                     .When<string>(pathTex => new Tex { path = pathTex })
                                     .ElseRequiredThrow();

                    Texture2D texture2D;
                    if (CustomAssetManager.Alternations.TryGetValue(texture.path, out UnityEngine.Object data))
                    {
                        if (data is Texture2D t2d)
                            texture2D = t2d;
                        else
                            throw new ArgumentException($"Given prefab is not a Texture: {texture.path}");
                    }
                    else
                    {
                        texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                        string basePath = typeof(RecipeRegistrator).Assembly.Location;
                        string assetPath = texture.path;
                        byte[] image = File.ReadAllBytes(Path.Combine(basePath, "..", assetPath));

                        if (!texture2D.LoadImage(image))
                            throw new ArgumentException($"Could not load an image: {assetPath}");

                        CustomAssetManager.Alternations.Add(assetPath, texture2D);
                        texture2D.name = texture.path;
                    }

                    string path = GetArgument<string>("path").ElseRequiredThrow();
                    Material material = new Material(Shader.Find("Standard"));
                    material.CopyPropertiesFromMaterial(new AssetsDb().DefaultMaterial);
                    material.mainTexture = texture2D;
                    material.SetTexture(Shader.PropertyToID("_AlbedoTex"), texture2D);
                    material.color = Color.white;
                    CustomAssetManager.Alternations.Add(path, material);
                    return new Mat { path = path };
                }, new[] { "path", "texture" }),
                #endregion

                #region Model
                ["add_prefab_box"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    Texture2D texture = GetArgument<Texture2D>("texture")
                                           .When<string>(pathTex =>
                                           {
                                               if (CustomAssetManager.Alternations.TryGetValue(pathTex, out UnityEngine.Object data))
                                                   return data is Texture2D t2d ? t2d : throw new ArgumentException($"Given prefab is not a Texture: {pathTex}");

                                               Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                                               string basePath = typeof(RecipeRegistrator).Assembly.Location;
                                               string assetPath = pathTex;
                                               byte[] image = File.ReadAllBytes(Path.Combine(basePath, "..", assetPath));
                                               if (!texture2D.LoadImage(image))
                                               {
                                                   throw new ArgumentException($"Could not load an image: {assetPath}");
                                               }
                                               CustomAssetManager.Alternations.Add(assetPath, texture2D);
                                               return texture2D;
                                           })
                                           .ElseRequiredThrow();

                    string path = GetArgument<string>("path").ElseRequiredThrow();
                    GameObject prefab = new GameObject();
                    prefab.SetActive(true);
                    prefab.name = path;

                    MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
                    Mesh mesh = new Mesh();
                    float x = GetArgument<float>("width").ElseDefault(0.5f);
                    float y = GetArgument<float>("height").ElseDefault(0.2f);
                    float z = GetArgument<float>("depth").ElseDefault(0.5f);
                    mesh.vertices = new[]
                    {
                        new Vector3(-x, 0, -z),
                        new Vector3(x, 0, z),
                        new Vector3(x, 0, -z),
                        new Vector3(-x, 0, z),
                        new Vector3(-x, y, -z),
                        new Vector3(x, y, z),
                        new Vector3(x, y, -z),
                        new Vector3(-x, y, z),
                    };
                    mesh.triangles = new[]
                    {
                        // Front face
                        0, 2, 1,
                        0, 3, 2,
                        // Back face
                        4, 5, 6,
                        4, 6, 7,
                        // Left face
                        0, 7, 3,
                        0, 4, 7,
                        // Right face
                        1, 2, 6,
                        1, 6, 5,
                        // Top face
                        3, 7, 6,
                        3, 6, 2,
                        // Bottom face
                        0, 1, 5,
                        0, 5, 4
                    };
                    mesh.uv = new[]
                    {
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
                    };
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    meshFilter.mesh = mesh;

                    MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
                    meshRenderer.material = new Material(Shader.Find("Standard"));
                    meshRenderer.material.mainTexture = texture;

                    CustomAssetManager.Alternations.Add(path, prefab);
                    CustomAssetManager.Alternations.Add(path + "__mesh", meshFilter.mesh);
                    CustomAssetManager.Alternations.Add(path + "__material", meshRenderer.material);
                    return new Prefab() { path = path };
                }, new[] { "path", "texture" }),
                #endregion

                #region Build research
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
                                    .ElseRequiredThrow(),
                        costMonths: GetArgument<int>("costs").ElseDefault(1));

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

                    if (GetArgument<Tex>("icon")
                        .When<string>(s => new Tex { path = s })
                        .ElseNotExists(out Tex path))
                    {
                        builder.AddIcon(Option.None, path.path);
                    }

                    return builder.BuildAndAdd();
                }, new[] { "researchId", "name", "description", "difficulty", "position", "parents" }),
                #endregion

                #region Define product reference for recipe creation
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

                    builder.SetDuration(GetArgument<Duration>("duration").When<int>(Duration.FromSec).ElseDefault(Duration.FromSec(60)));

                    RecipeProto recipe = builder.BuildAndAdd();
                    if (GetArgument<ResearchNodeProto>("research")
                        .When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
                        .When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
                        .ElseNotExists(out ResearchNodeProto research))
                    {
                        typeof(ResearchNodeProto).GetField("Units", BindingFlags.Public | BindingFlags.Instance)
                                                 .SetValue(research, research.Units
                                                                             .AsEnumerable()
                                                                             .Concat(new IUnlockNodeUnit[] { new RecipeUnlock(recipe, machine, false),
                                                                                                             new ProtoWithIconUnlock(machine, false) })
                                                                             .Distinct(i =>
                                                                             {
                                                                                 if (i is ProtoWithIconUnlock protoUnlock)
                                                                                     return protoUnlock.Proto.Id.Value;
                                                                                 else
                                                                                     return DateTime.Now.Ticks.ToString();
                                                                             })
                                                                             .ToImmutableArray());

                        typeof(ResearchNodeProto.Gfx).GetField("<Icons>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                                                     .SetValue(research.Graphics, research.Graphics.Icons
                                                                                                   .AsEnumerable()
                                                                                                   .Concat(new KeyValuePair<Option<Proto>, string>[] { new KeyValuePair<Option<Proto>, string>(machine, machine.IconPath) })
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
                
                #region Edit recipe
                ["edit_recipe"] = new Constructor((args) =>
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

                    if (GetArgument<List<object>>("ingredients").ElseNotExists(out var ingredientsList))
                    {
                        ingredientsList.Select(e => (Product)e)
                                       .Call(e =>
                                       {
                                           RecipeInput recipeInput = recipe.AllInputs
                                               .AsEnumerable()
                                               .Where(a => a.Product.Id == e.product.Id)
                                               .FirstOrDefault();

                                           if (recipeInput == null)
                                               throw new ArgumentException("Recipe has no ingredient with id: " + e.product.Id.Value);

                                           typeof(RecipeInput)
                                               .GetField("Quantity", BindingFlags.Public | BindingFlags.Instance)
                                               .SetValue(recipeInput, e.quantity);
                                       })
                                       .ToArray(); // invoke ling actions
                    }

                    if (GetArgument<List<object>>("products").ElseNotExists(out var productsList))
                    {
                        productsList.Select(e => (Product)e)
                                    .Call(e =>
                                    {
                                        RecipeOutput recipeOutput = recipe.AllOutputs
                                            .AsEnumerable()
                                            .Where(a => a.Product.Id == e.product.Id)
                                            .FirstOrDefault();

                                        if (recipeOutput == null)
                                            throw new ArgumentException("Recipe has no produc with id: " + e.product.Id.Value);

                                        typeof(RecipeOutput)
                                            .GetField("Quantity", BindingFlags.Public | BindingFlags.Instance)
                                            .SetValue(recipeOutput, e.quantity);
                                    })
                                    .ToArray(); // invoke ling actions
                    }

                    if (GetArgument<Duration>("duration")
                        .When<int>(i => Duration.FromSec(i))
                        .ElseNotExists(out Duration duration))
                    {
                        typeof(RecipeProto)
                            .GetField("<Duration>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                            .SetValue(recipe, duration);
                    }

                    if (GetArgument<ResearchNodeProto>("research")
                        .When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
                        .When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
                        .ElseNotExists(out ResearchNodeProto research))
                    {
                        MachineProto machine = GetArgument<MachineProto>("machine")
                                                   .When<MachineProto.ID>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(id))
                                                   .When<string>(id => registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(id)))
                                                   .ElseRequiredThrow();

                        if (!machine.Recipes.Any(r => r.Id == recipe.Id))
                            machine.AddRecipe(recipe);

                        typeof(ResearchNodeProto).GetField("Units", BindingFlags.Public | BindingFlags.Instance)
                                                 .SetValue(research, research.Units
                                                                             .AsEnumerable()
                                                                             .Concat(new IUnlockNodeUnit[] { new RecipeUnlock(recipe, machine, false),
                                                                                                             new ProtoWithIconUnlock(machine, false) })
                                                                             .Distinct(i =>
                                                                             {
                                                                                 if (i is ProtoWithIconUnlock protoUnlock)
                                                                                     return protoUnlock.Proto.Id.Value;
                                                                                 else
                                                                                     return DateTime.Now.Ticks.ToString();
                                                                             })
                                                                             .ToImmutableArray());

                        typeof(ResearchNodeProto.Gfx).GetField("<Icons>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                                                     .SetValue(research.Graphics, research.Graphics.Icons
                                                                                                   .AsEnumerable()
                                                                                                   .Concat(new KeyValuePair<Option<Proto>, string>[] { new KeyValuePair<Option<Proto>, string>(machine, machine.IconPath) })
                                                                                                   .ToImmutableArray());
                    }

                    return recipe;
                }, new string[]
                {
                    "recipe",
                    "machine",
                    "research",
                    "duration",
                    "ingredients",
                    "products"
                }),
                #endregion

                #region Build edict
                ["build_edict"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }


                    Proto.ID edictId = GetArgument<EdictProto.ID>("edictId")
                                    .When<string>(v => new EdictProto.ID(v))
                                    .ElseRequiredThrow();
                    return registrator.PrototypesDb.Add(new EdictProto(
                        id: edictId,
                        strings: Proto.CreateStr(
                            edictId,
                            name: GetArgument<string>("name").ElseRequiredThrow(),
                            descShort: GetArgument<string>("description").ElseDefault("")
                        ),
                        category: GetArgument<EdictCategoryProto>("category")
                            .When<EdictCategoryProto.ID>(id => registrator.PrototypesDb.GetOrThrow<EdictCategoryProto>(id))
                            .When<string>(id => registrator.PrototypesDb.GetOrThrow<EdictCategoryProto>(new EdictCategoryProto.ID(id)))
                            .ElseRequiredThrow(),
                        monthlyUpointsCost: GetArgument<Upoints>("cost")
                            .When<int>(i => i.Upoints())
                            .ElseDefault(0.3.Upoints()),
                        edictImplementation: GetArgument<Type>("implementation").ElseRequiredThrow(),
                        graphics: new EdictProto.Gfx(GetArgument<Tex>("icon")
                                                         .When<string>(s => new Tex() { path = s })
                                                         .ElseRequiredThrow().path),
                        isGeneratingUnity: GetArgument<bool?>("isGeneratingUnity").ElseDefault(null),
                        previousTier: GetArgument<Option<EdictProto>>("previousTier")
                            .When<EdictProto>(Option.Some)
                            .When<EdictProto.ID>(id => registrator.PrototypesDb.GetOrThrow<EdictProto>(id))
                            .When<string>(id => registrator.PrototypesDb.GetOrThrow<EdictProto>(new EdictProto.ID(id)))
                            .ElseDefault(Option.None)
                    ));
                }, new string[]
                {
                    "edictId",
                    "name",
                    "description",
                    "category",
                    "icon",
                    "implementation",
                    "cost",
                    "isGeneratingUnity",
                    "previousTier",
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
                                                                         .Concat(new IUnlockNodeUnit[] { new RecipeUnlock(recipe, machine, false),
                                                                                                         new ProtoWithIconUnlock(machine, false) })
                                                                         .Distinct(i =>
                                                                         {
                                                                             if (i is ProtoWithIconUnlock protoUnlock)
                                                                                 return protoUnlock.Proto.Id.Value;
                                                                             else
                                                                                 return DateTime.Now.Ticks.ToString();
                                                                         })
                                                                         .ToImmutableArray());

                    typeof(ResearchNodeProto.Gfx).GetField("<Icons>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                                                 .SetValue(research.Graphics, research.Graphics.Icons
                                                                                               .AsEnumerable()
                                                                                               .Concat(new KeyValuePair<Option<Proto>, string>[] { new KeyValuePair<Option<Proto>, string>(machine, machine.IconPath) })
                                                                                               .ToImmutableArray());
                    return null;
                }, new[] { "research", "machine", "recipe" }),

                ["add_unlock_product"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    ProductProto product = GetArgument<ProductProto>("product")
                                              .When<ProductProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ProductProto>(id))
                                              .When<string>(id => registrator.PrototypesDb.GetOrThrow<ProductProto>(new ProductProto.ID(id)))
                                              .ElseRequiredThrow();
                    ResearchNodeProto research = GetArgument<ResearchNodeProto>("research")
                                                    .When<ResearchNodeProto.ID>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(id))
                                                    .When<string>(id => registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(new ResearchNodeProto.ID(id)))
                                                    .ElseRequiredThrow();

                    typeof(ResearchNodeProto).GetField("Units", BindingFlags.Public | BindingFlags.Instance)
                                             .SetValue(research, research.Units
                                                                         .AsEnumerable()
                                                                         .Concat(new IUnlockNodeUnit[] { new ProductUnlock(product, false) })
                                                                         .ToImmutableArray());
                    return null;
                }, new[] { "research", "product" }),

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
                                                                         .Distinct(i =>
                                                                         {
                                                                             if (i is ProtoWithIconUnlock protoUnlock)
                                                                                 return protoUnlock.Proto.Id.Value;
                                                                             else
                                                                                 return DateTime.Now.Ticks.ToString();
                                                                         })
                                                                         .ToImmutableArray());

                    typeof(ResearchNodeProto.Gfx).GetField("<Icons>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                                                 .SetValue(research.Graphics, research.Graphics.Icons
                                                                                               .AsEnumerable()
                                                                                               .Concat(new KeyValuePair<Option<Proto>, string>[] { new KeyValuePair<Option<Proto>, string>(machine, machine.IconPath) })
                                                                                               .ToImmutableArray());
                    return null;
                }, new[] { "research", "machine" }),
                #endregion

                #region Build product
                ["build_product_loose"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    var id = GetArgument<ProductProto.ID>("productId")
                           .When<string>(ids => new ProductProto.ID(ids))
                           .ElseRequiredThrow();
                    var name = GetArgument<string>("name").ElseRequiredThrow();
                    var icon = GetArgument<Tex>("icon")
                        .When<string>(s => new Tex { path = s })
                        .ElseRequiredThrow()
                        .path;
                    var material = GetArgument<Mat>("material").When<string>(s => new Mat { path = s }).ElseRequiredThrow();
                    var desc = GetArgument<string>("description").ElseDefault("");
                    var isDumped = GetArgument<bool>("isDumped").ElseDefault(false);
                    var isStorable = GetArgument<bool>("isStorable").ElseDefault(false);
                    var isRecyclable = GetArgument<bool>("isRecyclable").ElseDefault(false);
                    var isWaste = GetArgument<bool>("isWaste").ElseDefault(false);
                    var isRough = GetArgument<bool>("isRough").ElseDefault(false);
                    var color = GetArgument<ColorRgba>("color")
                            .When<(int r, int g, int b)>(t => new ColorRgba((byte)t.r, (byte)t.g, (byte)t.b))
                            .ElseRequiredThrow();

                    var product = new LooseProductProto(
                        id: id,
                        strings: Proto.CreateStr(id, name, desc),
                        graphics: new LooseProductProto.Gfx(
                            prefabPath: isRough ? "Assets/Base/Transports/ConveyorLoose/PileRough.prefab" : "Assets/Base/Transports/ConveyorLoose/PileSmooth.prefab",
                            pileMaterialAssetPath: /*Mafi.Base.Assets.Base.Products.Loose.Coal_mat*/ material.path,
                            useRoughPileMeshes: isRough,
                            resourcesVizColor: color,
                            customIconPath: icon
                        ),
                        isDumpedOnTerrainByDefault: isDumped,
                        isStorable: isStorable,
                        isRecyclable: isRecyclable,
                        isWaste: isWaste
                    );
                    registrator.PrototypesDb.Add(product, GetArgument<bool>("isLocked").ElseDefault(false));
                    return product;
                }, new[] { "productId", "name", "description", "icon", "color", "isDumped", "isStorable", "isRecyclable", "isWaste", "isRough", "isLocked" }),

                ["build_product_unit"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    var id = GetArgument<ProductProto.ID>("productId")
                           .When<string>(ids => new ProductProto.ID(ids))
                           .ElseRequiredThrow();
                    var name = GetArgument<string>("name").ElseRequiredThrow();
                    var icon = GetArgument<Tex>("icon")
                        .When<string>(s => new Tex { path = s })
                        .ElseRequiredThrow()
                        .path;
                    var prefab = GetArgument<string>("prefab")
                                    .When<Prefab>(p => p.path)
                                    .ElseRequiredThrow();
                    var desc = GetArgument<string>("description").ElseDefault("");
                    var maxTransport = GetArgument<Quantity>("maxTransport")
                                          .When<int>(i => new Quantity(i))
                                          .ElseDefault(new Quantity(3));
                    var packing = GetArgument<CountableProductStackingMode>("packingMode").ElseDefault(CountableProductStackingMode.Auto);
                    var allowPackingNoise = GetArgument<bool>("allowPackingNoise").ElseDefault(false);
                    var isStorable = GetArgument<bool>("isStorable").ElseDefault(false);
                    var isWaste = GetArgument<bool>("isWaste").ElseDefault(false);

                    var product = new CountableProductProto(
                        id: id,
                        strings: Proto.CreateStr(id, name, desc),
                        maxQuantityPerTransportedProduct: maxTransport,
                        isStorable: isStorable,
                        graphics: new CountableProductProto.Gfx(
                            prefabPath: prefab,
                            customIconPath: icon,
                            packingMode: packing,
                            allowPackingNoise: allowPackingNoise
                        ),
                        isWaste: isWaste
                    );
                    registrator.PrototypesDb.Add(product, GetArgument<bool>("isLocked").ElseDefault(false));
                    return product;
                }, new[] { "productId", "name", "icon", "prefab", "maxTransport", "packingMode", "allowPackingNoise", "description", "isStorable", "isWaste", "isLocked" }),

                ["build_product_fluid"] = new Constructor((args) =>
                {
                    AnyArgument<T> GetArgument<T>(string argumentName)
                    {
                        IArgumentValue arg = args.Where(a => a.Name == argumentName).FirstOrDefault();
                        if (arg is null)
                            return new AnyArgument<T>(argumentName, default, empty: true);
                        else
                            return new AnyArgument<T>(argumentName, arg.Value);
                    }

                    var id = GetArgument<ProductProto.ID>("productId")
                           .When<string>(ids => new ProductProto.ID(ids))
                           .ElseRequiredThrow();
                    var name = GetArgument<string>("name").ElseRequiredThrow();
                    var icon = GetArgument<Tex>("icon")
                        .When<string>(s => new Tex { path = s })
                        .ElseRequiredThrow()
                        .path;
                    var desc = GetArgument<string>("description").ElseDefault("");
                    var maxTransport = GetArgument<Quantity>("maxTransport")
                                          .When<int>(i => new Quantity(i))
                                          .ElseDefault(new Quantity(3));
                    var packing = GetArgument<CountableProductStackingMode>("packingMode").ElseDefault(CountableProductStackingMode.Auto);
                    var allowPackingNoise = GetArgument<bool>("allowPackingNoise").ElseDefault(false);
                    var isStorable = GetArgument<bool>("isStorable").ElseDefault(false);
                    var isWaste = GetArgument<bool>("isWaste").ElseDefault(false);
                    var canBeDiscarded = GetArgument<bool>("canBeDiscarded").ElseDefault(true);

                    var color = GetArgument<ColorRgba>("color").ElseDefault(default);
                    var transportColor = GetArgument<ColorRgba>("transportColor").ElseDefault(default);
                    var transportAccentColor = GetArgument<ColorRgba>("transportAccentColor").ElseDefault(default);

                    var product = new FluidProductProto(
                        id: id,
                        strings: Proto.CreateStr(id, name, desc),
                        isStorable: isStorable,
                        canBeDiscarded: canBeDiscarded,
                        graphics: new FluidProductProto.Gfx(
                            prefabPath: Option.None,
                            customIconPath: icon,
                            color: color,
                            transportColor: transportColor,
                            transportAccentColor: transportAccentColor
                        ),
                        isWaste: isWaste
                    );
                    registrator.PrototypesDb.Add(product, GetArgument<bool>("isLocked").ElseDefault(false));
                    return product;
                }, new[] { "productId", "name", "icon", "canBeDiscarded", "packingMode", "allowPackingNoise", "description", "isStorable", "isWaste", "isLocked", "color", "transportColor", "transportAccentColor" })
                #endregion
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
