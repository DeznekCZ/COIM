using Mafi.Core.Syncers;
using System.Linq;
using Mafi;
using System;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.Ui;
using System.Runtime.Remoting.Contexts;
using System.Drawing.Drawing2D;
using Mafi.Unity.UiToolkit;

namespace ProgramableNetwork
{
    public partial class ControllerView
    {
        private ModuleConnector m_higlighted;

        public ModuleConnector OutputConnection { get; set; }

        private class ModuleView : UiComponent
        {
            private readonly Module m_module;
            private readonly ControllerView m_controller;

            public ModuleView(Module module, ControllerView controllerView, UiContext uiContext, bool selected, Action refresh)
                : base(new UnityEngine.UIElements.VisualElement())
            {
                this.m_module = module;
                this.m_controller = controllerView;
                string name = "moduleView_" + module.Id;
                var updater = UpdaterBuilder.Start();
                int width = module.Layout.GetWidth(module);
                bool displaysExists = module.Prototype.Displays.Count > 0;

                this.Size(width * 20, 80);

                // Add Input panel
                Row inputsPanel = new Row();
                inputsPanel.Size((width * 20).px(), 20.px());
                inputsPanel.Background(ColorRgba.DarkGreen);
                AddInputs(uiContext, inputsPanel, module, refresh);
                Add(inputsPanel);

                m_controller.m_updaters.Add(new EachFrame(() => module.Prototype.DisplayUpdate(module)));

                // Add Field panel
                ButtonText fieldsPanel = new ButtonText(new Mafi.Localization.LocStrFormatted(module.Prototype.Symbol));
                fieldsPanel.Size(width * 20, displaysExists ? 20 : 40);
                fieldsPanel.OnMouseEnterLeave(
                        () => m_controller.AddPreviewHighlight(module),
                        () => m_controller.ClearPreviewHighlight()
                    );
                fieldsPanel.OnClick(() =>
                    {
                        new ModuleEditDialog(module, m_controller, uiContext, m_controller.m_controller).Show();
                    });
                Add(fieldsPanel);

                m_controller.m_updaters.Add(new DataUpdater<(bool selected, ColorRgba color), int>(
                    (context) =>
                    {
                        if (module.Status == ModuleStatus.Error)
                            return (selected, ColorRgba.DarkRed);

                        return (selected, ColorRgba.DarkGreen);
                    },
                    (context, style) => {
                        fieldsPanel.BackgroundTint(style.color);
                        fieldsPanel.Selected(style.selected);
                    },
                    (styleA, styleB) => styleA.Equals(styleB),
                    0
                ));

                DataUpdater<string, int> tooltipUpdater;
                m_controller.m_updaters.Add(tooltipUpdater = new DataUpdater<string, int>(
                    // TODO show multiple tooltips
                    (context) => module.Error,
                    (context, style) => fieldsPanel.Tooltip(new Mafi.Localization.LocStrFormatted(style), enabled: !string.IsNullOrEmpty(style), isError: true),
                    (oldError, newError) => oldError != newError,
                    0
                ));

                if (selected)
                {
                    fieldsPanel.BackgroundTint(ColorRgba.DarkGreen);
                    fieldsPanel.Selected(selected);
                }

                if (displaysExists)
                {
                    Row displaysPanel = new Row()
                        .Size((width * 20).px(), 20.px())
                        .Background(ColorRgba.DarkDarkGray);
                    AddDisplays(uiContext, displaysPanel, module, refresh);

                    Add(displaysPanel);
                }

                // Add Ouptut panel
                Row outputsPanel = new Row()
                    .Size(width * 20, 20)
                    .Background(ColorRgba.DarkRed);
                AddOutputs(uiContext, outputsPanel, module, refresh);

                Add(outputsPanel);
            }

            private void AddInputs(UiContext uiContext, Row inputsPanel, Module module, Action refresh)
            {
                var inputs = module.Prototype.Inputs;
                for (int i = inputs.Count - 1; i >= 0; i--)
                {
                    var input = inputs[i];
                    bool isConnected = module.InputModules.ContainsKey(input.Id);

                    ButtonText btn = new ButtonText(new Mafi.Localization.LocStrFormatted(isConnected ? "◎" : "○"))
                        .Background(ColorRgba.DarkRed)
                        .Color(ColorRgba.Gold)
                        .Size(20.px(), 20.px())
                        .OnRightClick(() =>
                        {
                            if (module.InputModules.TryRemove(input.Id, out _))
                            {
                                refresh();
                            }
                            else
                            {
                                uiContext.AudioDb.InvalidOp(true).Play();
                            }
                        })
                        .OnClick(() =>
                        {
                            if (m_controller.OutputConnection == null)
                            {
                                uiContext.AudioDb.InvalidOp(true).Play();
                            }
                            else
                            {
                                module.InputModules[input.Id] = m_controller.OutputConnection;
                                refresh();
                            }
                        })
                        //.OnMouseEnterLeave(
                        //    () => { },
                        //    () => { }
                        //)
                        .Tooltip(new Mafi.Localization.LocStrFormatted((input.Name.Name + ": " + input.Name.DescShort).TrimEnd(':', ' ')));
                    inputsPanel.Add(btn);

                    m_controller.m_updaters.Add(new DataUpdater<(ColorRgba text, ColorRgba background), int>(
                        (context) =>
                        {
                            var text = ColorRgba.Gold;
                            var background = ColorRgba.DarkGreen;

                            if (isConnected && m_controller.m_higlighted != null &&
                                module.InputModules
                                    .Where(pair => pair.Key == input.Id)
                                    .Select(pair => pair.Value)
                                    .Any(connector => connector.Equals(m_controller.m_higlighted)))
                            {
                                text = ColorRgba.White;
                                background = ColorRgba.DarkGreen;
                            }

                            return (text, background);
                        },
                        (context, style) =>
                        {
                            btn.Color(style.text);

                        },
                        (styleA, styleB) => styleA.Equals(styleB),
                        0
                    ));
                }
            }

            private void AddOutputs(UiContext uiContext, Row inputsPanel, Module module, Action refresh)
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

                    ButtonText btn = new ButtonText(new Mafi.Localization.LocStrFormatted(isConnected ? "◎" : "○"))
                        .Background(ColorRgba.DarkRed)
                        .Color(ColorRgba.Gold)
                        .Size(20.px(), 20.px())
                        .OnRightClick(() =>
                        {
                            if (!isConnected)
                            {
                                // module not found, is not unassignable
                                uiContext.AudioDb.InvalidOp(true).Play();
                                return;
                            }

                            foreach (var target in m_controller.Entity.Modules)
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
                        .OnClick(() =>
                        {
                            if (m_controller.OutputConnection != null
                                && m_controller.OutputConnection.ModuleId == module.Id
                                && m_controller.OutputConnection.OutputId == output.Id)
                            {
                                m_controller.OutputConnection = null;
                            }
                            else
                            {
                                m_controller.OutputConnection = new ModuleConnector(module.Id, output.Id);
                            }
                        })
                        .Tooltip(new Mafi.Localization.LocStrFormatted((output.Name.Name + ": " + output.Name.DescShort).TrimEnd(':', ' ')));
                    btn.OnMouseEnterLeave(
                            () => { m_controller.m_higlighted = new ModuleConnector(module.Id, output.Id); },
                            () => { m_controller.m_higlighted = null; }
                        );
                    inputsPanel.Add(btn);

                    m_controller.m_updaters.Add(new DataUpdater<(ColorRgba text, ColorRgba background), int>(
                        (context) =>
                        {
                            var text = ColorRgba.Gold;
                            var background = ColorRgba.DarkGreen;

                            if (m_controller.OutputConnection != null
                                && m_controller.OutputConnection.ModuleId == module.Id
                                && m_controller.OutputConnection.OutputId == output.Id)
                                background = ColorRgba.Green;

                            return (text, background);
                        },
                        (context, style) =>
                        {
                            btn.Color(style.text);

                        },
                        (styleA, styleB) => styleA.Equals(styleB),
                        0
                    ));
                }
            }

            private void AddDisplays(UiContext uiContext, Row displaysPanel, Module module, Action refresh)
            {
                var displays = module.Prototype.Displays;
                for (int i = displays.Count - 1; i >= 0; i--)
                {
                    var display = displays[i];

                    var text = new ButtonText(new Mafi.Localization.LocStrFormatted($""))
                        .Size((20 * display.Width).px(), 20.px());
                    displaysPanel.Add(text);

                    if (display.DefaultText == "[image]")
                    {
                        displaysPanel.Add(ImageDisplay(uiContext, module, display));
                    }
                    else if (display.DefaultText.StartsWith("[toggle]"))
                    {
                        displaysPanel.Add(ToggleDisplay(uiContext, module, display));
                    }
                    else if (display.DefaultText.StartsWith("[led]"))
                    {
                        displaysPanel.Add(ToggleDisplay_LED(uiContext, module, display, click : false));
                    }
                    else
                    {
                        displaysPanel.Add(TextDisplay(uiContext, module, display));
                    }
                }
            }

            private Button TextDisplay(UiContext uiContext, Module module, ModuleConnectorProto display)
            {
                var text = new ButtonText(new Mafi.Localization.LocStrFormatted(module.Display[display.Id, display.DefaultText]));
                text.Color(ColorRgba.White);
                text.Size((20 * display.Width).px(), 20.px());

                m_controller.m_updaters.Add(new DataUpdater<
                        string,
                        (Module module, ButtonText text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[c.display.Id, c.display.DefaultText],
                    setter: (c, t) => c.text.Value(new Mafi.Localization.LocStrFormatted(t)),
                    comparator: string.Equals,
                    context: (module, text, display)
                ));
                return text;
            }

            private Button ImageDisplay(UiContext uiContext, Module module, ModuleConnectorProto display)
            {
                var text = new ButtonIcon(module.Display[display.Id, Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png]);
                text.Color(ColorRgba.White);
                text.Size((20 * display.Width).px(), 20.px());

                m_controller.m_updaters.Add(new DataUpdater<
                        string,
                        (Module module, ButtonIcon text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[c.display.Id, Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png],
                    setter: (c, t) => c.text.Icon.Value(t),
                    comparator: string.Equals,
                    context: (module, text, display)
                ));
                return text;
            }

            private Button ToggleDisplay(UiContext uiContext, Module module, ModuleConnectorProto display)
            {
                char separator = display.DefaultText["[toggle]".Length];
                string[] options = display.DefaultText.Replace($"[toggle]{separator}", "").Split(separator);

                if (options.Length == 1 && options[0].Length == 0)
                {
                    return ToggleDisplay_LED(uiContext, module, display);
                }
                else if (options.Length == 1)
                {
                    return ToggleDisplay_Symbol(uiContext, module, display, options[0]);
                }
                else if (options.Length == 2)
                {
                    return ToggleDisplay_DoubleText(uiContext, module, display, options);
                }
                else
                {
                    var text = new ButtonText(new Mafi.Localization.LocStrFormatted(module.Display[display.Id, display.DefaultText]));
                    text.Color(ColorRgba.White);
                    text.Size((20 * display.Width).px(), 20.px());

                    m_controller.m_updaters.Add(new DataUpdater<
                            string,
                            (Module module, ButtonText text, ModuleConnectorProto display)
                        >(
                        getter: (c) => c.module.Display[c.display.Id, c.display.DefaultText],
                        setter: (c, t) => c.text.Value(new Mafi.Localization.LocStrFormatted(t)),
                        comparator: string.Equals,
                        context: (module, text, display)
                    ));
                    return text;
                }
            }

            private Button ToggleDisplay_DoubleText(UiContext uiContext, Module module, ModuleConnectorProto display, string[] options)
            {
                var text = new ButtonText(new Mafi.Localization.LocStrFormatted(module.Display[display.Id, options[0]]));
                text.Color(ColorRgba.White);
                text.Size((20 * display.Width).px(), 20.px());

                m_controller.m_updaters.Add(new DataUpdater<
                        string,
                        (Module module, ButtonText text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[display.Id, options[0]],
                    setter: (c, t) => c.text.Value(new Mafi.Localization.LocStrFormatted(t)),
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
                return text;
            }

            private Button ToggleDisplay_Symbol(UiContext uiContext, Module module, ModuleConnectorProto display, string symbol, bool click = true)
            {
                ButtonText text = new ButtonText(new Mafi.Localization.LocStrFormatted(symbol));
                text.Size((20 * display.Width).px(), 20.px());

                //BtnStyle defaultStyle = click
                //    ? builder.Style.Global.GeneralBtnActive
                //    : builder.Style.Global.ImageBtn.Extend(border: BorderStyle.DEFAULT);

                if (module.Display[display.Id, ""].Length > 0)
                    text.Color(ColorRgba.Green);
                else
                    text.Color(ColorRgba.Red);

                m_controller.m_updaters.Add(new DataUpdater<
                        ColorRgba,
                        (Module module, ButtonText text, ModuleConnectorProto display)
                    >(
                    getter: (c) => c.module.Display[display.Id, ""].Length > 0 ? ColorRgba.Green : ColorRgba.Red,
                    setter: (c, t) => c.text.Color(t),
                    comparator: (a, b) => a == b,
                    context: (module, text, display)
                ));

                if (click)
                {
                    text.OnClick(() =>
                    {
                        if (module.Display[display.Id, ""].Length > 0)
                            module.Display[display.Id] = "";
                        else
                            module.Display[display.Id] = "1";
                    });
                }
                return text;
            }

            private Button ToggleDisplay_LED(UiContext uiContext, Module module, ModuleConnectorProto display, bool click = true)
            {
                return ToggleDisplay_Symbol(uiContext, module, display, "●", click);
            }
        }

        private void AddPreviewHighlight(Module module)
        {
            m_controller.AddPreviewHighlight(module);
        }

        private void ClearPreviewHighlight()
        {
            m_controller.ClearPreviewHighlight();
        }
    }
}
