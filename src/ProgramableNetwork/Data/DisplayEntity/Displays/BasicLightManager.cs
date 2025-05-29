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
using Mafi.Unity.Ui;
using RTG;

namespace ProgramableNetwork.Data.DisplayEntity.Displays
{
    public class BasicLightManager : IDisplayEntityManager
    {
        private Renderer m_render;
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
            Slider intensity;
            var components = new UiComponent[] {
                active = new Label("Active".AsLoc()).TextAlign(TextAlignment.LeftMiddle)
                .FlexGrow(0.4f),
                toggle = new Toggle().FlexGrow(0.1f),
                dropdown = new Dropdown<LightInfo>(
                    optionViewFactory: (option, index, isInDropdown) => new Icon(UserInterface.General.Circle_svg).Color(option.icon),
                    customButton: colorIcon = new ButtonIcon(UserInterface.General.Circle_svg)
                )
                .OnValueChanged((v, i) => {
                    Entity.SetProperty("colorOn.R", v.on.R);
                    Entity.SetProperty("colorOn.G", v.on.G);
                    Entity.SetProperty("colorOn.B", v.on.B);
                    Entity.SetProperty("colorOff.R", v.off.R);
                    Entity.SetProperty("colorOff.G", v.off.G);
                    Entity.SetProperty("colorOff.B", v.off.B);
                })
                .SetOptions(Colors().ToImmutableArray())
                .FlexGrow(0.5f)
            };

            m_row = panel.AddPanelRow(components);

            m_row.Observe(() => Entity.IsActive)
               .Do((playing) => {
                   toggle.Value(playing);
                   if (playing)
                       panel.Status.AsWorking();
                   else
                       panel.Status.AsIdle();
               });

            m_row
               .Observe(() => new ColorRgba(
                    Entity.GetProperty("colorOn.R", ColorRgba.Red.R).IntegerPart,
                    Entity.GetProperty("colorOn.G", ColorRgba.Red.G).IntegerPart,
                    Entity.GetProperty("colorOn.B", ColorRgba.Red.B).IntegerPart
                ))
               .Observe(() => new ColorRgba(
                    Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(63).R).IntegerPart,
                    Entity.GetProperty("colorOff.G", ColorRgba.Red.G).IntegerPart,
                    Entity.GetProperty("colorOff.B", ColorRgba.Red.B).IntegerPart
                ))
               .Do((colorOn, colorOff) => {
                   for (var i = 0; i < dropdown.OptionsCount; i++)
                   {
                       var option = dropdown.GetOptionAt(i);
                       if (option.on == colorOn && option.off == colorOff)
                       {
                           //dropdown.SetValueIndex(i);
                           colorIcon.Icon.Color(option.icon);
                           return;
                       }
                   }
                   //dropdown.SetValueIndex(0);
                   //colorIcon.Icon.Color(dropdown.GetOptionAt(0).icon);
               });

            toggle.OnValueChanged((playing) => Entity.SetActive(playing));

            InitColorSelection(colorIcon, dropdown);

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
            var colorOn = new Color(
                Entity.GetProperty("colorOn.R", ColorRgba.Red.R).ToFloat() / 127,
                Entity.GetProperty("colorOn.G", ColorRgba.Red.G).ToFloat() / 127,
                Entity.GetProperty("colorOn.B", ColorRgba.Red.B).ToFloat() / 127
            );
            var colorOff = new Color(
                Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(63).R).ToFloat() / 255,
                Entity.GetProperty("colorOff.G", ColorRgba.Red.G).ToFloat() / 255,
                Entity.GetProperty("colorOff.B", ColorRgba.Red.B).ToFloat() / 255
            );
            materialPropertyBlock.SetColor("_Color", colorOff);
            materialPropertyBlock.SetColor("_EmissionColor", colorOn);
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
                on = ColorRgba.Red,
                off = ColorRgba.Red.SetR(63).SetA(255),
                icon = ColorRgba.Red
            };
            yield return new LightInfo
            {
                on = ColorRgba.Yellow,
                off = ColorRgba.Yellow.SetR(63).SetG(63),
                icon = ColorRgba.Yellow
            };
            yield return new LightInfo
            {
                on = ColorRgba.Green,
                off = ColorRgba.Green.SetG(63),
                icon = ColorRgba.Green
            };
            yield return new LightInfo
            {
                on = ColorRgba.Blue,
                off = ColorRgba.Blue.SetB(63),
                icon = ColorRgba.Blue
            };
            yield return new LightInfo
            {
                on = ColorRgba.LightGray,
                off = ColorRgba.LightGray.SetR(63).SetG(63).SetB(63),
                icon = ColorRgba.LightGray
            };
        }
    }
}