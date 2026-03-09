using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity;
using static Mafi.Unity.Assets.Unity;
using System;
using UnityEngine;
using System.Collections.Generic;
using ProgramableNetwork;
using Mafi.Collections.ImmutableCollections;
using System.Linq;
using Mafi.Collections;
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;
using Mafi.Unity.Ui;
using RTG;
using Mafi.Core.Entities;
using Mafi.Unity.Ui.Library;

namespace ProgramableNetwork.Ui.DisplayEntity.Displays
{
	public class BasicLightManager : IDisplayEntityInspector
	{
		private PanelRow m_row;

		public BasicLightManager(Data.DisplayEntity.DisplayEntity entity)
		{
			this.Entity = entity;
		}

		public Data.DisplayEntity.DisplayEntity Entity { get; }

		public Action Create(DisplayEntityInspector panel)
		{
			Label active;
			ButtonIcon colorIcon;
			Toggle toggle;
			//Dropdown<LightInfo> dropdown;
			RgbColorPicker colorPicker;
			Slider intensity;
			var components = new UiComponent[] {
				active = new Label("Active".AsLoc()).TextAlign(TextAlignment.LeftMiddle)
				.FlexGrow(0.4f),
				toggle = new Toggle().FlexGrow(0.1f),
				colorPicker = new RgbColorPicker()
					.OnColorChanged((v) => {
						Entity.SetProperty("colorOn.R", v.R);
						Entity.SetProperty("colorOn.G", v.G);
						Entity.SetProperty("colorOn.B", v.B);
						Entity.SetProperty("colorOff.R", v.R.Min(100));
						Entity.SetProperty("colorOff.G", v.G.Min(100));
						Entity.SetProperty("colorOff.B", v.B.Min(100));
					}),
				//dropdown = new Dropdown<LightInfo>(
				//	optionViewFactory: (option, index, isInDropdown) => new Icon(UserInterface.General.Circle_svg).Color(option.icon),
				//	customButton: colorIcon = new ButtonIcon(UserInterface.General.Circle_svg)
				//)
				//.OnValueChanged((v, i) => {
				//	Entity.SetProperty("colorOn.R", v.on.R);
				//	Entity.SetProperty("colorOn.G", v.on.G);
				//	Entity.SetProperty("colorOn.B", v.on.B);
				//	Entity.SetProperty("colorOff.R", v.off.R);
				//	Entity.SetProperty("colorOff.G", v.off.G);
				//	Entity.SetProperty("colorOff.B", v.off.B);
				//})
				//.SetOptions(Colors().ToImmutableArray())
				//.FlexGrow(0.5f)
			};

			m_row = panel.AddPanelRow(components);

			m_row.Observe(() => Entity.IsActive)
			   .Do((lightIsOn) => {
				   toggle.Value(lightIsOn);
				   if (lightIsOn) {
						panel.Status.AsWorking();
					} else {
						panel.Status.AsIdle();
					}
				});

			m_row
				.Observe(() => new ColorRgba(
					Entity.GetProperty("colorOn.R", ColorRgba.Red.R).IntegerPart,
					Entity.GetProperty("colorOn.G", ColorRgba.Red.G).IntegerPart,
					Entity.GetProperty("colorOn.B", ColorRgba.Red.B).IntegerPart
				))
			 //  .Observe(() => new ColorRgba(
				//	Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(100).R).IntegerPart,
				//	Entity.GetProperty("colorOff.G", ColorRgba.Red.G).IntegerPart,
				//	Entity.GetProperty("colorOff.B", ColorRgba.Red.B).IntegerPart
				//))
				.Do((colorOn/*, colorOff*/) => {
					colorPicker.Value(colorOn);
					//	for (var i = 0; i < dropdown.OptionsCount; i++)
					//	{
					//	   var option = dropdown.GetOptionAt(i);
					//	   if (option.on == colorOn && option.off == colorOff)
					//	   {
					//		   //dropdown.SetValueIndex(i);
					//		   colorIcon.Icon.Color(option.icon);
					//		   return;
					//	   }
					//	}
					//dropdown.SetValueIndex(0);
					//colorIcon.Icon.Color(dropdown.GetOptionAt(0).icon);
				});

			toggle.OnValueChanged((playing) => Entity.SetActive(playing));

			//InitColorSelection(colorIcon, dropdown);

			return () => m_row.RemoveFromHierarchy();
		}

		private void InitColorSelection(ButtonIcon colorIcon, Dropdown<LightInfo> dropdown)
		{
			var colorOn = new ColorRgba(
				Entity.GetProperty("colorOn.R", ColorRgba.Red.R).IntegerPart,
				Entity.GetProperty("colorOn.G", ColorRgba.Red.G).IntegerPart,
				Entity.GetProperty("colorOn.B", ColorRgba.Red.B).IntegerPart
			);
			var colorOff = new ColorRgba(
				Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(63).R).IntegerPart,
				Entity.GetProperty("colorOff.G", ColorRgba.Red.G).IntegerPart,
				Entity.GetProperty("colorOff.B", ColorRgba.Red.B).IntegerPart
			);
			for (var i = 0; i < dropdown.OptionsCount; i++)
			{
				var option = dropdown.GetOptionAt(i);
				//Log.Info($"At {i} OFF: {option.on.ToHex()} is {colorOn.ToHex()}");
				//Log.Info($"At {i}  ON: {option.off.ToHex()} is {colorOff.ToHex()}");
				if (option.on == colorOn && option.off == colorOff)
				{
					dropdown.SetValueIndex(i);
					colorIcon.Icon.Color(option.icon);
					break;
				}
			}
		}

		public static IEnumerable<LightInfo> Colors()
		{
			yield return new LightInfo
			{
				on = ColorRgba.Red,
				off = ColorRgba.Red.SetR(100),
				icon = ColorRgba.Red
			};
			yield return new LightInfo
			{
				on = ColorRgba.Yellow,
				off = ColorRgba.Yellow.SetR(100).SetG(100),
				icon = ColorRgba.Yellow
			};
			yield return new LightInfo
			{
				on = ColorRgba.Green,
				off = ColorRgba.Green.SetG(100),
				icon = ColorRgba.Green
			};
			yield return new LightInfo
			{
				on = ColorRgba.Blue,
				off = ColorRgba.Blue.SetB(100),
				icon = ColorRgba.Blue
			};
			yield return new LightInfo
			{
				on = ColorRgba.LightGray,
				off = ColorRgba.LightGray.SetR(100).SetG(100).SetB(100),
				icon = ColorRgba.LightGray
			};
		}
	}
}

namespace ProgramableNetwork.Data.DisplayEntity.Displays
{
	public class BasicLightManager : IDisplayEntityManager
	{
		private static readonly int COLOR = Shader.PropertyToID("_Color");
		private static readonly int EMISSION_COLOR = Shader.PropertyToID("_EmissionColor");
		private Renderer m_render;

		public BasicLightManager(DisplayEntity disp)
		{
			Entity = disp;
			Proto = Entity.Prototype;
			Inspector = new Ui.DisplayEntity.Displays.BasicLightManager(disp);
		}

		public DisplayEntity Entity { get; }

		public DisplayEntityProto Proto { get; }
		public Ui.DisplayEntity.IDisplayEntityInspector Inspector { get; }
		public DisplayEntityMb Mb { get; private set; }

		private bool m_lightIsActive = false;

		private Color colorOn;
		private Color colorOff;
		private Lyst<string> m_changedColors;

		public void Init(DisplayEntityMb mb)
		{
			Mb = mb;

			if (!mb.gameObject.TryFindChild("light", out var light)) {
				throw new NullReferenceException("missing 'light' object");
			}

			m_render = light.GetComponent<Renderer>();
			if (m_render is null) {
				throw new NullReferenceException("missing renderer on 'light' object");
			}

			m_lightIsActive = Entity.IsEnabled && Entity.IsActive && !Entity.ElectricityConsumer.Value.NotEnoughPower;
			m_changedColors = new Lyst<string>();

			applyColors(true, true);
		}

		public void RenderUpdate(GameTime time)
		{
			// Nothing to do
		}

		public void SyncUpdate(GameTime time)
		{
			if (time.IsGamePaused) {
				return;
			}

			m_changedColors.Clear();

			bool lightIsActive = Entity.IsEnabled && Entity.IsActive
				&& !Entity.ElectricityConsumer.Value.NotEnoughPower;
			bool lightSwitched = m_lightIsActive != lightIsActive;
			m_lightIsActive = lightIsActive;

			bool colorChanged = Entity.Sync(ref m_changedColors);
			applyColors(lightSwitched, colorChanged);
		}

		private void applyColors(bool lightSwitched, bool colorChanged)
		{
			if (colorChanged) {
				colorOn = new Color(
						Entity.GetProperty("colorOn.R", ColorRgba.Red.R).ToFloat() / 127,
						Entity.GetProperty("colorOn.G", ColorRgba.Red.G).ToFloat() / 127,
						Entity.GetProperty("colorOn.B", ColorRgba.Red.B).ToFloat() / 127
					);
				colorOff = new Color(
						Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(100).R).ToFloat() / 255,
						Entity.GetProperty("colorOff.G", ColorRgba.Red.G).ToFloat() / 255,
						Entity.GetProperty("colorOff.B", ColorRgba.Red.B).ToFloat() / 255
					);

				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				materialPropertyBlock.SetColor(COLOR, colorOff);
				materialPropertyBlock.SetColor(EMISSION_COLOR, colorOn);
				m_render.SetPropertyBlock(materialPropertyBlock);
			}

			if (lightSwitched) {
				if (m_lightIsActive) {
					m_render.material.EnableKeyword("_EMISSION");
				} else {
					m_render.material.DisableKeyword("_EMISSION");
				}
			}
		}
	}
}