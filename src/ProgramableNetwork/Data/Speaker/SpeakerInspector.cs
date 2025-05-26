using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mafi.Unity.Assets.Unity;

namespace ProgramableNetwork.Data.Speaker
{
    public class SpeakerInspector : BaseInspector<Speaker>
    {
        public SpeakerInspector(UiContext context) : base(context)
        {
            Toggle toggle;
            Dropdown<KeyValuePair<string, LocStrFormatted>> dropdown;
            AddPanelRow(
                new Label("Active".AsLoc()),
                toggle = new Toggle(),
                dropdown = new Dropdown<KeyValuePair<string, LocStrFormatted>>(
                    optionViewFactory: (option, index, isInDropdown) => new Label(option.Value)
                )
                .OnValueChanged((v, i) => { Entity.SetSound(v.Key ?? UserInterface.Audio.ShipAlarm_prefab); })
                .SetOptions(Sounds)
            );

            this.Observe(() => Entity.IsPlaying)
                .Do((playing) => {
                    toggle.Value(playing);
                    if (playing)
                        Status.AsIdle();
                    else
                        Status.AsWorking();
                });

            this.Observe(() => Entity.Sound)
                .Do((sound) => {
                    for (var i = 0; i < dropdown.OptionsCount; i++)
                    {
                        var option = dropdown.GetOptionAt(i);
                        if (option.Key == sound)
                        {
                            dropdown.SetValueIndex(i);
                            return;
                        }
                    }
                    dropdown.SetValueIndex(0);
                });

            toggle.OnValueChanged((playing) => Entity.SetPlaying(playing));

            EmbedStatusToTheTop();
        }

        /// <summary>
        /// TODO: add possibility to define custom sound, by any kind of prototype
        /// </summary>
        public static ImmutableArray<KeyValuePair<string, LocStrFormatted>> Sounds { get; }
            = new KeyValuePair<string, LocStrFormatted>[]
            {
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.ShipAlarm_prefab,
                        "Alarm".AsLoc()
                    ),
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.MoneyAction_prefab,
                        "Cash".AsLoc()
                    ),
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.TurretShot_prefab,
                        "Shoot".AsLoc()
                    ),
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.NewMessage_prefab,
                        "Message".AsLoc()
                    )
            }.ToImmutableArray();
    }
}
