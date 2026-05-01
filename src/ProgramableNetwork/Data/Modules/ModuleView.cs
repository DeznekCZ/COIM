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
							() => {
								m_controller.m_controller.HoveredModuleGraphic = module;
								m_controller.AddPreviewHighlight(module);
							},
							() => {
								if (m_controller.m_controller.HoveredModuleGraphic == module) {
									m_controller.m_controller.HoveredModuleGraphic = null;
								}
								m_controller.ClearPreviewHighlight();
							}
						);
					fieldsPanel.OnClick(() =>
					{
						var inspector = m_controller.m_controller;
						switch (inspector.Mode)
						{
							case ControllerEditMode.Add:
								// Adopt this module's prototype + data as the "last created"
								// template so shift-click on a free slot stamps a copy.
								ControllerView.m_lastCreated = module;
								break;
							case ControllerEditMode.Move:
								// Click toggles pick-up. A second click on the same module cancels.
								if (inspector.PickedUpModule == null)
								{
									inspector.PickedUpModule = module;
								}
								else if (inspector.PickedUpModule.Id == module.Id)
								{
									inspector.PickedUpModule = null;
								}
								else
								{
									// Target slot is occupied — can't drop here.
									uiContext.AudioDb.InvalidOp(true).Play();
								}
								break;
							case ControllerEditMode.Edit:
							default:
								new ModuleEditDialog(module, m_controller, uiContext, fieldsPanel, inspector);
								break;
						}
					});
					// Right-click in Add mode removes the module — pairs with left-click
					// (which copies it as the "last created" template).
					fieldsPanel.OnRightClick(() =>
					{
						if (m_controller.m_controller.Mode == ControllerEditMode.Add)
						{
							m_controller.RemoveModule(module);
						}
					});

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

					// Cable-style highlight when this module is hovered from the connections panel,
					// or when it has been picked up in Move mode.
					fieldsPanel.Observe(() => m_controller.m_controller.HighlightedFromSidePanel)
						.Observe(() => m_controller.m_controller.PickedUpModule)
						.Do((highlighted, picked) =>
						{
							bool isHovered = highlighted != null && highlighted.Id == module.Id;
							bool isPicked = picked != null && picked.Id == module.Id;
							ColorRgba color;
							if (isPicked) {
								color = ColorRgba.Gold;
							} else if (isHovered) {
								color = ColorRgba.CornflowerBlue;
							} else {
								color = ColorRgba.CornflowerBlue.SetA(0);
							}
							fieldsPanel.Border(all: 2.px(), radius: 4, color: color);
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

					PortPinButton btn = new PortPinButton(PortPinButton.PortKind.Input, isConnected)
						.Tooltip(new LocStrFormatted((input.Name.Name + ": " + input.Name.DescShort).TrimEnd(':', ' ')));
					// Paint the dot with the matching cable's hue so the user can
					// trace which output this input is wired to at a glance.
					if (isConnected)
					{
						var cableColor = m_controller.GetCableColor(module, input.Id, isInput: true);
						if (cableColor.HasValue) {
							btn.DotColor(cableColor.Value);
						}
					}
					inputsPanel.Add(btn);

					if (!preview)
					{
						btn .OnRightClick(() =>
							{
								if (module.InputModules.ContainsKey(input.Id))
								{
									// Wait for the disconnect to actually apply before refreshing
									// the UI — otherwise the redraw runs against stale state.
									uiContext.InputScheduler.ScheduleAndOnApplied(
										new ModuleSetInputConnectionCmd(
											module.Controller.Id, module.Id, input.Id, 0L, ""),
										btn, refresh);
								}
								else
								{
									uiContext.AudioDb.InvalidOp(true).Play();
								}
							})
							.OnClick(() =>
							{
								var inspector = m_controller.m_controller;
								if (inspector.OutputConnection == null)
								{
									uiContext.AudioDb.InvalidOp(true).Play();
									return;
								}
								ModuleConnector held = inspector.OutputConnection;
								// Left-click on an input that's ALREADY carrying the held
								// cable disconnects it — same shape as right-click but
								// without putting the held output down, so the user can
								// keep rerouting without an extra click.
								if (module.InputModules.TryGetValue(input.Id, out var existing)
									&& existing.Equals(held))
								{
									uiContext.InputScheduler.ScheduleAndOnApplied(
										new ModuleSetInputConnectionCmd(
											module.Controller.Id, module.Id, input.Id, 0L, ""),
										btn, refresh);
									return;
								}
								// Otherwise attach (overwrites any other prior source).
								uiContext.InputScheduler.ScheduleAndOnApplied(
									new ModuleSetInputConnectionCmd(
										module.Controller.Id, module.Id, input.Id, held.ModuleId, held.OutputId),
									btn, refresh);
							})
							// Inputs are "open" (enlarged circle) while an output is picked,
							// signalling that a click on this pin would complete the connection.
							// Outputs never enter the open state — they stay closed and look
							// identical to idle inputs.
							.Observe(() => m_controller.m_controller.OutputConnection != null)
							.Do(open =>
							{
								btn.Open(open);
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

					PortPinButton btn = new PortPinButton(PortPinButton.PortKind.Output, isConnected)
						.With(b => b.ObserveEnabled(() => m_controller.m_controller.OutputConnection == null
													   || (m_controller.m_controller.OutputConnection.ModuleId == module.Id
														&& m_controller.m_controller.OutputConnection.OutputId == output.Id)))
						.Tooltip(new LocStrFormatted((output.Name.Name + ": " + output.Name.DescShort).TrimEnd(':', ' ')));
					// Same hue as the cable(s) leaving this output — every connection from
					// one output shares a single palette index, so any one wins the lookup.
					if (isConnected)
					{
						var cableColor = m_controller.GetCableColor(module, output.Id, isInput: false);
						if (cableColor.HasValue) {
							btn.DotColor(cableColor.Value);
						}
					}

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

								// Disconnect the first input that consumes this output. Routed through
								// a command so multiplayer hosts/clients agree, and refresh waits for
								// the cmd to actually apply.
								foreach (var target in m_controller.Entity.Modules)
								{
									foreach (var connection in target.InputModules)
									{
										if (connection.Value.ModuleId == module.Id)
										{
											uiContext.InputScheduler.ScheduleAndOnApplied(
												new ModuleSetInputConnectionCmd(
													target.Controller.Id, target.Id, connection.Key, 0L, ""),
												btn, refresh);
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
					}
				}
			}

			private void AddDisplays(UiContext uiContext, Row displaysPanel, Module module, bool preview, Action refresh) {
				var displays = module.Prototype.Displays;
				foreach (ModuleConnectorProto display in displays) {
					if (display.DefaultText.StartsWith("[image]"))
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
					else if (display.DefaultText.StartsWith("[slider]"))
					{
						displaysPanel.Add(SliderDisplay(uiContext, module, display, preview));
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
				string defaultIcon = display.DefaultText.Length > "[image]".Length
					? display.DefaultText.Substring("[image]".Length)
					: UserInterface.General.Empty128_png;
				var text = new DisplayWithIcon(StatusText(module.Display[display.Id, defaultIcon], out DisplayState? state, out ColorRgba? color));
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
				text.Observe(() => module.Display[display.Id, defaultIcon])
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

			// TODO(displays): revisit the slider/display abstraction — the display
			// definition needs richer fields (similar to inputs/outputs/cable info) so a
			// single proto entry carries enough to render and bind data without parsing
			// the DefaultText string.  Current implementation works for the slider but is
			// the minimum-viable shape; deferred for later.
			//
			// Slider display.  Value lives on module.Display[id] and the bounds on
			// module.Display[id + "_min"] / module.Display[id + "_max"], so a Python
			// action() can drive everything by writing strings back through the Display
			// dict.  Proto-time defaults are baked into DefaultText as
			// "[slider]:<min>:<max>" by AddDisplaySlider.  Width is display.Width clamped
			// to 1-4 cells.  IgnoreInputPicking makes the slider non-interactive — it's a
			// display, not an input.
			private UiComponent SliderDisplay(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
			{
				// Parse the proto-time defaults out of DefaultText.
				float defaultMin = 0f;
				float defaultMax = 1f;
				if (display.DefaultText.Length > "[slider]".Length)
				{
					string body = display.DefaultText.Substring("[slider]".Length);
					if (body.Length > 0)
					{
						char sep = body[0];
						string[] parts = body.Substring(1).Split(sep);
						if (parts.Length >= 1 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float pmin)) {
							defaultMin = pmin;
						}
						if (parts.Length >= 2 && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float pmax)) {
							defaultMax = pmax;
						}
					}
				}

				string keyMin = display.Id + "_min";
				string keyMax = display.Id + "_max";

				float resolveMin()
				{
					string s = module.Display[keyMin, ""];
					return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : defaultMin;
				}
				float resolveMax()
				{
					string s = module.Display[keyMax, ""];
					return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : defaultMax;
				}

				int blockWidth = Math.Max(1, Math.Min(4, display.Width.ToFloat().RoundToInt()));
				var slider = new Mafi.Unity.UiToolkit.Library.Slider();
				slider.Size(Sizes.BLOCK_SIZE * blockWidth, Sizes.BLOCK_SIZE);
				slider.IgnoreInputPicking(); // read-only display, not an input

				slider.Observe(() => module.Display[display.Id, "0"])
					  .Observe(resolveMin)
					  .Observe(resolveMax)
					  .Do((valStr, min, max) =>
					  {
						  if (max <= min) {
							max = min + 1f; // safety: avoid divide-by-zero in Slider.Value
						}
						slider.Range(min, max);
						  if (float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
						  {
							  slider.Value(v);
						  }
						  else
						  {
							  slider.Value(min);
						  }
					  });
				return slider;
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

			// remove module (positions live on the module, no separate grid to clear)
			Entity.Modules.RemoveFirst(m => m.Id == module.Id);

			// Remove connections that referenced the deleted module
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

		// New move logic operating directly on Module.Row / Module.Column.
		// Replaces the broken cache/Rows-based version.
		public bool CanMove(Module module, int x = 0, int y = 0)
		{
			if (module == null || module.Controller == null || Entity == null) {
				return false;
			}
			if (module.Controller.Id != Entity.Id) {
				return false;
			}

			int targetRow = module.Row + y;
			int targetCol = module.Column + x;
			int width = module.Layout.GetWidth(module);
			return IsRangeFree(targetRow, targetCol, width, ignore: module);
		}

		public void Move(Module module, int x = 0, int y = 0)
		{
			if (!CanMove(module, x, y)) {
				return;
			}
			module.Row += y;
			module.Column += x;
			RedrawComponents();
		}

		// Drop a module at an explicit (row, col). Returns true on success.
		public bool TryMoveTo(Module module, int targetRow, int targetCol)
		{
			if (module == null || module.Controller == null || Entity == null) {
				return false;
			}
			if (module.Controller.Id != Entity.Id) {
				return false;
			}

			int width = module.Layout.GetWidth(module);
			if (!IsRangeFree(targetRow, targetCol, width, ignore: module))
			{
				m_controller.Context.AudioDb.InvalidOp(true).Play();
				return false;
			}
			module.Row = targetRow;
			module.Column = targetCol;
			RedrawComponents();
			return true;
		}
	}
}
