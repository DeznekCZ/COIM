using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using Mafi.Unity.Ui.Library;

namespace ProgramableNetwork.Ui.DataBand
{
    public class AMDataBandChannelView : PanelRow
    {
        public AMDataBandChannelView(AntenaInspector inspector, AMDataBandChannel channel)
        {
            Column frequency = Body.AddAndReturn(new Column());

            Row firstRow = frequency.AddAndReturn(new Row()).Width(400);

            Display display = firstRow.AddAndReturn(new Display()).FlexGrow(1);

            this.Observe(() => channel.Index)
                .Observe(() => channel.WorldMapMine)
                .Observe(() => channel.BattleShip)
                .Do((index, mine, ship) =>
                {
                    // Channel is "connected" if it is bound to either a mine or the ship.
                    if (mine != null || ship != null)
                    {
                        string displayValue = channel.OriginalDataBand.Prototype
                                                 .Display(channel.OriginalDataBand.Antena.Context, channel);
                        display.Value($"AM {displayValue}".AsLoc());
                    }
                    else
                    {
                        display.Value(NewTr.Inspector.Disconnected);
                    }
                });

            // Re-apply Disconnected on first show; see FMDataBandChannelView for the rationale.
            display.LaterText<Display>(() => NewTr.Inspector.Disconnected, this, (d, v) => {
                if (channel.WorldMapMine == null && channel.BattleShip == null) {
                    d.Value(v);
                }
            });

            firstRow.AddAndReturn(new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png))
                .Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
                .Margin(Px.Zero)
                .OnClick(() => {
                    int slot = currentSlot(channel);
                    if (slot < 0) { return; }
                    inspector.Context.InputScheduler.ScheduleInputCmd(
                        new AntenaRemoveRedirectedChannelCmd(channel.OriginalDataBand.Antena.Id, slot));
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
                .OnClick(() => dispatchSetIndex(inspector, channel, 0))
                .ObserveEnabled(() => channel.Index > 0);

            // move to left fast
            secondRow.AddAndReturn(new ButtonText("<<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => dispatchMove(inspector, channel, -5));

            // move to left
            secondRow.AddAndReturn(new ButtonText("<".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => dispatchMove(inspector, channel, -1));

            // move to right
            secondRow.AddAndReturn(new ButtonText(">".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => dispatchMove(inspector, channel, 1));

            // move to right fast
            secondRow.AddAndReturn(new ButtonText(">>".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => dispatchMove(inspector, channel, 5));

            // move to end
            secondRow.AddAndReturn(new ButtonText(">>|".AsLoc()))
                .TextAlign(TextAlignment.CenterMiddle)
                .TextOverflow(TextOverflow.Clip)
                .Height(Sizes.BLOCK_SIZE)
                .FlexGrow(1)
                .OnClick(() => dispatchSetIndex(inspector, channel, channel.OriginalDataBand.Prototype.Channels - 1))
                .ObserveEnabled(() => channel.Index < channel.OriginalDataBand.Prototype.Channels - 2);

            try
            {
                Body.Add(
                    new MineTab(
                        uiContext: inspector.Context,
                        module: inspector.Entity,
                        fieldId: channel,
                        distanceBoost: channel.OriginalDataBand.Antena.Prototype.DistanceBoost,
                        parentWindow: inspector,
                        inspector: inspector
                    ));

                Body.Add(
                    new MineActionTab(
                        uiContext: inspector.Context,
                        module: inspector.Entity,
                        fieldId: channel,
                        filter: (antena, mine) => true,
                        parentWindow: inspector,
                        antenaInspector: inspector
                    ));
            }
            catch (Exception e)
            {
                Log.Exception(e, "Failed to get instance of antena");
                // ignore missing value
            }
        }

        public AMDataBandChannelView(ControllerInspector controllerInspector, Action refresh, Reference reference)
        {
            this.Width(400);

            var dataBandProto = controllerInspector.Entity.Context.ProtosDb
                                           .Get<DataBandProto>(DataBands.DataBand_AM).Value;


            Row firstRow = Body.AddAndReturn(new Row());

            Display display = firstRow.AddAndReturn(new Display()).FlexGrow(1);

            display.Observe(() => reference.Value)
                   .Do(index =>
                   {
                       string displayValue = ((53 + reference.Value) * 10.ToFix32()).IntegerPart.ToString();
					   display.Value($"AM {displayValue} kHz".AsLoc());
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

        /// <summary>Resolves a channel's current position in its band's redirected list. -1 if not found.</summary>
        private static int currentSlot(AMDataBandChannel channel)
        {
            int i = 0;
            foreach (var c in channel.OriginalDataBand.Channels)
            {
                if (object.ReferenceEquals(c, channel)) { return i; }
                i++;
            }
            return -1;
        }

        private static void dispatchSetIndex(AntenaInspector inspector, AMDataBandChannel channel, int newIndex)
        {
            int slot = currentSlot(channel);
            if (slot < 0) { return; }
            inspector.Context.InputScheduler.ScheduleInputCmd(
                new AntenaChannelSetIndexCmd(channel.OriginalDataBand.Antena.Id, slot, newIndex));
        }

        private static void dispatchMove(AntenaInspector inspector, AMDataBandChannel channel, int delta)
        {
            int total = channel.OriginalDataBand.Prototype.Channels;
            int next = channel.Index + delta;
            if (next < 0) { next += total; } else if (next >= total) { next -= total; }
            dispatchSetIndex(inspector, channel, next);
        }
    }
}
