using Mafi.Core.Syncers;
using System.Linq;
using Mafi;
using System;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.Ui.Library;
using Mafi.Localization;
using System.Collections.Generic;
using static Mafi.Unity.Assets.Unity;
using System.Globalization;
using Mafi.Core.Research;

namespace ProgramableNetwork.Ui
{
	public partial class ControllerView
	{
		public class ModuleView : Panel
		{
			private readonly Module m_module;
			private readonly ControllerView m_controller;

			public Module Module => m_module;

			public ModuleView(Module module, ControllerView controllerView, UiContext uiContext, bool preview, Action refresh)
				: base()
			{
				this.Margin(Px.Zero);
				this.Body.Padding(Px.Zero);
				this.Body.Margin(Px.Zero);
				this.Body.Gap(Px.Zero);

				this.m_module = module;
				this.m_controller = controllerView;
				string name = "moduleView_" + module.Id;
				var updater = UpdaterBuilder.Start();
				int width = module.Layout.GetWidth(module);
				bool displaysExists = module.Prototype.Displays.Count > 0;

				this.Size(width * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 4);
				this.Class(Cls.panel);

				// Add Input panel
				Row inputsPanel = new Row()
					.Size(width * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
					.Background(ColorRgba.DarkGreen)
					.AlignItemsEnd();
				AddInputs(uiContext, inputsPanel, module, preview, refresh);
				BodyAdd(inputsPanel);

				// Add Field panel
				ButtonText fieldsPanel = new ButtonText(module.Prototype.Symbol.AsLoc());
				fieldsPanel.TextOverflow(TextOverflow.Clip);
				fieldsPanel.TextAlign(TextAlignment.CenterMiddle);
				fieldsPanel.Size(width * Sizes.BLOCK_SIZE, displaysExists ? Sizes.BLOCK_SIZE : (Sizes.BLOCK_SIZE * 2));
				if (!preview)
				{
					fieldsPanel.OnMouseEnterLeave(
							() => m_controller.AddPreviewHighlight(module),
							() => m_controller.ClearPreviewHighlight()
						);
					fieldsPanel.OnClick(() => new ModuleEditDialog(module, m_controller, uiContext, fieldsPanel, m_controller.m_controller));

					this.Observe(() => module.Status)
						.Observe(() => module.Error)
						.Observe(() => module.Warning)
						.Do((status, text, warn) =>
						{
							bool isError = status == ModuleStatus.Error;
							fieldsPanel.Tooltip(text.AsLoc(), enabled: !string.IsNullOrEmpty(text), isError: isError);

							if (status == ModuleStatus.Running) {
								fieldsPanel.Class(Cls.btn_general);
								fieldsPanel.ClassRemove(Cls.btn_primary);
							} else {
								fieldsPanel.Class(Cls.btn_primary);
								fieldsPanel.ClassRemove(Cls.btn_general);
							}
						});

					if (displaysExists)
					{
						this.Observe(() => DateTime.Now)
							.Do((time) => module.Prototype.DisplayUpdate(module));
					}
				}
				BodyAdd(fieldsPanel);

				if (displaysExists) {
					Row displaysPanel = new Row()
						.Size((width * Sizes.BLOCK_SIZE), Sizes.BLOCK_SIZE)
						.Background(ColorRgba.DarkDarkGray);
					AddDisplays(uiContext, displaysPanel, module, preview, refresh);

					BodyAdd(displaysPanel);

					// Add display synchronization every 200 ms
					Element.schedule
						.Execute(() => module.Prototype.DisplayUpdate(module))
						.Every(100);
				}

				// Add Ouptut panel
				Row outputsPanel = new Row()
					.Class(Cls.group)
					.Size(width * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
					.Background(ColorRgba.DarkRed)
					.AlignItemsEnd();
				AddOutputs(uiContext, outputsPanel, module, preview, refresh);

				BodyAdd(outputsPanel);
			}

			private void AddInputs(UiContext uiContext, Row inputsPanel, Module module, bool preview, Action refresh)
			{
				var inputs = module.Prototype.Inputs;
				if (module.Layout.GetWidth(module) - inputs.Count > 0)
				{
					inputsPanel.AddAndReturn(new UiComponent())
						.Width((module.Layout.GetWidth(module) - inputs.Count) * Sizes.BLOCK_SIZE)
						.Height(Sizes.BLOCK_SIZE);
				}
				for (int i = 0; i < inputs.Count; i++)
				{
					var input = inputs[i];
					bool isConnected = module.InputModules.ContainsKey(input.Id);

					ButtonText btn = new ButtonText(new LocStrFormatted(isConnected ? "◎" : "○"))
						.Background(ColorRgba.Green)
						.Color(ColorRgba.Gold)
						.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
						.Tooltip(new LocStrFormatted((input.Name.Name + ": " + input.Name.DescShort).TrimEnd(':', ' ')));
					inputsPanel.Add(btn);

					if (!preview)
					{
						btn .OnRightClick(() =>
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
								if (m_controller.m_controller.OutputConnection == null)
								{
									uiContext.AudioDb.InvalidOp(true).Play();
								}
								else
								{
									module.InputModules[input.Id] = m_controller.m_controller.OutputConnection;
									refresh();
								}
							})
							.Observe(() =>
							{
								var text = ColorRgba.Gold;
								var background = ColorRgba.DarkGreen;

								if (isConnected && m_controller.m_controller.OutputConnection != null &&
									module.InputModules
										.Where(pair => pair.Key == input.Id)
										.Select(pair => pair.Value)
										.Any(connector => connector.Equals(m_controller.m_controller.OutputConnection)))
								{
									text = ColorRgba.White;
									background = ColorRgba.DarkGreen;
								}

								else if (isConnected && m_controller.m_controller.m_higlightedOutput != null && m_controller.m_controller.OutputConnection == null &&
									module.InputModules
										.Where(pair => pair.Key == input.Id)
										.Select(pair => pair.Value)
										.Any(connector => connector.Equals(m_controller.m_controller.m_higlightedOutput)))
								{
									text = ColorRgba.White;
									background = ColorRgba.DarkGreen;
								}

								return (text, background);
							})
							.Do(pair => 
							{
								btn.Color(pair.text);
							});

						btn.OnMouseEnterLeave(
							() => { m_controller.m_controller.m_higlightedInput = new ModuleConnector(module.Id, input.Id); },
							() => { m_controller.m_controller.m_higlightedInput = null; }
						);
					}
				}
			}

			private void AddOutputs(UiContext uiContext, Row inputsPanel, Module module, bool preview, Action refresh)
			{
				var outputs = module.Prototype.Outputs;
				if (module.Layout.GetWidth(module) - outputs.Count > 0)
				{
					inputsPanel.AddAndReturn(new UiComponent())
						.Width((module.Layout.GetWidth(module) - outputs.Count) * Sizes.BLOCK_SIZE)
						.Height(Sizes.BLOCK_SIZE);
				}
				for (int i = 0; i < outputs.Count; i++)
				{
					var output = outputs[i];
					bool isConnected = module.Controller.Modules
						.AsEnumerable()
						.Where(m => m.InputModules.Count > 0)
						.SelectMany(m => m.InputModules)
						.Select(p => p.Value)
						.FirstOrDefault(c => c.ModuleId == module.Id
										  && c.OutputId == output.Id) != null;

					ButtonText btn = new ButtonText(new LocStrFormatted(isConnected ? "◎" : "○"))
						.Background(ColorRgba.Red)
						.Color(ColorRgba.Gold)
						.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
						.With(b => b.ObserveEnabled(() => m_controller.m_controller.OutputConnection == null
													   || (m_controller.m_controller.OutputConnection.ModuleId == module.Id
														&& m_controller.m_controller.OutputConnection.OutputId == output.Id)))
						.Tooltip(new LocStrFormatted((output.Name.Name + ": " + output.Name.DescShort).TrimEnd(':', ' ')));


					inputsPanel.Add(btn);

					if (!preview)
					{
						btn .OnRightClick(() =>
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
								if (m_controller.m_controller.OutputConnection != null
									&& m_controller.m_controller.OutputConnection.ModuleId == module.Id
									&& m_controller.m_controller.OutputConnection.OutputId == output.Id)
								{
									m_controller.m_controller.OutputConnection = null;
								}
								else
								{
									m_controller.m_controller.OutputConnection = new ModuleConnector(module.Id, output.Id);
								}
							});
							btn.OnMouseEnterLeave(
								() => { m_controller.m_controller.m_higlightedOutput = new ModuleConnector(module.Id, output.Id); },
								() => { m_controller.m_controller.m_higlightedOutput = null; }
							);

						btn .Observe(() =>
							{
								var text = ColorRgba.Gold;
								var background = ColorRgba.DarkGreen;

								if (m_controller.m_controller.OutputConnection != null
									&& m_controller.m_controller.OutputConnection.ModuleId == module.Id
									&& m_controller.m_controller.OutputConnection.OutputId == output.Id) {
									background = ColorRgba.Green;
								}

								return (text, background);
							})
							.Do(pairs =>
							{
								btn.Color(pairs.text);
							});
					}
				}
			}

			private void AddDisplays(UiContext uiContext, Row displaysPanel, Module module, bool preview, Action refresh) {
				var displays = module.Prototype.Displays;
				foreach (ModuleConnectorProto display in displays) {
					if (display.DefaultText == "[image]")
					{
						displaysPanel.Add(ImageDisplay(uiContext, module, display));
					}
					else if (display.DefaultText.StartsWith("[toggle]"))
					{
						displaysPanel.Add(ToggleDisplay(uiContext, module, display, preview));
					}
					else if (display.DefaultText.StartsWith("[led]"))
					{
						displaysPanel.Add(ToggleDisplay_LED(uiContext, module, display, preview, click : false));
					}
					else if (display.DefaultText.StartsWith("[fill]"))
					{
						if (display.Width > 0) {
							displaysPanel.Add(new Display().StateInactive().Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE));
						}
					}
					else
					{
						displaysPanel.Add(TextDisplay(uiContext, module, display, preview));
					}
				}
			}

			private UiComponent TextDisplay(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
			{
				var text = new StatusDisplay();
				text.Class(Cls.displayFont);
				text.PaddingLeftRight(6.px());
				text.Value(StatusText(module.Display[display.Id, display.DefaultText], out DisplayState? state, out ColorRgba? color).AsLoc());
				if (state.HasValue) {
					text.State(state ?? DisplayState.Neutral);
				}
				if (color.HasValue) {
					text.TextColor(color);
				}
				text.TextOverflow(TextOverflow.Clip);
				text.TextAlign(TextAlignment.RightMiddle);
				text.Color(ColorRgba.White);
				text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
				text.Observe(() => module.Display[display.Id, display.DefaultText])
					.Do((t) => {
						text.Value(StatusText(t, out DisplayState? stateN, out ColorRgba? colorN).AsLoc());
						if (stateN.HasValue) {
							text.State(stateN ?? DisplayState.Neutral);
						}
						if (colorN.HasValue) {
							text.TextColor(colorN);
						}
					});
				return text;
			}

			private string StatusText(string text, out DisplayState? state, out ColorRgba? color)
			{
				state = null;
				color = null;

				if (text.IsNullOrEmpty()) {
					return "";
				}

				while (text.StartsWith("#"))
				{
					switch (text[1])
					{
						case 'E':
							state = DisplayState.Danger;
							text = text.Substring(2);
							break;
						case 'W':
							state = DisplayState.Warning;
							text = text.Substring(2);
							break;
						case 'I':
							state = DisplayState.Inactive;
							text = text.Substring(2);
							break;
						case 'P':
							state = DisplayState.Positive;
							text = text.Substring(2);
							break;
						case 'C':
							int r = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
							int g = int.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
							int b = int.Parse(text.Substring(6, 2), NumberStyles.HexNumber);

							color = new ColorRgba(r, g, b);

							text = text.Substring(8);
							break;
						default:
							throw new NotImplementedException($"Missing type of format: {text[1]}!");
					}
				}
				return text;
			}

			private UiComponent ImageDisplay(UiContext uiContext, Module module, ModuleConnectorProto display)
			{
				var text = new DisplayWithIcon(StatusText(module.Display[display.Id, UserInterface.General.Empty128_png], out DisplayState? state, out ColorRgba? color));
				if (state.HasValue) {
					text.State(state ?? DisplayState.Neutral);
				}
				if (color.HasValue) {
					text.Icon.Color(color);
				}
				text.Icon.Margin(Px.Zero);
				text.Icon.Padding(Px.Zero);
				text.Icon.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
				text.Color(ColorRgba.White);
				text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
				text.Observe(() => module.Display[display.Id, UserInterface.General.Empty128_png])
					.Do((t) =>
					{
						text.Icon.Value(StatusText(t, out DisplayState? newState, out ColorRgba? newColor));
						if (newState.HasValue) {
							text.State(newState ?? DisplayState.Neutral);
						}
						if (newColor.HasValue) {
							text.Icon.Color(newColor);
						}
					});
				return text;
			}

			private UiComponent ToggleDisplay(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
			{
				char separator = display.DefaultText["[toggle]".Length];
				string[] options = display.DefaultText.Replace($"[toggle]{separator}", "").Split(separator);

				if (options.Length == 1 && options[0].Length == 0)
				{
					return ToggleDisplay_LED(uiContext, module, display, preview);
				}
				else if (options.Length == 1)
				{
					return ToggleDisplay_Symbol(uiContext, module, display, preview, options[0]);
				}
				else if (options.Length == 2)
				{
					return ToggleDisplay_DoubleText(uiContext, module, display, preview, options);
				}
				else
				{
					var text = new ButtonText(new Mafi.Localization.LocStrFormatted(module.Display[display.Id, display.DefaultText]));
					text.Color(ColorRgba.White);
					text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);

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

			private Button ToggleDisplay_DoubleText(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview, string[] options)
			{
				var text = new ButtonText(new LocStrFormatted(module.Display[display.Id, options[0]]));
				text.TextOverflow(TextOverflow.Clip);
				text.TextAlign(TextAlignment.CenterMiddle);
				text.FontSize(10);
				text.Color(ColorRgba.White);
				text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);

				text.Observe(() => module.Display[display.Id, options[0]])
					.Do(t => text.Value(t.AsLoc()));

				if (!preview) {
					text.OnClick(() =>
					{
						if (module.Display[display.Id, options[0]] == options[0]) {
							module.Display[display.Id] = options[1];
						} else {
							module.Display[display.Id] = options[0];
						}
					});
				}
				return text;
			}

			private UiComponent ToggleDisplay_Symbol(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview, string symbol, bool click = true)
			{
				ButtonText text = new ButtonText(new LocStrFormatted(symbol));
				text.TextOverflow(TextOverflow.Clip);
				text.TextAlign(TextAlignment.CenterMiddle);
				text.FontSize(10);
				text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
				text.Color(module.Display[display.Id, ""].Length > 0 ? ColorRgba.Green : ColorRgba.Red);
				text.Observe(() => module.Display[display.Id, ""].Length > 0 ? ColorRgba.Green : ColorRgba.Red)
					.Do((color) => text.Color(color));

				if (click && !preview)
				{
					text.OnClick(() =>
					{
						if (module.Display[display.Id, ""].Length > 0) {
							module.Display[display.Id] = "";
						} else {
							module.Display[display.Id] = "1";
						}
					});
				}
				return text;
			}

			private UiComponent ToggleDisplay_LED(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview, bool click = true)
			{
				if (click) {
					return ToggleDisplay_Symbol(uiContext, module, display, preview, "●", click);
				}

				var text = new DisplayWithIcon(UserInterface.General.Circle_svg);
				string value = StatusText(module.Display[display.Id, ""], out DisplayState? state, out ColorRgba? color);
				text.Icon.Color(color.HasValue ? color : value.Length > 0 ? ColorRgba.Green : ColorRgba.Red);
				text.Icon.Margin(Px.Zero);
				text.Icon.Padding(Px.Zero);
				text.Icon.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
				if (state.HasValue) {
					text.State(state ?? DisplayState.Neutral);
				}
				text.Color(ColorRgba.White);
				text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
				text.Observe(() => (color: StatusText(module.Display[display.Id, ""], out DisplayState? newState, out ColorRgba? userColor).Length > 0 ? ColorRgba.Green : ColorRgba.Red, state: newState, userColor))
					.Do((pair) =>
					{
						if (pair.userColor != null) {
							text.Icon.Color(pair.userColor);
						} else {
							text.Icon.Color(pair.color);
						}

						if (pair.state.HasValue) {
							text.State(pair.state ?? DisplayState.Neutral);
						}
					});
				return text;
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

		public void RemoveModule(Module module)
		{
			// GUARD
			if (module.Controller.Id != Entity.Id) {
				return;
			}

			// remove module
			Entity.Modules.RemoveFirst(m => m.Id == module.Id);

			// remove placements
			for (int i = 0; i < Entity.Rows.Count; i++)
			{
				for (int j = 0; j < Entity.Rows[i].Count; j++)
				{
					if (Entity.Rows[i][j].ModuleId == module.Id)
					{
						Entity.Rows[i][j] = ModulePlacement.Empty;
					}
				}
			}

			// Remove connections
			foreach (Module item in Entity.Modules)
			{
				foreach (KeyValuePair<string, ModuleConnector> input in item.InputModules.ToList())
				{
					if (input.Value.ModuleId == module.Id)
					{
						item.InputModules.Remove(input.Key);
					}
				}
			}

			RedrawComponents();
		}

		public bool CanMove(Module module, int x = 0, int y = 0)
		{
			// GUARD
			if (module.Controller.Id != Entity.Id) {
				return false;
			}

			// Read from placement cache
			if (!ModulePlacementCache.TryGetValue(module.Id, out var placement)) {
				return false;
			}

			if (y > 0 && placement.y + y >= Entity.Prototype.Rows - 1) {
				return false;
			}
			if (y < 0 && placement.y + y <= 0) {
				return false;
			}

			int len = module.Layout.GetWidth(module);
			if (x > 0 && placement.x + y + len >= Entity.Prototype.Columns - 1) {
				return false;
			}
			if (x < 0 && placement.x + y <= 0) {
				return false;
			}

			for (int i = 0; i < len; i++)
			{
				var place = module.Controller.Rows[placement.y + y][placement.x + x + i];
				if (place.ModuleId != 0 && place.ModuleId != module.Id) {
					return false;
				}
			}

			return true;
		}

		public void Move(Module module, int x = 0, int y = 0)
		{
			// MUST BE GUARDED BEFORE
			int width = module.Layout.GetWidth(module);

			(int sourceX, int sourceY) = ModulePlacementCache[module.Id];
			for (int i = sourceX; i < sourceX + width; i++)
			{
				Entity.Rows[sourceY][i] = ModulePlacement.Empty;
			}
			int targetX = sourceX + x;
			int targetY = sourceY + y;
			for (int i = targetX; i < targetX + width; i++)
			{
				Entity.Rows[targetY][i] = targetX == i ? ModulePlacement.Origin(module.Id) : ModulePlacement.Rest(module.Id);
			}
			RedrawComponents();
		}
	}
}
