using Mafi.Core.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using Mafi.Core.Prototypes;
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
			long newId = controller.Resolver.Resolve<ModuleIdManager>().Allocate();
            Module module = new Module(proto, controller.Context, controller, newId);
            // Position is owned by the module itself since Controller.MODULE_LAYOUT_INFO.
            module.Row = row;
            module.Column = column;
            controller.Modules.Add(module);

            int width = module.Layout.GetWidth(module);
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