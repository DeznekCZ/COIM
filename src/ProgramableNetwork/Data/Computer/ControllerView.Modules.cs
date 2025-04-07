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
    public partial class ControllerView : Column
    {
        private int m_targetRow = -1;
        private int m_targetColumn = -1;
        private ModuleProto m_newModule;
        private Module m_editModule;
        private Module m_lastCreated;
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

        private void AddModuleImplementation(Action refresh)
        {

            this.Observe(() => Entity).Do((entity) => {
                CloseDialogs();
                m_targetRow = -1;
                m_targetColumn = -1;
                if (m_editModule != null)
                    m_editModule.IsDebugging = false;
                m_editModule = null;
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
            this.Observe(() => (m_targetRow, m_targetColumn)).Do((change) => RedrawComponents(Entity.Modules.Select(m => m.Id).ToLyst()));

            //updaterBuilder.Observe(WasUpdated).Do(UpdateChanged);
        }

        //private List<IDataUpdater> WasUpdated()
        //{
        //    List<IDataUpdater> dataUpdaters = new List<IDataUpdater>();
        //    foreach (IDataUpdater item in m_updaters)
        //        if (item.WasChanged())
        //            dataUpdaters.Add(item);
        //    return dataUpdaters;
        //}

        //private void UpdateChanged(List<IDataUpdater> obj)
        //{
        //    foreach (var item in obj)
        //        item.Update();
        //}

        //private void CreateNewDialog(int targetRow, int targetColumn, bool templates)
        //{
        //    CloseDialogs();

        //    m_targetRow = targetRow;
        //    m_targetColumn = targetColumn;

        //    m_newDialog = Builder.NewStackContainer("dialogNew")
        //        .SetItemSpacing(5)
        //        .SetStackingDirection(StackContainer.Direction.TopToBottom)
        //        .SetSizeMode(StackContainer.SizeMode.Dynamic)
        //        .SetWidth(400);

        //    if (templates)
        //    {
        //        // TODO add template browser in Update 3

        //        Dictionary<string, Template> protos = TemplateRegistrator.GetTemplates();

        //        Builder.NewTxt("dialogNewTemplates")
        //            .SetText(NewTr.Tools.Templates)
        //            .SetParent(m_newDialog, true)
        //            .SetHeight(20)
        //            .AppendTo(m_newDialog);
        //        foreach (KeyValuePair<string, Template> item in protos)
        //        {
        //            string text = item.Value.ModuleProto.Strings.Name.TranslatedString;
        //            if (item.Value.Name != null)
        //                text += ": " + item.Value.Name;

        //            Builder.NewBtnGeneral($"dialogNewCreateTemplate_{item.Key}")
        //                .SetText(text)
        //                .SetParent(m_newDialog, true)
        //                .SetHeight(20)
        //                .OnClick(() =>
        //                {
        //                    if (TryPlaceAt(item.Value.ModuleProto, m_targetRow, m_targetColumn))
        //                    {
        //                        m_editModule.Prototype.ExecuteInit(m_editModule);
        //                        item.Value.Setting(m_editModule);
        //                        CreateEditDialog(m_editModule);
        //                    }
        //                })
        //                .AppendTo(m_newDialog);
        //        }
        //    }
        //    else
        //    {
        //        Action refresh = () => CreateNewDialog(targetRow, targetColumn, templates);

        //        Builder.NewTxt("dialogNewPick")
        //            .SetText(NewTr.Tools.Pick)
        //            .SetParent(m_newDialog, true)
        //            .SetHeight(20)
        //            .AppendTo(m_newDialog);

        //        Dropdwn catPicker = Builder.NewDropdown("dialogNewCatPicker")
        //            .SetParent(m_newDialog, true)
        //            .SetHeight(20)
        //            .AppendTo(m_newDialog);

        //        FillCategoryPicker(catPicker, refresh);

        //        Dropdwn typePicker = Builder.NewDropdown("dialogNewTypePicker")
        //            .SetParent(m_newDialog, true)
        //            .SetHeight(20)
        //            .AppendTo(m_newDialog);

        //        FillModulePicker(typePicker, m_category, refresh);

        //        Builder.NewBtnGeneral("dialogNewCreate")
        //            .SetText(NewTr.Tools.Add)
        //            .SetParent(m_newDialog, true)
        //            .SetHeight(20)
        //            .SetEnabled(m_newModule != null)
        //            .OnClick(() =>
        //            {
        //                if (TryPlaceAt(m_newModule, m_targetRow, m_targetColumn))
        //                {
        //                    m_editModule.Prototype.ExecuteInit(m_editModule);
        //                    CreateEditDialog(m_editModule);
        //                }
        //            })
        //            .AppendTo(m_newDialog);

        //    }

        //    m_newDialog.AppendTo(m_moduleDialog);
        //    m_newDialog.SetParent(m_moduleDialog, true);
        //    m_moduleDialog.SetWidth(400);
        //}

        //private void FillCategoryPicker(Dropdwn picker, Action refresh)
        //{
        //    List<Category> types = Category.Categories(Entity.Context.ProtosDb, Entity);
        //    types.Insert(0, new Category("all", "All"));

        //    var typeStrings = types.Select(t => t.Name).ToList();

        //    var selected = types.SelectIndicesWhere(t => t.Id == m_category?.Id).FirstOrDefault();
        //    picker.AddOptions(typeStrings);
        //    picker.OnValueChange(index =>
        //    {
        //        m_category = types[index];
        //        m_newModule = null;
        //        refresh();
        //    });

        //    picker.SetValueWithoutNotify(selected);

        //    if (m_category == null)
        //        m_category = types[0];
        //}

        //private void FillModulePicker(Dropdwn picker, Category category, Action refresh)
        //{
        //    var typesAll = Entity.Context.ProtosDb.All<ModuleProto>();
        //    List<ModuleProto> types = 
        //        (category.Id == "all"
        //            ? Entity.Context.ProtosDb.All<ModuleProto>()
        //            : Entity.Context.ProtosDb.All<ModuleProto>()
        //                  .Where(module => module.Categories.Contains(category)))
        //        .Where(Entity.Prototype.AllowedModule)
        //        .OrderBy(t => t.Strings.Name.TranslatedString)
        //        .ToList();

        //    var typeStrings = types
        //        .Select(t => t.Strings.Name.TranslatedString)
        //        .ToList();
        //    var selected = types.SelectIndicesWhere(t => t.Id == m_newModule?.Id).FirstOrDefault();
        //    picker.AddOptions(typeStrings);
        //    picker.OnValueChange(index =>
        //    {
        //        m_newModule = types[index];
        //        refresh();
        //    });

        //    picker.SetValueWithoutNotify(selected);

        //    if (m_newModule == null && types.Count > 0)
        //        m_newModule = types[0];
        //    else if (types.Count == 0)
        //        m_newModule = null;
        //}

        //private void CreateEditDialog(Module module)
        //{
        //    CloseDialogs();

        //    if (m_editModule != null)
        //        m_editModule.IsDebugging = false;
        //    if (module != null)
        //        module.IsDebugging = m_debugging;

        //    m_editModule = module;
        //    m_targetRow = -1;
        //    m_targetColumn = -1;

        //    for (int i = 0; i < Entity.Rows.Count; i++)
        //    {
        //        for (int j = 0; j < Entity.Rows[i].Count; j++)
        //        {
        //            if (Entity.Rows[i][j].Placement && Entity.Rows[i][j].ModuleId == module?.Id)
        //            {
        //                m_targetRow = i;
        //                m_targetColumn = j;
        //            }
        //        }
        //    }

        //    m_editDialog = Builder.NewStackContainer("dialogEdit")
        //        .SetItemSpacing(5)
        //        .SetStackingDirection(StackContainer.Direction.TopToBottom)
        //        .SetSizeMode(StackContainer.SizeMode.Dynamic)
        //        .SetWidth(400);

        //    var moduleName = Builder.NewTxt("dialogEditName")
        //        .SetParent(m_editDialog, true)
        //        .SetText(m_editModule.Prototype.Strings.Name)
        //        .SetHeight(20)
        //        .AppendTo(m_editDialog);

        //    CreateSettingsPanel(m_editDialog, () => m_editModule, () => CreateEditDialog(module));

        //    Builder.NewBtnGeneral("dialogEditRemove")
        //        .SetText(NewTr.Tools.Remove)
        //        .SetHeight(20)
        //        .OnClick(() =>
        //        {
        //            Entity.Modules.Remove(m_editModule);
        //            for (int i = m_targetColumn; i < Entity.Rows[m_targetRow].Count; i++)
        //            {
        //                // ignored width because of possible deprecation or phantoms
        //                if (Entity.Rows[m_targetRow][i].ModuleId != m_editModule.Id)
        //                    break;
        //                Entity.Rows[m_targetRow][i] = 0;
        //            }
        //            m_container.HideItem(m_editDialog);
        //        })
        //        .AppendTo(m_editDialog);

        //    m_editDialog.AppendTo(m_moduleDialog);
        //    m_editDialog.SetParent(m_moduleDialog, true);
        //    m_moduleDialog.SetWidth(400);
        //}

        private void CloseDialogs()
        {
            //try { if (m_editDialog != null) m_moduleDialog.RemoveAndDestroy(m_editDialog); m_editDialog = null; }
            //catch (Exception) { Console.WriteLine("Failed to delete edit dialog"); }
            //try { if (m_newDialog != null) m_moduleDialog.RemoveAndDestroy(m_newDialog); m_newDialog = null; }
            //catch (Exception) { Console.WriteLine("Failed to delete new dialog"); }
            //m_moduleDialog.SetWidth(400);
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

            for (int i = 0; i < Entity.Rows.Count; i++)
            {
                var rowElement = new Row();
                rowElement.Height(80);
                rowElement.Width(Entity.Prototype.Columns * 20);

                var row = Entity.Rows[i];
                for (int j = 0; j < row.Count; j++)
                {
                    if (!row[j].Placement) continue;

                    bool selected = i == m_targetRow && j == m_targetColumn;

                    var module = (Entity.Modules ?? new Lyst<Module>())
                        .AsEnumerable()
                        .FirstOrDefault(m => m.Id == row[j].ModuleId);

                    if (module == null)
                    {
                        AddFreeSlot(rowElement, i, j, selected);
                        continue;
                    }
                    rowElement.Add(new ModuleView(module, this, m_controller.Context, selected, () => RedrawComponents(modules)));
                }

                Add(rowElement);
            }
        }

        private void AddFreeSlot(Row rowElement, int targetRow, int targetColumn, bool selected)
        {
            ButtonText button = new ButtonText(new LocStrFormatted("+"));
            button.OnClick(() =>
            {
                ProtoPickerPopup<AModuleProtoSelector> protoPicker = new ProtoPickerPopup<AModuleProtoSelector>(
                    optionsProvider: () => NewModulePicker(targetRow, targetColumn),
                    optionViewFactory: (s) => s.CreateUi(),
                    onOptionSelected: (s) => s.Selected(),
                    button: button,
                    title: new LocStrFormatted("Add module"),
                    config: new ProtoPickerConfig { ItemsPerRow = 1 },
                    orderAlphabetically: false,
                    searchable: true
                );
                protoPicker.Show();
            });
        }

        private IEnumerable<AModuleProtoSelector> NewModulePicker(int targetRow, int targetColumn)
        {
            Controller controller = m_controller.Entity;
            StaticEntityProto.ID id = controller.Prototype.Id;

            if (m_lastCreated != null && m_lastCreated.Prototype.AllowedDevices.Contains(id))
                yield return new LastCreatedModule(m_controller.Entity, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
                {
                    if (TryPlaceAt(moduleProto, targetRow, targetColumn))
                    {
                        return (true, m_editModule);
                    }
                    else
                    {
                        return (false, null);
                    }
                }, m_lastCreated);

            if (TemplateRegistrator.GetTemplates().Count > 0)
                foreach (KeyValuePair<string, Template> item in TemplateRegistrator.GetTemplates())
                    yield return new TemplateModule(m_controller.Entity, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
                    {
                        if (TryPlaceAt(moduleProto, targetRow, targetColumn))
                        {
                            return (true, m_editModule);
                        }
                        else
                        {
                            return (false, null);
                        }
                    }, item);

            foreach (ModuleProto item in m_controller.Entity.Context.ProtosDb
                                            .All<ModuleProto>()
                                            .Where(p => p.IsAvailable)
                                            .Where(p => p.AllowedDevices.Contains(id)))
                yield return new NewModule(m_controller.Entity, m_refresh, (m) => m_lastCreated = m, (moduleProto) =>
                {
                    if (TryPlaceAt(moduleProto, targetRow, targetColumn))
                    {
                        return (true, m_editModule);
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
                m_editModule = module;
                m_targetRow = targetRow;
                m_targetColumn = targetColumn;
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
