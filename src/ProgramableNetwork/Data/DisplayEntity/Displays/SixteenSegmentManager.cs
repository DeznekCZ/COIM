using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using UnityEngine;
using Display = Mafi.Unity.Ui.Library.Display;
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;

namespace ProgramableNetwork.Ui.DisplayEntity.Displays
{
	public class SixteenSegmentManager : ColorizedLightInspector, IDisplayEntityInspector
	{
		public static string[] SEGMENTS = ["A1", "B", "C", "D1", "E", "F", "G1", "A2", "D2", "G2", "H", "I", "J", "K", "L", "M", "DP"];
		private PanelRow m_row;
		private PanelWithHeader m_act;

		public SixteenSegmentManager(Data.DisplayEntity.DisplayEntity entity)
			: base(entity)
		{
		}

		public Action Create(DisplayEntityInspector panel)
		{
			Label active;
			Display number;

			var components = new UiComponent[] {
				active = new Label().LaterText(() => NewTr.Inspector.Color, panel).TextAlign(TextAlignment.LeftMiddle)
				.FlexGrow(0.4f),
				GetColorPickerComponent(),
				number = new Display("000".AsLoc()).Width(1.25f * Sizes.BLOCK_SIZE)
			};

			m_row = panel.AddPanelRow(components);

			m_row.Observe(() => Entity.IsActive)
				.Do((playing) => {
					if (playing) {
						panel.Status.AsWorking();
					} else {
						panel.Status.AsIdle();
					}
				});

			var dict = new List<bool>();
			for (int d = 0; d < SEGMENTS.Length; d++)
			{
				dict.Add(false);
				NewMethod(d);
			}

			number.ObserveEnumerable(() => dict)
				.Do(list =>
				{
					int final = 0;
					for (int i = 0; i < list.Count; i++) {
						if (list[i]) {
							final = final | (1 << i);
						}
					}
					number.Value(final.ToString("D3").AsLoc());
				});

			m_act = panel.AddRootPanelWithHeader();
			m_act.Body.AlignItemsCenterMiddle();
			m_act.Body.Add(new Row(5) { Toggles(panel) });
			m_act.Header.Add(new Label().LaterText(() => NewTr.Inspector.Segments, panel).Class(Cls.panelHeader).TextAlign(TextAlignment.CenterMiddle));

			return () => {
				m_row.RemoveFromHierarchy();
				m_act.RemoveFromHierarchy();
			};

			void NewMethod(int index)
			{
				number.Observe(() => Entity.GetProperty(SEGMENTS[index], Fix32.Zero) > 0)
					  .Do(a => dict[index] = a);
			}
		}

		private IEnumerable<UiComponent> Toggles(DisplayEntityInspector panel)
		{
			foreach (var item in SEGMENTS)
			{
				var toggle = new Toggle();
				var group = new Column(2)
				{
					new Label((item == "DP" ? "Dot" : item).AsLoc())
						.TextAlign(TextAlignment.CenterMiddle)
						.FlexGrow(1),
					toggle
				}.FlexGrow(1);
				NewMethod(panel, toggle, item);

				yield return group;
			}

			void NewMethod(DisplayEntityInspector insp, Toggle toggle, string item)
			{
				toggle.ObserveValue(() => insp.Entity.GetProperty(item, Fix32.Zero) > Fix32.Zero);
				toggle.OnValueChanged(on => {
					insp.Entity.SetProperty(item, on ? Fix32.One : Fix32.Zero);
					insp.Entity.SetActive(true);
				});
			}
		}
	}

}

namespace ProgramableNetwork.Data.DisplayEntity.Displays
{

	public class SixteenSegmentManager : IDisplayEntityManager
	{
		public static string[] SEGMENTS = ["A1", "B", "C", "D1", "E", "F", "G1", "A2", "D2", "G2", "H", "I", "J", "K", "L", "M", "DP"];
		private static readonly int COLOR = Shader.PropertyToID("_Color");
		private static readonly int EMISSION_COLOR = Shader.PropertyToID("_EmissionColor");
		private Dictionary<string, Renderer> m_render;
		private bool m_lightIsActive;
		private Color m_colorOn;
		private Color m_colorOff;
		private Lyst<string> m_changedColors;

		public SixteenSegmentManager(DisplayEntity disp)
		{
			Entity = disp;
			Proto = Entity.Prototype;
			Inspector = new Ui.DisplayEntity.Displays.SixteenSegmentManager(disp);
			m_colorOn = Color.black;
			m_colorOff = Color.black;
		}

		public DisplayEntity Entity { get; }

		public DisplayEntityProto Proto { get; }

		public DisplayEntityMb Mb { get; private set; }

		public Ui.DisplayEntity.IDisplayEntityInspector Inspector { get; }

		public void Init(DisplayEntityMb mb)
		{
			Mb = mb;

			m_render = new Dictionary<string, Renderer>();

			foreach (var item in SEGMENTS)
			{
				m_render[item] = mb.gameObject.TryFindChild(item, out var light) ? light.GetComponent<Renderer>() : throw new NullReferenceException($"missing object '{item}' with render");
				m_render[item].material = new Material(m_render[item].material);
			}
			
			m_lightIsActive = Entity.IsEnabled && Entity.IsActive
				&& !Entity.ElectricityConsumer.Value.NotEnoughPower;
			m_changedColors = ["color"]; // force update of colors and emission on init
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

		private void applyColors(bool lightSwitched, bool colorOrEmissionsChanged)
		{
			if (colorOrEmissionsChanged && m_changedColors.Any(c => c.StartsWith("color"))) {
				m_colorOn = new Color(
						Entity.GetProperty("colorOn.R", ColorRgba.Red.R).ToFloat() / 127,
						Entity.GetProperty("colorOn.G", ColorRgba.Red.G).ToFloat() / 127,
						Entity.GetProperty("colorOn.B", ColorRgba.Red.B).ToFloat() / 127
					);
				m_colorOff = new Color(
						Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(100).R).ToFloat() / 255,
						Entity.GetProperty("colorOff.G", ColorRgba.Red.G).ToFloat() / 255,
						Entity.GetProperty("colorOff.B", ColorRgba.Red.B).ToFloat() / 255
					);

				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				materialPropertyBlock.SetColor(COLOR, m_colorOff);
				materialPropertyBlock.SetColor(EMISSION_COLOR, m_colorOn);

				foreach (var render in m_render.Values) {
					render.SetPropertyBlock(materialPropertyBlock);
				}
			}

			// TODO: optimize by only changing emission keyword on segments that changed
			if (lightSwitched || colorOrEmissionsChanged) {
				if (m_lightIsActive) {
					foreach (KeyValuePair<string, Renderer> render in m_render) {
						if (Entity.GetProperty(render.Key) > 0) {
							render.Value.material.EnableKeyword("_EMISSION");
						} else {
							render.Value.material.DisableKeyword("_EMISSION");
						}
					}
				} else {
					foreach (var render in m_render.Values) {
						render.material.DisableKeyword("_EMISSION");
					}
				}
			}
		}
	}
}