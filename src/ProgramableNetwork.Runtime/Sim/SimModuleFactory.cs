using Mafi;

namespace ProgramableNetwork.Runtime.Sim
{
    /// <summary>Creates a <see cref="SimModule"/> from a definition, seeding field values with their defaults.</summary>
    public static class SimModuleFactory
    {
        public static SimModule Create(ModuleDefinition def, long id)
        {
            SimModule m = new SimModule(def) { Id = id };
            foreach (FieldDef f in def.Fields)
            {
                m.FieldUseField[f.Id] = true;
                switch (f.Kind)
                {
                    case FieldKind.Int32:
                        m.FieldNumbers[f.Id] = Fix32.FromInt(f.Default is int i ? i : 0);
                        break;
                    case FieldKind.Int64:
                        m.FieldNumbers[f.Id] = Fix32.FromInt(f.Default is long l ? (int)l : 0);
                        break;
                    case FieldKind.Fix32:
                        m.FieldNumbers[f.Id] = f.Default is Fix32 fx ? fx : Fix32.Zero;
                        break;
                    case FieldKind.Boolean:
                        m.FieldNumbers[f.Id] = (f.Default is bool b && b) ? Fix32.One : Fix32.Zero;
                        break;
                    case FieldKind.String:
                        m.FieldStrings[f.Id] = f.Default as string ?? "";
                        break;
                    case FieldKind.Entity:
                        m.FieldEntities[f.Id] = MockEntity.Null;
                        break;
                }
            }
            return m;
        }
    }
}
