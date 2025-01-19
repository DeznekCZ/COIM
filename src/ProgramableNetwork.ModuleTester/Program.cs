using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using ProgramableNetwork.Python;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.ModuleTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ProtosDb protosDb = new ProtosDb();

            ConstructorInfo constructor = typeof(ProtoRegistrator).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            ProtoRegistrator registrator = (ProtoRegistrator)constructor.Invoke(new object[] { protosDb, new IConfig[0].ToImmutableArray() });
            InitBaseGame(registrator);

            Console.WriteLine(new DirectoryInfo(".").FullName);

            string file = @"..\..\..\ProgramableNetwork.Modules\Custom\connection_isactive.py";

            ModuleRegistrator.Register(registrator, file);

            protosDb.TryFindProtoIgnoreCase("ProgramableNetwork_Module_Connection_IsActive", out ModuleProto proto);

            EntityManager manager = new EntityManager();
            EntityContext entityContext = new EntityContext(null, manager, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            FakeProto fakeProto = new FakeProto(new FakeProto.ID("fake"));
            manager.AddEntity(0, new FakeEntity(new EntityId(0), fakeProto, entityContext));
            Module m = new Module(proto, entityContext, null);
            m.NumberData["field__entity"] = 0;
            proto.Action(m);

            Console.WriteLine("lala");
        }

        private static void InitBaseGame(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(new ProductProto(new ProductProto.ID("Product_Electronics"), new Proto.Str(), 1.Quantity(), true, true, false, new ProductProto.Gfx(Option.None, Option.Create("text"))));
            registrator.PrototypesDb.Add(new VirtualProductProto(new ProductProto.ID("Product_Virtual_MaintenanceT1"), new Proto.Str(), new ProductProto.Gfx(Option.None, Option.Create("text"))));
        }
    }
}
