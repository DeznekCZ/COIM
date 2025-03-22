using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.GameLoop;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork.Data.Variables
{
    [GlobalDependency(RegistrationMode.AsSelf)]
    public class VariableWindow : WindowView
    {
        private StackContainer m_variables;
        private readonly VariableManager m_variableManager;
        private readonly List<IDataUpdater> m_updaters;
        private readonly IGameLoopEvents m_gameLoop;

        public StackContainer ItemsContainer { get; private set; }

        public VariableWindow(VariableManager variableManager, IUnityInputMgr unityInput, UiBuilder uiBuilder, IGameLoopEvents gameLoop)
            : base("variables", FooterStyle.Round, false)
        {
            m_variableManager = variableManager;
            m_gameLoop = gameLoop;
            m_updaters = new List<IDataUpdater>();
            unityInput.RegisterGlobalShortcut(
                m => KeyBindings.FromPrimaryKeys(KbCategory.General, ShortcutMode.Game, KeyCode.LeftControl, KeyCode.N),
                () =>
                {
                    BuildAndShow(uiBuilder);
                });
            SetOnCloseButtonClickAction(Hide);
            OnShowStart += VariableWindow_OnShowStart;
            OnHide += VariableWindow_OnHide;
        }

        private void VariableWindow_OnHide()
        {
            m_gameLoop.SyncUpdate.RemoveNonSaveable(this, SyncUpdate);
            m_gameLoop.RenderUpdate.RemoveNonSaveable(this, RenderUpdate);
        }

        private void VariableWindow_OnShowStart()
        {
            m_gameLoop.SyncUpdate.AddNonSaveable(this, SyncUpdate);
            m_gameLoop.RenderUpdate.AddNonSaveable(this, RenderUpdate);
        }

        protected override void BuildWindowContent()
        {
            SetTitle("Network variables");
            PositionSelfToCenter();
            MakeMovable();

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            ItemsContainer = Builder.NewStackContainer("ItemsContainer")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetWidth(500)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetInnerPadding(Offset.Top(Style.Panel.Padding))
                .PutTo(GetContentPanel());

            var bar = Builder.NewStackContainer("StatusBar")
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetHeight(40)
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .AppendTo(ItemsContainer);

            var statusPanel = new StatusPanel(bar, Builder)
                .SetHeight(40)
                .AppendTo(bar);

            updaterBuilder.Observe(() => m_variableManager.AllVariables.Count)
                .Do((i) => statusPanel.SetStatus($"Stored variables: {i}",
                    i < 50 ? StatusPanel.State.Ok :
                    i < 100 ? StatusPanel.State.Warning :
                    StatusPanel.State.Critical));

            // Header
            AddSectionTitle(ItemsContainer, "Variables:");

            StackContainer container = Builder.NewStackContainer("header")
                .SetParent(ItemsContainer)
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetHeight(20)
                .SetWidth(500)
                .AppendTo(ItemsContainer);
            Txt name = Builder.NewTxt("headerName")
                .SetWidth(200)
                .SetText("Name:")
                .AppendTo(container);
            Txt value = Builder.NewTxt("headerValue")
                .SetWidth(300)
                .SetText("Value:")
                .AppendTo(container);

            // TODO search bar
            m_variables = Builder.NewStackContainer("variables")
                .SetParent(ItemsContainer)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .AppendTo(ItemsContainer);

            updaterBuilder
                .Observe(() => m_variableManager.AllVariables.Keys, new KeyStringComparer())
                .Do(Refresh);

            updaterBuilder
                .Observe(() => DateTime.Now.Ticks)
                .Do((l) => m_updaters.ForEach(u => u.Update()));

            AddUpdater(updaterBuilder.Build());

            ItemsContainer.SizeChanged += delegate
            {
                SetContentSize(500, Math.Max(ItemsContainer.GetDynamicHeight(), 300));
            };
            SetContentSize(500, Math.Max(ItemsContainer.GetDynamicHeight(), 300));
        }

        private void Refresh(Lyst<string> lyst)
        {
            if (lyst is null) return;

            m_variables.ClearAll();
            m_updaters.Clear();

            foreach (var item in lyst)
            {
                m_variables.AppendDivider(2, Style.EntitiesMenu.MenuBg);

                StackContainer container = Builder.NewStackContainer(DateTime.Now.Ticks.ToString())
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetHeight(20)
                    .SetWidth(500)
                    .AppendTo(m_variables);
                Txt name = Builder.NewTxt(DateTime.Now.Ticks.ToString())
                    .SetWidth(200)
                    .SetText(item)
                    .AppendTo(container);
                Txt value = Builder.NewTxt(DateTime.Now.Ticks.ToString())
                    .SetWidth(300)
                    .SetText(m_variableManager.GetVariable(item).ToString())
                    .SetTextStyle(Builder.Style.Global.Text.Extend(ColorRgba.White))
                    .AppendTo(container);

                m_updaters.Add(new DataUpdater<string, Txt>(
                    (c) => c.Text,
                    (c, v) => c.SetText(m_variableManager.GetVariable(item).ToString()),
                    string.Equals,
                    value));
            }

            Builder.NewStackContainer("filler")
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .AppendTo(m_variables);
        }
    }
}