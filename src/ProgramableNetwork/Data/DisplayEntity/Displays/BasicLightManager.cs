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
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;

namespace ProgramableNetwork.Data.DisplayEntity.Displays
{
    public class BasicLightManager : IDisplayEntityManager
    {
        private Renderer m_render;
        private Color m_colorOn;
        private Color m_colorOff;
        private PanelRow m_row;

        public BasicLightManager(DisplayEntity disp)
        {
            Entity = disp;
            Proto = Entity.Prototype;
        }

        public DisplayEntity Entity { get; }

        public DisplayEntityProto Proto { get; }

        public DisplayEntityMb Mb { get; private set; }

        public void Init(DisplayEntityMb mb)
        {
            Mb = mb;

            if (!mb.gameObject.TryFindChild("light", out var light))
                throw new NullReferenceException("missing 'light' object");

            m_render = light.GetComponent<Renderer>();
            if (m_render is null)
                throw new NullReferenceException("missing renderer on 'light' object");

            ApplyColors();
        }

        public Action Inspector(DisplayEntityInspector panel)
        {
            Label active;
            ButtonIcon colorIcon;
            Toggle toggle;
            Dropdown<LightInfo> dropdown;
            var components = new UiComponent[] {
                active = new Label("Active".AsLoc()).TextAlign(TextAlignment.LeftMiddle)
                .FlexGrow(0.4f),
                toggle = new Toggle().FlexGrow(0.1f),
                dropdown = new Dropdown<LightInfo>(
                    optionViewFactory: (option, index, isInDropdown) => new Icon(UserInterface.General.Circle_svg).Color(option.icon),
                    customButton: colorIcon = new ButtonIcon(UserInterface.General.Circle_svg)
                )
                .OnValueChanged((v, i) => {
                    Entity.SetProperty("colorOn.R", v.on.r.ToFix32());
                    Entity.SetProperty("colorOn.G", v.on.g.ToFix32());
                    Entity.SetProperty("colorOn.B", v.on.b.ToFix32());
                    Entity.SetProperty("colorOff.R", v.off.r.ToFix32());
                    Entity.SetProperty("colorOff.G", v.off.g.ToFix32());
                    Entity.SetProperty("colorOff.B", v.off.b.ToFix32());
                })
                .SetOptions(Colors().ToImmutableArray())
                .FlexGrow(0.5f)
            };

            m_row = panel.AddPanelRow(active, toggle, dropdown);

            m_row.Observe(() => Entity.IsActive)
               .Do((playing) => {
                   toggle.Value(playing);
                   if (playing)
                       panel.Status.AsWorking();
                   else
                       panel.Status.AsIdle();
               });

            m_row.Observe(() => m_colorOn)
               .Observe(() => m_colorOff)
               .Do((colorOn, colorOff) => {
                   for (var i = 0; i < dropdown.OptionsCount; i++)
                   {
                       var option = dropdown.GetOptionAt(i);
                       if (option.on == colorOn && option.off == colorOff)
                       {
                           dropdown.SetValueIndex(i);
                           colorIcon.Icon.Color(Colors().Skip(i).First().icon);
                           return;
                       }
                   }
                   dropdown.SetValueIndex(0);
                   colorIcon.Icon.Color(Colors().First().icon);
               });

            toggle.OnValueChanged((playing) => Entity.SetActive(playing));

            return () => m_row.RemoveFromHierarchy();
        }

        public void RenderUpdate(GameTime time)
        {
            // Nothing to do
        }

        public void SyncUpdate(GameTime time)
        {
            if (time.IsGamePaused) return;

            ApplyColors();
        }

        private void ApplyColors()
        {
            bool lightOn = Entity.IsEnabled && Entity.IsActive && !Entity.ElectricityConsumer.Value.NotEnoughPower;

            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            m_colorOff = new Color(
                Entity.GetProperty("colorOff.R", Fix32.Half).ToFloat(),
                Entity.GetProperty("colorOff.G", Fix32.Zero).ToFloat(),
                Entity.GetProperty("colorOff.B", Fix32.Zero).ToFloat()
            );
            m_colorOn = new Color(
                Entity.GetProperty("colorOn.R", Fix32.One).ToFloat(),
                Entity.GetProperty("colorOn.G", Fix32.Zero).ToFloat(),
                Entity.GetProperty("colorOn.B", Fix32.Zero).ToFloat()
            );
            materialPropertyBlock.SetColor("_Color", m_colorOff);
            materialPropertyBlock.SetColor("_EmissionColor", m_colorOn);
            m_render.SetPropertyBlock(materialPropertyBlock);

            if (lightOn)
                m_render.material.EnableKeyword("_EMISSION");
            else
                m_render.material.DisableKeyword("_EMISSION");
        }

        public static IEnumerable<LightInfo> Colors()
        {
            yield return new LightInfo
            {
                on = Color.red,
                off = Color.Lerp(Color.red, Color.black, 0.5f),
                icon = ColorRgba.Red
            };
            yield return new LightInfo
            {
                on = Color.yellow,
                off = Color.Lerp(Color.yellow, Color.black, 0.5f),
                icon = ColorRgba.Yellow
            };
            yield return new LightInfo
            {
                on = Color.green,
                off = Color.Lerp(Color.green, Color.black, 0.5f),
                icon = ColorRgba.Green
            };
            yield return new LightInfo
            {
                on = Color.blue,
                off = Color.Lerp(Color.blue, Color.black, 0.5f),
                icon = ColorRgba.Blue
            };
        }
    }
}