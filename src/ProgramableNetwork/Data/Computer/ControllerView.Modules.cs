using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity;
using System.Collections.Generic;
using System.Linq;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi;
using Mafi.Core.Entities.Dynamic;
using Mafi.Localization;
using System;
using ProgramableNetwork.Python;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.Ui.Library;
using Mafi.Core.Entities.Static;

namespace ProgramableNetwork
{
    public class Sizes
    {
        public static readonly Px BLOCK_SIZE = 32.px();
        public static readonly Px IMAGE_SIZE = 28.px();
        public static readonly Px IMAGE_PADDING = 2.px();
    }

    public partial class ControllerView : Column
    {
        public static Module m_lastCreated;
        private LocStr? m_decription;
        private Category m_category;
        private readonly ControllerInspector m_controller;
        private List<IDataUpdater> m_updaters;
        private readonly Action m_refresh;

        public ControllerView(ControllerInspector controller, Action refresh)
            : base(gap: 5)
        {
            m_controller = controller;
            m_updaters = new List<IDataUpdater>();
            m_refresh = refresh;
            AddModuleImplementation(refresh);
        }

        public Controller Entity => m_controller.Entity;

        public Dictionary<long, (int x, int y)> ModulePlacementCache { get; } = new Dictionary<long, (int x, int y)>();
        public ControllerInspector Inspector => m_controller;

        private void AddModuleImplementation(Action refresh)
        {
            this.Observe(() => Entity).Do((entity) => {
                m_decription = null;
                m_controller.OutputConnection = null;
                m_controller.EntityHighlighterSelectable.ClearAllHighlights();
                m_updaters.Clear();

                if (entity != null)
                {
                    RedrawComponents(Entity.Modules.Select(m => m.Id).ToLyst());
                }
            });
            this.Observe(WasOrderChanged, new ModuleIdComparator()).Do(RedrawComponents);
        }

        private IEnumerable<long> WasOrderChanged()
        {
            return Entity?.Rows?.SelectMany(item => item)?.Select(item => item.ModuleId);
        }

        public void RedrawComponents()
        {
            if (Entity != null)
            {
                RedrawComponents(new Lyst<long>());
            }
        }

        private void RedrawComponents(Lyst<long> modules)
        {
            Clear();
            ModulePlacementCache.Clear();

            for (int i = 0; i < Entity.Rows.Count; i++)
            {
                var rowElement = new Row();
                rowElement.Height(Sizes.BLOCK_SIZE * 4);
                rowElement.Width(Entity.Prototype.Columns * Sizes.BLOCK_SIZE);

                var row = Entity.Rows[i];
                for (int j = 0; j < row.Count; j++)
                {
                    if (!row[j].Placement) continue;

                    var module = (Entity.Modules ?? new Lyst<Module>())
                        .AsEnumerable()
                        .FirstOrDefault(m => m.Id == row[j].ModuleId);

                    if (module == null)
                    {
                        AddFreeSlot(rowElement, i, j);
                        continue;
                    }
                    rowElement.Add(new ModuleView(module, this, m_controller.Context, false, () => RedrawComponents(modules)));
                    ModulePlacementCache[module.Id] = (i, j);
                }

                Add(rowElement);
            }
        }

        private void AddFreeSlot(Row rowElement, int targetRow, int targetColumn)
        {
            Column column = rowElement.AddAndReturn(new Column())
                .Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 4);

            // add filler
            column.AddAndReturn(new UiComponent())
                  .Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);

            ButtonText button = column.AddAndReturn(new ButtonText(new LocStrFormatted("+")));
            button.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 2);

            ProtoPickerPopup<AModuleProtoSelector> protoPicker = new ProtoPickerPopup<AModuleProtoSelector>(
                optionsProvider: () => NewModulePicker(targetRow, targetColumn),
                optionViewFactory: (s) => s.CreateUi(),
                onOptionSelected: (s) => s.Selected(),
                button: button,
                title: new LocStrFormatted("Add module"),
                config: new ProtoPickerConfig { ItemsPerRow = 1, ItemSize = new UnityEngine.Vector2(600, Px.Auto) },
                orderAlphabetically: false,
                searchable: true
            );

            // add filler
            column.AddAndReturn(new UiComponent())
                  .Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
        }

        private IEnumerable<AModuleProtoSelector> NewModulePicker(int targetRow, int targetColumn)
        {
            Controller controller = m_controller.Entity;
            StaticEntityProto.ID id = controller.Prototype.Id;

            if (m_lastCreated != null && m_lastCreated.Prototype.AllowedDevices.Contains(id))
                yield return new LastCreatedModule(m_controller.Entity, this, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
                {
                    if (TryPlaceAt(moduleProto, targetRow, targetColumn))
                    {
                        return (true, m_lastCreated);
                    }
                    else
                    {
                        return (false, null);
                    }
                }, m_lastCreated);

            if (TemplateRegistrator.GetTemplates().Count > 0)
                foreach (KeyValuePair<string, Template> item in TemplateRegistrator.GetTemplates()
                                                                    .Where(p => p.Value.ModuleProto.AllowedDevices.Any(e => e.Equals(id))))
                    yield return new TemplateModule(m_controller.Entity, this, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
                    {
                        if (TryPlaceAt(moduleProto, targetRow, targetColumn))
                        {
                            return (true, m_lastCreated);
                        }
                        else
                        {
                            return (false, null);
                        }
                    }, item);

            foreach (ModuleProto item in m_controller.Entity.Context.ProtosDb
                                            .All<ModuleProto>()
                                            //.Where(p => p.IsAvailable)
                                            .Where(p => p.AllowedDevices.Any(e => e.Equals(id))))
                yield return new NewModule(m_controller.Entity, this, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
                {
                    if (TryPlaceAt(moduleProto, targetRow, targetColumn))
                    {
                        return (true, m_lastCreated);
                    }
                    else
                    {
                        return (false, null);
                    }
                }, item);
        }

        public bool TryPlaceAt(ModuleProto moduleProto, int targetRow, int targetColumn)
        {
            var module = new Module(moduleProto, Entity.Context, Entity);
            var width = module.Layout.GetWidth(module);
            var placeFound = true;
            var row = Entity.Rows[targetRow];

            var end = targetColumn + width;
            for (int columnEnd = targetColumn; columnEnd < end; columnEnd++)
            {
                if (row[columnEnd].ModuleId != 0)
                {
                    placeFound = false;
                    break;
                }
            }

            if (placeFound)
            {
                for (int i = targetColumn; i < end; i++)
                {
                    row[i] = (module.Id, false);
                }
                row[targetColumn] = (module.Id, true);
                Entity.Modules.Add(module);
                m_lastCreated = module;
                return true;
            }
            else
            {
                m_controller.Context.AudioDb.InvalidOp(true).Play();
                return false;
            }
        }

        private class ModuleIdComparator : ICollectionComparator<long, IEnumerable<long>>
        {
            public bool AreSame(IEnumerable<long> collectionC, Lyst<long> lastKnown)
            {
                if ((lastKnown == null && collectionC != null) || (lastKnown != null && collectionC == null))
                {
                    return false;
                }

                Lyst<long> collection = collectionC?.ToLyst();
                if (lastKnown?.Count != collection?.Count)
                {
                    return false;
                }

                int length = lastKnown.Count;
                for (int i = 0; i < length; i++)
                {
                    if (collection[i] != lastKnown[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
