using Mafi.Core.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using Mafi.Core.Prototypes;
using System.Threading;
using Mafi;

namespace ProgramableNetwork.Python
{
    internal class ControllerWrapper
    {
        private ProtoRegistrator registrator;
        private Class definition;
        private Controller controller;
        public int next_row;
        public int next_column;

        public ControllerWrapper(ProtoRegistrator registrator, Class item)
        {
            this.registrator = registrator;
            this.definition = item;
        }

        public ControllerTemplate.Settings Generate(Controller controller)
        {
            this.controller = controller;
            if (definition.classContext["modules"] is Method moduleMethod)
            {
                try
                {
                    List<object> modules = moduleMethod.Invoke([
                            new NamedValue("self", this)
                        ]) as List<object>;


                    if (definition.classContext["settings"] is Method settingsMethod)
                    {
                        return () =>
                        {
                            try
                            {
                                settingsMethod.Invoke([
                                        new NamedValue("self", this),
                                    new NamedValue("modules", modules)
                                    ]);
                            }
                            catch (Exception e)
                            {
                                Log.Exception(e, $"Failed to apply settings to controller from template: {definition.name}");
                            }
                        };
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e, $"Failed to apply settings to controller from template: {definition.name}");
                }
                return () => { };
            }
            throw new NotImplementedException("Missing 'modules' method");
        }

        public ModuleWrapper add_module(Type moduleProto, int row, int column)
        {

            string name = moduleProto.Name.ModuleId();
            ModuleProto proto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID(name))
                .ValueOrThrow("Missing module");
            Module module = new Module(proto, controller.Context, controller);
            controller.Modules.Add(module);
            Thread.Sleep(1);

            int width = module.Layout.GetWidth(module);
            controller.Rows[row][column] = ModulePlacement.Origin(module.Id);
            for (int i = 1; i < width; i++) {
				controller.Rows[row][column + i] = ModulePlacement.Rest(module.Id);
			}

			column += width;
            if (column == controller.Prototype.Columns)
            {
                column = 0;
                row++;
            }

            this.next_row = row;
            this.next_column = column;
            return new ModuleWrapper(module);
        }
    }
}