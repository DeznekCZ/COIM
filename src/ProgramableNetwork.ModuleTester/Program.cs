using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using ProgramableNetwork.Python;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProgramableNetwork.ModuleTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ExportBinaries();
            RunTest();
        }

        private static void RunTest()
        {
            ProtosDb protosDb = new ProtosDb();

            ConstructorInfo constructor = typeof(ProtoRegistrator).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            ProtoRegistrator registrator = (ProtoRegistrator)constructor.Invoke(new object[] { protosDb, new IConfig[0].ToImmutableArray() });
            InitBaseGame(registrator);

            Console.WriteLine(new DirectoryInfo(".").FullName);

            string[] files = new string[]
            {
                @"..\..\..\ProgramableNetwork.Modules\Custom\delay.py",
                @"..\..\..\ProgramableNetwork.Modules\Custom\connection_isactive.py",
                @"..\..\..\ProgramableNetwork.Modules\Custom\memory_selector.py",
                @"..\..\..\ProgramableNetwork.Modules\Custom\randomizer.py",
            };

            foreach (var file in files)
            {
                ModuleRegistrator.Register(registrator, file, out _);
            }

            protosDb.TryFindProtoIgnoreCase("ProgramableNetwork_Module_Connection_IsActive", out ModuleProto proto);

            EntityManager manager = new EntityManager();
            EntityContext entityContext = new EntityContext(null, manager, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            FakeProto fakeProto = new FakeProto(new FakeProto.ID("fake"));
            manager.AddEntity(1, new FakeEntity(new EntityId(1), fakeProto, entityContext));
            Module m = new Module(proto, entityContext, null);
            m.Controller = new Controller(
                new EntityId(2),
                new ControllerProto(
                    new Mafi.Core.Entities.Static.StaticEntityProto.ID("asd"),
                    Proto.Str.Empty,
                    new EntityLayoutParser(protosDb).ParseLayoutOrThrow("[1]"),
                    new EntityCosts(),
                    new LayoutEntityProto.Gfx("path")
                 ),
                new TileTransform(),
                entityContext,
                null
            );

            Fix32 i = new Fix32();
            int[] outputs = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            while (true)
            {
                i += 1;
                m.Input["0"] = i;
                m.Field["entity"] = Fix32.FromRaw(1);
                m.Execute();

                IEnumerable<string> converted = outputs.Select(o => m.Output[o.ToString(), Fix32.Zero].ToString());
                Console.WriteLine($"{i}, {string.Join(", ", converted)} [{m.Status}]");

                Console.WriteLine("lala");
            }
        }

        private static void ExportBinaries()
        {
            var mafiTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.dll").GetExportedTypes();
            //var mafiUnityTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.Unity.dll").GetExportedTypes();
            var mafiCoreTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.Core.dll").GetExportedTypes();
            var mafiBaseTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.Base.dll").GetExportedTypes();

            IEnumerable<Type> allTypes = mafiTypes.Concat(mafiCoreTypes).Concat(mafiBaseTypes);
            ILookup<string, Type> lookup = allTypes.ToLookup(t => t.Namespace);

            DirectoryInfo targetDirectory = new DirectoryInfo(@"..\..\..\ProgramableNetwork.Modules");

            foreach (var item in lookup)
            {
                string directoryname = targetDirectory.FullName + @"\" + item.Key.Replace(".", @"\");
                string filename = targetDirectory.FullName + @"\" + item.Key.Replace(".", @"\") + @"\__init__.py";

                StringBuilder stringBuilder = new StringBuilder();

                foreach (var type in item)
                {
                    if (type.IsArray) continue;
                    if (type.IsGenericType) continue;

                    stringBuilder.Append("class ");
                    stringBuilder.Append(type.Name);
                    stringBuilder.Append(":\n");

                    foreach (var property in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
                    {
                        stringBuilder.Append("    ");
                        stringBuilder.Append(property.Name);
                        stringBuilder.Append(" = None\n");
                    }

                    stringBuilder.Append("\n    def __init__(self):\n");

                    HashSet<string> names = new HashSet<string>();
                    foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        AppendInstanceProperty(property, stringBuilder, names);
                    }
                    foreach (var property in type.GetInterfaces()
                        .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public)))
                    {
                        AppendInstanceProperty(property, stringBuilder, names);
                    }

                    if (names.Count == 0)
                        stringBuilder.Append("        pass\n\n");
                }

                Directory.CreateDirectory(directoryname);
                File.WriteAllText(filename, stringBuilder.ToString());
                Console.WriteLine();
            }
            Console.WriteLine();

            void AppendInstanceProperty(PropertyInfo property, StringBuilder stringBuilder, HashSet<string> names)
            {
                if (!names.Add(property.Name))
                    return;

                if (property.PropertyType == typeof(bool))
                    stringBuilder.Append($"        self.{property.Name} = False\n");
                else if (property.PropertyType == typeof(string))
                    stringBuilder.Append($"        self.{property.Name} = str(0)\n");
                else if (property.PropertyType == typeof(int))
                    stringBuilder.Append($"        self.{property.Name} = int(0)\n");
                else if (property.PropertyType == typeof(float))
                    stringBuilder.Append($"        self.{property.Name} = float(0)\n");
                else if (property.PropertyType == typeof(Fix32))
                    stringBuilder.Append($"        from Mafi import Fix32\n        self.{property.Name} = Fix32()\n");
                else if (property.PropertyType == typeof(Fix64))
                    stringBuilder.Append($"        from Mafi import Fix64\n        self.{property.Name} = Fix64()\n");
                else if (property.PropertyType.Name.StartsWith("Option"))
                    stringBuilder.Append($"        from Mafi import Option\n        self.{property.Name} = Option()\n");
                else
                    stringBuilder.Append($"        self.{property.Name} = None\n");
            }
        }

        private static void InitBaseGame(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(new ProductProto(new ProductProto.ID("Product_Electronics"), new Proto.Str(), 1.Quantity(), true, true, false, new ProductProto.Gfx(Option.None, Option.Create("text"))));
            registrator.PrototypesDb.Add(new VirtualProductProto(new ProductProto.ID("Product_Virtual_MaintenanceT1"), new Proto.Str(), new ProductProto.Gfx(Option.None, Option.Create("text"))));
        }
    }
}
