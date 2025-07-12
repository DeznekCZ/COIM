using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity;
using static Mafi.Unity.Assets.Unity;
using System;
using System.Collections.Generic;
using ProgramableNetwork;
using Mafi.Collections.ImmutableCollections;
using System.Linq;
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;
using Mafi.Unity.Ui;
using RTG;
using Mafi.Unity.UiToolkit;
using UnityEngine;
using Display = Mafi.Unity.Ui.Library.Display;

namespace ProgramableNetwork.Ui.DisplayEntity.Displays
{
    public class SixteenSegmentManager : IDisplayEntityInspector
    {
        public static string[] SEGMENTS = ["A1", "B", "C", "D1", "E", "F", "G1", "A2", "D2", "G2", "H", "I", "J", "K", "L", "M", "DP"];
        private PanelRow m_row;
        private PanelWithHeader m_act;

        public SixteenSegmentManager(Data.DisplayEntity.DisplayEntity entity)
        {
            Entity = entity;
        }

        public Data.DisplayEntity.DisplayEntity Entity { get; }

        public Action Create(DisplayEntityInspector panel)
        {
            Label active;
            ButtonIcon colorIcon;
            Dropdown<LightInfo> dropdown;
            Display number;
            var components = new UiComponent[] {
                active = new Label("Color".AsLoc()).TextAlign(TextAlignment.LeftMiddle)
                .FlexGrow(0.4f),
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
                .FlexGrow(0.5f),
                number = new Display("000".AsLoc()).Width(1.25f * Sizes.BLOCK_SIZE)
            };

            m_row = panel.AddPanelRow(components);

            m_row.Observe(() => Entity.IsActive)
               .Do((playing) => {
                   if (playing)
                       panel.Status.AsWorking();
                   else
                       panel.Status.AsIdle();
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
                      for (int i = 0; i < list.Count; i++)
                          if (list[i])
                              final = final | (1 << i);
                      number.Value(final.ToString("D3").AsLoc());
                  });

            m_row
               .Observe(() => new ColorRgba(
                    Entity.GetProperty("colorOn.R", ColorRgba.Red.R).IntegerPart,
                    Entity.GetProperty("colorOn.G", ColorRgba.Red.G).IntegerPart,
                    Entity.GetProperty("colorOn.B", ColorRgba.Red.B).IntegerPart
                ))
               .Observe(() => new ColorRgba(
                    Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(100).R).IntegerPart,
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

            m_act = panel.AddRootPanelWithHeader();
            m_act.Body.AlignItemsCenterMiddle();
            m_act.Body.Add(new Row(5) { Toggles(panel).ToArray() });
            m_act.Header.Add(new Label("Segments".AsLoc()).Class(Cls.panelHeader).TextAlign(TextAlignment.CenterMiddle));

            InitColorSelection(colorIcon, dropdown);

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

        private void InitColorSelection(ButtonIcon colorIcon, Dropdown<LightInfo> dropdown)
        {
            var colorOn = new ColorRgba(
                Entity.GetProperty("colorOn.R", ColorRgba.Red.R).IntegerPart,
                Entity.GetProperty("colorOn.G", ColorRgba.Red.G).IntegerPart,
                Entity.GetProperty("colorOn.B", ColorRgba.Red.B).IntegerPart
            );
            var colorOff = new ColorRgba(
                Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(100).R).IntegerPart,
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
        private Dictionary<string, Renderer> m_render;
        private Color m_oldColorOn;
        private Color m_oldColorOff;

        public SixteenSegmentManager(DisplayEntity disp)
        {
            Entity = disp;
            Proto = Entity.Prototype;
            Inspector = new Ui.DisplayEntity.Displays.SixteenSegmentManager(disp);
            m_oldColorOn = Color.black;
            m_oldColorOff = Color.black;
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

            ApplyColors();
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

            var colorOn = new Color(
                Entity.GetProperty("colorOn.R", ColorRgba.Red.R).ToFloat() / 127,
                Entity.GetProperty("colorOn.G", ColorRgba.Red.G).ToFloat() / 127,
                Entity.GetProperty("colorOn.B", ColorRgba.Red.B).ToFloat() / 127
            );
            var colorOff = new Color(
                Entity.GetProperty("colorOff.R", ColorRgba.Red.SetR(100).R).ToFloat() / 255,
                Entity.GetProperty("colorOff.G", ColorRgba.Red.G).ToFloat() / 255,
                Entity.GetProperty("colorOff.B", ColorRgba.Red.B).ToFloat() / 255
            );

            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            if (m_oldColorOn != colorOn)
                materialPropertyBlock.SetColor("_EmissionColor", colorOn);
            if (m_oldColorOff != colorOff)
                materialPropertyBlock.SetColor("_Color", colorOff);
            m_oldColorOn = colorOn;
            m_oldColorOff = colorOff;

            if (!materialPropertyBlock.isEmpty)
                foreach (var render in m_render.Values)
                    render.SetPropertyBlock(materialPropertyBlock);

            foreach (var render in m_render)
            {
                if (Entity.GetProperty(render.Key) > 0)
                    render.Value.material.EnableKeyword("_EMISSION");
                else
                    render.Value.material.DisableKeyword("_EMISSION");
            }
        }
    }
}