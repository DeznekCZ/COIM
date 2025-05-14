using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System.Collections.Generic;
using System.Net.Configuration;
using UnityEngine.UIElements;

namespace ProgramableNetwork
{
    public class ModuleEditDialog : Window
    {
        public ModuleEditDialog(Module module, ControllerView controllerView, UiContext uiContext, ControllerInspector controllerInspector)
            : base(module.Prototype.Strings.Name)
        {
            ScrollColumn settings = new ScrollColumn();
            settings.Gap(5.px());
            settings.Height(400.px());
            settings.Width(400.px());

            // TODO: copy, paste, template
            RowContainer row = new PanelRow();
            //row.Height(Sizes.BLOCK_SIZE);
            //row.Class(Cls.groupHeader);
            //row.AlignItemsEnd();
            Body.Add(row);
            Body.Gap(5.px());

            // add filler
            row.AddAndReturn(new UiComponent()).Fill();

            ButtonIcon moveLeft = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.ArrowLeft128_png)
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING)
                     .Margin(Px.Zero)
                .Parent.As<ButtonIcon>().Value;
            moveLeft.Tooltip("Try move module to left".ToDoLoc());
            moveLeft.OnClick(() => controllerView.Move(module, x: -1));
            this.Observe(() => controllerView.CanMove(module, x: -1))
                .Do(m => moveLeft.Enabled(m));
            row.Add(moveLeft);

            ButtonIcon moveRight = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.ArrowRight_svg)
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING)
                     .Margin(Px.Zero)
                .Parent.As<ButtonIcon>().Value;
            moveRight.Tooltip("Try move module to right".ToDoLoc());
            moveRight.OnClick(() => controllerView.Move(module, x: 1));
            this.Observe(() => controllerView.CanMove(module, x: 1))
                .Do(m => moveRight.Enabled(m));
            row.Add(moveRight);

            ButtonIcon moveUp = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.UpArrow128_png)
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING)
                     .Margin(Px.Zero)
                .Parent.As<ButtonIcon>().Value;
            moveUp.Tooltip("Try move module to up".ToDoLoc());
            moveUp.OnClick(() => controllerView.Move(module, y: -1));
            this.Observe(() => controllerView.CanMove(module, y: -1))
                .Do(m => moveUp.Enabled(m));
            row.Add(moveUp);

            ButtonIcon moveDown = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.DownArrow128_png)
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING)
                     .Margin(Px.Zero)
                .Parent.As<ButtonIcon>().Value;
            moveDown.Tooltip("Try move module to down".ToDoLoc());
            moveDown.OnClick(() => controllerView.Move(module, y: 1));
            this.Observe(() => controllerView.CanMove(module, y: 1))
                .Do(m => moveDown.Enabled(m));
            row.Add(moveDown);

            ButtonIcon copy = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.ExportToString_svg)
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING)
                     .Margin(Px.Zero)
                .Parent.As<ButtonIcon>().Value;
            copy.Tooltip("Copy to last created for next addition".ToDoLoc());
            copy.OnClick(() => ControllerView.m_lastCreated = module);
            row.Add(copy);

            ButtonIcon remove = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png)
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING)
                     .Margin(Px.Zero)
                .Parent.As<ButtonIcon>().Value;
            remove.Tooltip("Removes current module".ToDoLoc());
            remove.OnClick(() =>
            {
                controllerView.RemoveModule(module);
                Close();
            });
            row.Add(remove);

            ButtonIcon paste = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.ImportFromString_svg)
                .Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .IconSize(Sizes.IMAGE_SIZE * 2, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING, Sizes.IMAGE_PADDING)
                     .Margin(Px.Zero)
                .Parent.As<ButtonIcon>().Value;
            paste.Tooltip("Copy data from last created or coppied module".ToDoLoc());
            paste.OnClick(() =>
            {
                module.SetStatus(ModuleStatus.Init);
                module.NumberData.Clear();
                module.StringData.Clear();
                foreach (KeyValuePair<string, int> item in ControllerView.m_lastCreated.NumberData)
                    module.NumberData[item.Key] = item.Value;
                foreach (KeyValuePair<string, string> item in ControllerView.m_lastCreated.StringData)
                    module.StringData[item.Key] = item.Value;
                module.Prototype.Init(module);
            });
            this.Observe(() => ControllerView.m_lastCreated)
                .Do(m => paste.Enabled(!(m is null) && m.Prototype.Id == module.Prototype.Id));
            row.Add(paste);

            Body.Add(settings);

            foreach (IField item in module.Prototype.Fields)
            {
                item.Init(controllerInspector, controllerInspector, settings, uiContext, module, () => { });
            }

            this.Height(450.px());
            this.Width(420.px());
            this.OpenIn(controllerInspector);
        }
    }
}