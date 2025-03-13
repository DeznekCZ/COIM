using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Core.Entities;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Core.World;

namespace ProgramableNetwork
{
    public partial class DataBands : AValidatedData
    {
        public static readonly Proto.ID DataBand_Unknown = new Proto.ID("ProgramableNetwork_DataBand_Unknown");
        public static readonly Proto.ID DataBand_FM = new Proto.ID("ProgramableNetwork_DataBand_FM");
        public static readonly Proto.ID DataBand_AM = new Proto.ID("ProgramableNetwork_DataBand_AM");

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(DataBandProto.Create<UnkownnDataBandType, IDataBandChannel>(
                id: DataBand_Unknown,
                strings: Proto.CreateStr(DataBand_Unknown, "Unknown", "Received signal is unrecognizable", "unkonwn band description"),
                (antena, context, proto) => new UnkownnDataBandType(context, proto),
                channels: 0,
                (c0, c1) => false,
                UnkownnDataBandType.Serialize,
                UnkownnDataBandType.Deserialize,
                channelDisplay: (c,i) => "~"));

            // known signals
            registrator.PrototypesDb.Add(DataBandProto.Create<FMDataBand, FMDataBandChannel>(
                id: DataBand_FM,
                strings: Proto.CreateStr(DataBand_FM, "FM", "Standard Frquency Modulated signal used in classic radios. The channels are from 85.5 to 108.0 kHz and steping by 500 Hz (total 45 channels), default redirection distance is 1000 metres, Antena tower may extend it", "Commonly known radio signal description"),
                (antena, context, proto) => new FMDataBand(antena, context, proto),
                channels: 45,
                (c0, c1) => c0.Index != c1.Index,
                FMDataBand.Serialize,
                FMDataBand.Deserialize,
                channelDisplay: (c, i) => ((171 + i.Index).ToFix32() * 0.5f.ToFix32()).ToStringRounded(1) + " kHz",
                buttons: (entity, inspector, view, builder, container, dataBand, proto) =>
                {
                    new AntenaPicker(builder, entity.Prototype, dataBand, value => dataBand.Antena = value,
                        proto.Distance * entity.Prototype.DistanceBoost,
                        () => { }, view, inspector)
                        .AppendTo(container);

                    var text = builder.NewTxt("band_channel_value")
                        .SetHeight(40)
                        .SetText(proto.Display(entity.Context, dataBand))
                        .SetAlignment(UnityEngine.TextAnchor.MiddleLeft)
                        .SetText(proto.Display(entity.Context, dataBand))
                        .AppendTo(container);

                    builder.NewBtnGeneral("NstartkHz")
                        .SetText("|<")
                        .OnClick(() =>
                        {
                            dataBand.Index = 0;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                    builder.NewBtnGeneral("N-5kHz")
                        .SetText("<<")
                        .OnClick(() =>
                        {
                            dataBand.Index -= 10;
                            if (dataBand.Index < 0)
                                dataBand.Index += 46;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N-0.5kHz")
                        .SetText("<")
                        .OnClick(() =>
                        {
                            dataBand.Index -= 1;
                            if (dataBand.Index < 0)
                                dataBand.Index += 46;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+0.5kHz")
                        .SetText(">")
                        .OnClick(() =>
                        {
                            dataBand.Index += 1;
                            if (dataBand.Index > 45)
                                dataBand.Index -= 46;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+5kHz")
                        .SetText(">>")
                        .OnClick(() =>
                        {
                            dataBand.Index += 10;
                            if (dataBand.Index > 45)
                                dataBand.Index -= 46;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("NendkHz")
                        .SetText(">|")
                        .OnClick(() =>
                        {
                            dataBand.Index = 45;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                },
                distance: 500.ToFix32()
                ));

            registrator.PrototypesDb.Add(DataBandProto.Create<AMDataBand, AMDataBandChannel>(
                id: DataBand_AM,
                strings: Proto.CreateStr(DataBand_AM, "AM", "Standard Amplitude Modulated signal used in long range radios. The channels are from 530 to 1700 kHz and steping by 10 kHz (total 118 channels), default redirection distance is 500 km, Antena tower may extend it", "Commonly known radio signal description"),
                (antena, context, proto) => new AMDataBand(antena, context, proto),
                channels: 118,
                (c0, c1) => c0.Index != c1.Index,
                AMDataBand.Serialize,
                AMDataBand.Deserialize,
                channelDisplay: (c, i) => ((53 + i.Index).ToFix32() * 10.ToFix32()).IntegerPart + " kHz",
                buttons: (entity, inspector, view, builder, container, dataBand, proto) =>
                {
                    var text = builder.NewTxt("band_channel_value")
                        .SetHeight(20)
                        .SetText(proto.Display(entity.Context, dataBand))
                        .SetWidth(180)
                        .SetAlignment(UnityEngine.TextAnchor.MiddleLeft);

                    var action = new MineActionTab(builder, entity, dataBand, (a, m) => true, view, inspector)
                        .AppendTo(container);

                    new MineTab(builder, entity, dataBand,
                        entity.Prototype.DistanceBoost,
                        view, inspector,
                        () => {
                            text.SetText(proto.Display(entity.Context, dataBand));
                            action.Refresh(); // for update of product
                        })
                        .AppendTo(container);

                    text.AppendTo(container);

                    builder.NewBtnGeneral("NstartkHz")
                        .SetText("|<")
                        .OnClick(() =>
                        {
                            dataBand.Index = 0;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                    builder.NewBtnGeneral("N-5kHz")
                        .SetText("<<")
                        .OnClick(() =>
                        {
                            dataBand.Index -= 10;
                            if (dataBand.Index < 0)
                                dataBand.Index += 118;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N-0.5kHz")
                        .SetText("<")
                        .OnClick(() =>
                        {
                            dataBand.Index -= 1;
                            if (dataBand.Index < 0)
                                dataBand.Index += 118;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+0.5kHz")
                        .SetText(">")
                        .OnClick(() =>
                        {
                            dataBand.Index += 1;
                            if (dataBand.Index > 117)
                                dataBand.Index -= 118;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+5kHz")
                        .SetText(">>")
                        .OnClick(() =>
                        {
                            dataBand.Index += 10;
                            if (dataBand.Index > 117)
                                dataBand.Index -= 118;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("NendkHz")
                        .SetText(">|")
                        .OnClick(() =>
                        {
                            dataBand.Index = 117;
                            text.SetText(proto.Display(entity.Context, dataBand));
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                },
                distance: 500.ToFix32()
                ));
        }
    }
}
