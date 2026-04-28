using System;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;

namespace ProgramableNetwork.Ui.DisplayEntity.Displays
{
	public class BasicLightManager : ColorizedLightInspector, IDisplayEntityInspector
	{
		private PanelRow m_row;

		public BasicLightManager(Data.DisplayEntity.DisplayEntity entity)
			: base(entity) { }

		public Action Create(DisplayEntityInspector panel)
		{
			Label active;
			Toggle toggle;

			var components = new UiComponent[] {
				active = new Label().LaterText(() => NewTr.Inspector.Active, panel).TextAlign(TextAlignment.LeftMiddle)
				.FlexGrow(0.4f),
				toggle = new Toggle().FlexGrow(0.1f),
				GetColorPickerComponent()
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

			toggle.OnValueChanged((playing) => Entity.SetActive(playing));

			//InitColorSelection(colorIcon, dropdown);

			return () => m_row.RemoveFromHierarchy();
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