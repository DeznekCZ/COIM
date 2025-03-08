﻿using Mafi.Core.Syncers;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System.Linq;
using Mafi;
using System;

namespace ProgramableNetwork
{
    public partial class ControllerView
    {
        private ModuleConnector m_higlighted;

        private class ModuleView : StackContainer
        {
            private readonly Module m_module;
            private readonly ControllerInspector m_controller;
            private readonly ControllerView m_computerView;

            public ModuleView(UiBuilder uiBuilder, Module module, ControllerInspector controller, ControllerView computerView, bool selected, Action refresh)
                : base(uiBuilder, "moduleView_" + module.Id)
            {
                this.m_module = module;
                this.m_controller = controller;
                this.m_computerView = computerView;
                string name = "moduleView_" + module.Id;
                var updater = UpdaterBuilder.Start();
                int width = module.Layout.GetWidth(module);
                bool displaysExists = module.Prototype.Displays.Count > 0;

                this.SetSize(width * 20, 80);
                this.SetStackingDirection(Direction.TopToBottom);
                this.SetItemSpacing(0);
                this.SetSizeMode(SizeMode.StaticDirectionAligned);

                // Add Input panel
                StackContainer inputsPanel = uiBuilder.NewStackContainer(name + "_inputs")
                    .SetParent(this, true)
                    .SetSize(width * 20, 20)
                    .SetBackground(ColorRgba.DarkGreen)
                    .SetSizeMode(SizeMode.StaticCenterAligned)
                    .SetStackingDirection(Direction.RightToLeft);
                AddInputs(uiBuilder, inputsPanel, module, updater, refresh);

                inputsPanel.AppendTo(this);


                // Add Field panel
                Btn fieldsPanel = uiBuilder.NewBtnGeneral(name + "_edit")
                    .SetParent(this, true)
                    .SetSize(width * 20, displaysExists ? 20 : 40)
                    .SetText(module.Prototype.Symbol)
                    .OnClick(() =>
                    {
                        m_computerView.CreateEditDialog(module);
                    });

                m_computerView.m_updaters.Add(new DataUpdater<BtnStyle, int>(
                    (context) =>
                    {
                        var baseStyle = (selected ? uiBuilder.Style.Global.GeneralBtnActive : uiBuilder.Style.Global.GeneralBtn)
                            .Extend(backgroundClr: ColorRgba.DarkGreen);

                        if (module.Status == ModuleStatus.Running)
                            return baseStyle.Extend(
                                backgroundClr: ColorRgba.DarkGreen);

                        else if (module.Status == ModuleStatus.Error)
                            return baseStyle.Extend(
                                backgroundClr: ColorRgba.DarkRed);

                        return baseStyle;
                    },
                    (context, style) => fieldsPanel.SetButtonStyle(style),
                    (styleA, styleB) => styleA.Equals(styleB),
                    0
                ));

                DataUpdater<string, int> tooltipUpdater;
                m_computerView.m_updaters.Add(tooltipUpdater = new DataUpdater<string, int>(
                    (context) => module.Error,
                    (context, style) => { },
                    (oldError, newError) => oldError != newError,
                    0
                ));

                fieldsPanel.ToolTip(controller, tooltipUpdater.GetValue, attached: true);

                fieldsPanel.AppendTo(this);

                if (selected)
                    fieldsPanel.SetButtonStyle(uiBuilder.Style.Global.GeneralBtnActive);

                if (displaysExists)
                {
                    StackContainer displaysPanel = uiBuilder.NewStackContainer(name + "_displays")
                        .SetParent(this, true)
                        .SetSize(width * 20, 20)
                        .SetBackground(ColorRgba.DarkDarkGray)
                        .SetSizeMode(SizeMode.StaticCenterAligned)
                        .SetStackingDirection(Direction.LeftToRight);
                    AddDisplays(uiBuilder, displaysPanel, module, updater, refresh);

                    displaysPanel.AppendTo(this);
                }

                // Add Ouptut panel
                StackContainer outputsPanel = uiBuilder.NewStackContainer(name + "_outputs")
                    .SetParent(this, true)
                    .SetSize(width * 20, 20)
                    .SetBackground(ColorRgba.DarkRed)
                    .SetSizeMode(SizeMode.StaticCenterAligned)
                    .SetStackingDirection(Direction.RightToLeft);
                AddOutputs(uiBuilder, outputsPanel, module, updater, refresh);

                outputsPanel.AppendTo(this);

                var updaterBuilt = updater.Build();
                computerView.AddUpdater(updaterBuilt);

                this.RectTransform.ForceUpdateRectTransforms();
            }

            private void AddInputs(UiBuilder builder, StackContainer inputsPanel, Module module, UpdaterBuilder updater, Action refresh)
            {
                var inputs = module.Prototype.Inputs;
                for (int i = inputs.Count - 1; i >= 0; i--)
                {
                    var input = inputs[i];
                    bool isConnected = module.InputModules.ContainsKey(input.Id);

                    Btn btn = builder.NewBtnGeneral($"{module.Id}_input_{i}");
                    btn.SetText("O")
                        .SetButtonStyle((
                                isConnected ? builder.Style.Global.GeneralBtnActive : builder.Style.Global.GeneralBtn
                            ).Extend(backgroundClr: ColorRgba.DarkGreen))
                        .SetSize(20, 20)
                        .OnRightClick(() =>
                        {
                            if (module.InputModules.TryRemove(input.Id, out _))
                            {
                                refresh();
                            }
                            else
                            {
                                builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp).Play();
                            }
                        })
                        .OnClick(() =>
                        {
                            if (m_computerView.OutputConnection == null)
                            {
                                builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp).Play();
                            }
                            else
                            {
                                module.InputModules[input.Id] = m_computerView.OutputConnection;
                                refresh();
                            }
                        })
                        .SetOnMouseEnterLeaveActions(
                            () => { },
                            () => { }
                        )
                        .AppendTo(inputsPanel)
                        .ToolTip(m_computerView.m_controller, (input.Name.Name + ": " + input.Name.DescShort).TrimEnd(':', ' '),
                            offset: Offset.Top(-5), attached: true);

                    m_computerView.m_updaters.Add(new DataUpdater<BtnStyle, int>(
                        (context) =>
                        {
                            var baseStyle = (isConnected ? builder.Style.Global.GeneralBtnActive : builder.Style.Global.GeneralBtn)
                                .Extend(backgroundClr: ColorRgba.DarkGreen);

                            if (isConnected && m_computerView.m_higlighted != null &&
                                module.InputModules
                                    .Where(pair => pair.Key == input.Id)
                                    .Select(pair => pair.Value)
                                    .Any(connector => connector.Equals(m_computerView.m_higlighted)))
                                return baseStyle.Extend(
                                    backgroundClr: ColorRgba.Green,
                                    text: baseStyle.Text.Extend(ColorRgba.White));

                            return baseStyle;
                        },
                        (context, style) => btn.SetButtonStyle(style),
                        (styleA, styleB) => styleA.Equals(styleB),
                        0
                    ));
                }
            }

            private void AddOutputs(UiBuilder builder, StackContainer inputsPanel, Module module, UpdaterBuilder updater, Action refresh)
            {
                var outputs = module.Prototype.Outputs;
                for (int i = outputs.Count - 1; i >= 0; i--)
                {
                    var output = outputs[i];
                    bool isConnected = module.Controller.Modules
                        .AsEnumerable()
                        .Where(m => m.InputModules.Count > 0)
                        .SelectMany(m => m.InputModules)
                        .Select(p => p.Value)
                        .FirstOrDefault(c => c.ModuleId == module.Id
                                          && c.OutputId == output.Id) != null;

                    var btn = builder.NewBtnGeneral($"{module.Id}_output_{i}")
                        .SetText("O")
                        .SetButtonStyle((
                                isConnected ? builder.Style.Global.GeneralBtnActive : builder.Style.Global.GeneralBtn
                            ).Extend(backgroundClr: ColorRgba.DarkRed))
                        .SetSize(20, 20)
                        .OnRightClick(() =>
                        {
                            if (!isConnected)
                            {
                                // module not found, is not unassignable
                                builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp).Play();
                                return;
                            }

                            foreach (var target in m_controller.SelectedEntity.Modules)
                            {
                                foreach (var connection in target.InputModules)
                                {
                                    if (connection.Value.ModuleId == module.Id)
                                    {
                                        target.InputModules.TryRemove(connection.Key, out _);
                                        refresh();
                                        return;
                                    }
                                }
                            }
                        })
                        .OnClick(() => {
                            if (m_computerView.OutputConnection != null
                                && m_computerView.OutputConnection.ModuleId == module.Id
                                && m_computerView.OutputConnection.OutputId == output.Id)
                            {
                                m_computerView.OutputConnection = null;
                            }
                            else
                            {
                                m_computerView.OutputConnection = new ModuleConnector(module.Id, output.Id);
                            }
                        })
                        .SetOnMouseEnterLeaveActions(
                            () => { m_computerView.m_higlighted = new ModuleConnector(module.Id, output.Id); },
                            () => { m_computerView.m_higlighted = null; }
                        )
                        .AppendTo(inputsPanel)
                        .ToolTip(m_computerView.m_controller, (output.Name.Name + ": " + output.Name.DescShort).TrimEnd(':', ' '),
                            offset: Offset.Top(20), attached: true);

                    m_computerView.m_updaters.Add(new DataUpdater<BtnStyle, int>(
                        (context) =>
                        {
                            var baseStyle = (
                                isConnected ? builder.Style.Global.GeneralBtnActive : builder.Style.Global.GeneralBtn
                            ).Extend(backgroundClr: ColorRgba.DarkRed);

                            if (m_computerView.OutputConnection != null
                                && m_computerView.OutputConnection.ModuleId == module.Id
                                && m_computerView.OutputConnection.OutputId == output.Id)
                                return baseStyle.Extend(
                                    text: baseStyle.Text.Extend(ColorRgba.Green));

                            return baseStyle;
                        },
                        (context, style) => btn.SetButtonStyle(style),
                        (styleA, styleB) => styleA.Equals(styleB),
                        0
                    ));
                }
            }

            private void AddDisplays(UiBuilder builder, StackContainer displaysPanel, Module module, UpdaterBuilder updater, Action refresh)
            {
                var displays = module.Prototype.Displays;
                for (int i = displays.Count - 1; i >= 0; i--)
                {
                    var display = displays[i];

                    var text = builder.NewBtnGeneral($"{module.Id}_display_{i}")
                        .SetSize(20 * display.Width, 20)
                        .AppendTo(displaysPanel);

                    if (display.DefaultText == "[image]")
                    {
                        ImageDisplay(builder, module, display, text);
                    }
                    else if (display.DefaultText.StartsWith("[toggle]"))
                    {
                        ToggleDisplay(builder, module, display, text);
                    }
                    else
                    {
                        TextDisplay(builder, module, display, text);
                    }
                }
            }

            private void TextDisplay(UiBuilder builder, Module module, ModuleConnectorProto display, Btn text)
            {
                text.SetButtonStyle(builder.Style.Global.GeneralBtn.ExtendText(color: ColorRgba.White));
                text.SetText(module.Display[display.Id, display.DefaultText]);

                m_computerView.m_updaters.Add(new DataUpdater<
                        string,
                        (Module module, Btn text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[c.display.Id, c.display.DefaultText],
                    setter: (c, t) => c.text.SetText(t),
                    comparator: string.Equals,
                    context: (module, text, display)
                ));
            }

            private void ImageDisplay(UiBuilder builder, Module module, ModuleConnectorProto display, Btn text)
            {
                text.SetButtonStyle(builder.Style.Global.ImageBtn);
                text.SetIcon(module.Display[display.Id, builder.Style.Icons.Empty]);
                string imagePath = display.DefaultText.Replace("[image]", "") + builder.Style.Icons.Empty;

                m_computerView.m_updaters.Add(new DataUpdater<
                        string,
                        (Module module, Btn text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[c.display.Id, builder.Style.Icons.Empty],
                    setter: (c, t) => c.text.SetIcon(t),
                    comparator: string.Equals,
                    context: (module, text, display)
                ));
            }

            private void ToggleDisplay(UiBuilder builder, Module module, ModuleConnectorProto display, Btn text)
            {
                char separator = display.DefaultText["[toggle]".Length];
                string[] options = display.DefaultText.Replace($"[toggle]{separator}", "").Split(separator);

                if (options.Length == 1 && options[0].Length == 0)
                {
                    ToggleDisplay_LED(builder, module, display, text);
                }
                else if (options.Length == 1)
                {
                    ToggleDisplay_Symbol(builder, module, display, text, options[0]);
                }
                else if (options.Length == 2)
                {
                    ToggleDisplay_DoubleText(builder, module, display, text, options);
                }
                else
                {
                    text.SetButtonStyle(builder.Style.Global.GeneralBtn.ExtendText(color: ColorRgba.White));
                    text.SetText(module.Display[display.Id, display.DefaultText]);

                    m_computerView.m_updaters.Add(new DataUpdater<
                            string,
                            (Module module, Btn text, ModuleConnectorProto display)
                        >(
                        getter: (c) => c.module.Display[c.display.Id, c.display.DefaultText],
                        setter: (c, t) => c.text.SetText(t),
                        comparator: string.Equals,
                        context: (module, text, display)
                    ));
                }
            }

            private void ToggleDisplay_DoubleText(UiBuilder builder, Module module, ModuleConnectorProto display, Btn text, string[] options)
            {
                text.SetButtonStyle(builder.Style.Global.GeneralBtn.ExtendText(color: ColorRgba.White));
                text.SetText(module.Display[display.Id, options[0]]);

                m_computerView.m_updaters.Add(new DataUpdater<
                        string,
                        (Module module, Btn text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[display.Id, options[0]],
                    setter: (c, t) => c.text.SetText(t),
                    comparator: string.Equals,
                    context: (module, text, display)
                ));

                text.OnClick(() =>
                {
                    if (module.Display[display.Id, options[0]] == options[0])
                        module.Display[display.Id] = options[1];
                    else
                        module.Display[display.Id] = options[0];
                });
            }

            private void ToggleDisplay_Symbol(UiBuilder builder, Module module, ModuleConnectorProto display, Btn text, string symbol)
            {
                text.SetText(symbol);
                if (module.Display[display.Id, ""].Length > 0)
                {
                    text.SetButtonStyle(builder.Style.Global.GeneralBtnActive.ExtendText(color: ColorRgba.Green));
                }
                else
                {
                    text.SetButtonStyle(builder.Style.Global.GeneralBtnActive.ExtendText(color: ColorRgba.Red));
                }

                m_computerView.m_updaters.Add(new DataUpdater<
                        ColorRgba,
                        (Module module, Btn text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[display.Id, ""].Length > 0 ? ColorRgba.Green : ColorRgba.Red,
                    setter: (c, t) => c.text.SetButtonStyle(builder.Style.Global.GeneralBtnActive.ExtendText(color: t)),
                    comparator: (a, b) => a == b,
                    context: (module, text, display)
                ));

                text.OnClick(() =>
                {
                    if (module.Display[display.Id, ""].Length > 0)
                        module.Display[display.Id] = "";
                    else
                        module.Display[display.Id] = "1";
                });
            }

            private void ToggleDisplay_LED(UiBuilder builder, Module module, ModuleConnectorProto display, Btn text)
            {
                ToggleDisplay_Symbol(builder, module, display, text, "●");
            }
        }
    }
}
