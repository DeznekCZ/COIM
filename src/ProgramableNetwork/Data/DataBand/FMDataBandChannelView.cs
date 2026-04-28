using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Ui.DataBand
{
    public class FMDataBandChannelView : PanelRow
    {
        /// <summary>
        /// Used for common channels
        /// </summary>
        /// <param name="type"></param>
        /// <param name="antenaInspector"></param>
        /// <param name="channel"></param>
        public FMDataBandChannelView(AntenaInspector antenaInspector, FMDataBandChannel channel)
        {
            Column frequency = Body.AddAndReturn(new Column());

            Row firstRow = frequency.AddAndReturn(new Row()).Width(400);

            Display display = firstRow.AddAndReturn(new Display()).FlexGrow(1);
            display.OnMouseEnterLeave(
                () =>
                {
                    if (channel.Antena != null) {
						antenaInspector.EntityHighlighter.HighlightOnly(channel.Antena, ColorRgba.CornflowerBlue);
					}
				},
                () =>
                {
                    antenaInspector.EntityHighlighter.ClearAllHighlights();
                });

            this.Observe(() => channel.Index)
                .Observe(() => channel.Antena?.Prototype.IconPath)
                .Do((index, icon) =>
                {
                    // TODO antena picker

                    if (icon != null)
                    {
                        string displayValue = channel.OriginalDataBand.Prototype
                                                 .Display(channel.OriginalDataBand.Antena.Context, channel);
                        display.Value($"FM {displayValue}".AsLoc());
                    }
                    else
                    {
                        display.Value(NewTr.Inspector.Disconnected);
                    }
                });

            // Re-apply Disconnected on first show in case construction captured a pre-rebind LocStr
            // (the observer above only refires on state changes, so a steady disconnected channel
            // would otherwise stay frozen with whatever text was current at construction time).
            display.LaterText<Display>(() => NewTr.Inspector.Disconnected, this, (d, v) => {
                if (channel.Antena == null) {
                    d.Value(v);
                }
            });

            firstRow.AddAndReturn(new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png))
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .OnClick(() => {
                    channel.OriginalDataBand.RemoveChannel(channel);
                    RemoveFromHierarchy();
                })
                .IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
                .Icon.Padding(Sizes.IMAGE_PADDING)
                        .Margin(Px.Zero);

            Row secondRow = frequency.AddAndReturn(new Row()).Width(400);

            // move to beginning
            secondRow.AddAndReturn(new ButtonText("|<<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => channel.Index = 0)
                .ObserveEnabled(() => channel.Index > 0);

            // move to left fast
            secondRow.AddAndReturn(new ButtonText("<<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => channel.Move(-5));

            // move to left
            secondRow.AddAndReturn(new ButtonText("<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => channel.Move(-1));

            // move to right
            secondRow.AddAndReturn(new ButtonText(">".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => channel.Move(1));

            // move to right fast
            secondRow.AddAndReturn(new ButtonText(">>".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => channel.Move(5));

            // move to end
            secondRow.AddAndReturn(new ButtonText(">>|".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => channel.Index = channel.OriginalDataBand.Prototype.Channels - 1)
                .ObserveEnabled(() => channel.Index < channel.OriginalDataBand.Prototype.Channels - 2);

            try
            {
                Body.Add(
                    new AntenaPicker(
                        module: channel,
                        distance: channel.OriginalDataBand.Prototype.Distance * channel.OriginalDataBand.Antena.Prototype.DistanceBoost,
                        refresh: () => { },
                        inspector: antenaInspector
                    ));
            }
            catch (Exception e)
            {
                Log.Exception(e, "Failed to get instance of antena");
                // ignore missing value
            }
        }

        public FMDataBandChannelView(ControllerInspector controllerInspector, Action refresh, Reference reference)
        {
            this.Width(400);

            var dataBandProto = controllerInspector.Entity.Context.ProtosDb
                                           .Get<DataBandProto>(DataBands.DataBand_FM).Value;


            Row firstRow = Body.AddAndReturn(new Row());

            Display display = firstRow.AddAndReturn(new Display()).FlexGrow(1);

            display.Observe(() => reference.Value)
                   .Do(index =>
                   {
                       string displayValue = ((171 + reference.Value) * 0.5f.ToFix32()).ToStringRounded(1);
                       display.Value($"FM {displayValue} MHz".AsLoc());
                   });

            Row secondRow = Body.AddAndReturn(new Row());

            // move to beginning
            secondRow.AddAndReturn(new ButtonText("|<<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => reference.Value = 0)
                .ObserveEnabled(() => reference.Value > 0);

            // move to left fast
            secondRow.AddAndReturn(new ButtonText("<<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => Move(-5));

            // move to left
            secondRow.AddAndReturn(new ButtonText("<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => Move(-1));

            // move to right
            secondRow.AddAndReturn(new ButtonText(">".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => Move(1));

            // move to right fast
            secondRow.AddAndReturn(new ButtonText(">>".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => Move(5));

            // move to end
            secondRow.AddAndReturn(new ButtonText(">>|".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => reference.Value = dataBandProto.Channels - 1)
                .ObserveEnabled(() => reference.Value < dataBandProto.Channels - 2);

            void Move(int v)
            {
                Fix32 newIndex = reference.Value + v;
                if (newIndex < 0) {
					reference.Value = dataBandProto.Channels + newIndex;
				} else if (newIndex >= dataBandProto.Channels) {
					reference.Value = newIndex - dataBandProto.Channels;
				} else {
					reference.Value = newIndex;
				}
			}
        }
    }
}
