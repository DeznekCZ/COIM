using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities.Static;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using Mafi.Unity.InstancedRendering;
using RTG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProgramableNetwork.Data.Computer
{
    public class ControllerMb : StaticEntityMb, IEntityMbWithRenderUpdate
    {
        private ImmutableArray<IColorizer> m_colorizers;
        private Controller m_controller;
        private ColorRgba m_color = ColorRgba.Empty;

        public void Initialize(Controller layoutEntity)
        {
            base.Initialize(layoutEntity);
            m_controller = layoutEntity;
            List<IColorizer> colorizers = [];
            generateColorizers(colorizers);
            m_colorizers = colorizers.ToImmutableArray();
        }

        private void generateColorizers(List<IColorizer> colorizers)
        {
            foreach (Transform colorable in transform)
            {
                string transformName = colorable.name;
                if (transformName.EndsWith("[0,0,0]"))
                {
                    //Log.Info($"ControllerMb: Skipped {colorable.name}");
                    continue;
                }
                try
                {
                    string colorPart = transformName.Split('[')[1].TrimEnd(']');
                    string[] integerStrings = colorPart.Split(',');

                    List<MeshRenderer> renderers = [];
                    foreach (Transform colorTransform in colorable)
                    {
                        var renderer = colorTransform.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            renderers.Add(renderer);
                        }
                    }

                    Log.Info($"ControllerMb: Loaded {renderers.Count}");

                    if (integerStrings[0] == "255" && integerStrings[1] == "255" && integerStrings[2] == "255")
                    {
                        colorizers.Add(new Colorizer(renderers.ToImmutableArray()));
                    }
                    else
                    {
                        colorizers.Add(new PartialColorizer(renderers.ToImmutableArray(),
                            int.Parse(integerStrings[0]),
                            int.Parse(integerStrings[1]),
                            int.Parse(integerStrings[2])));
                    }
                    return;
                }
                catch (Exception e)
                {
                    Log.Error("ControllerMb: Invalid colorable block");
                    Log.Exception(e);
                }
            }
        }

        private void applyColor()
        {
            m_color = m_controller.Color;
            foreach (IColorizer colorizer in m_colorizers)
            {
                colorizer.Colorize(m_color);
            }
        }

        public void RenderUpdate(GameTime time)
        {
            if (m_color != m_controller.Color)
            {
                applyColor();
            }
        }

        private interface IColorizer
        {
            void Colorize(ColorRgba color);
        }

        private class Colorizer : IColorizer
        {
            private readonly ImmutableArray<MeshRenderer> m_renderers;

            public Colorizer(ImmutableArray<MeshRenderer> renderers)
            {
                m_renderers = renderers;
            }

            public virtual void Colorize(ColorRgba color)
            {
                foreach (MeshRenderer render in m_renderers)
                {
                    try
                    {
                        MaterialPropertyBlock materialPropertyBlock = new();
                        materialPropertyBlock.SetColor("_Color", color.SetA(255).ToColor());
                        render.SetPropertyBlock(materialPropertyBlock);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Render has invalid state: {render.GetType()}: {render.gameObject.name}");
                        Log.Exception(e);
                    }
                }
            }
        }

        private class PartialColorizer : Colorizer
        {
            private readonly int m_r;
            private readonly int m_g;
            private readonly int m_b;

            public PartialColorizer(ImmutableArray<MeshRenderer> renderers, int r, int g, int b)
                : base(renderers)
            {
                m_r = r;
                m_g = g;
                m_b = b;
            }

            public override void Colorize(ColorRgba color)
            {
                color = color
                    .SetR((byte)(int)(color.R * (m_r / 255f)).Min(255).Max(0))
                    .SetG((byte)(int)(color.G * (m_g / 255f)).Min(255).Max(0))
                    .SetB((byte)(int)(color.B * (m_b / 255f)).Min(255).Max(0));

                Colorize(color);
            }
        }
    }
}
