using Mafi;
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
            Slider volume;
            AddPanelRow(
                new Label().LaterText(() => NewTr.Inspector.Active, this).TextAlign(TextAlignment.LeftMiddle)
                .FlexGrow(0.4f),
                toggle = new Toggle().FlexGrow(0.1f),
                dropdown = new Dropdown<KeyValuePair<string, LocStrFormatted>>(
                    optionViewFactory: (option, index, isInDropdown) => new Label(option.Value)
                )
                .OnValueChanged((v, i) => { Entity.SetSound(v.Key ?? UserInterface.Audio.ShipAlarm_prefab); })
                .SetOptions(BuildSounds())
                .FlexGrow(0.5f),
                volume = new Slider().FlexGrow(1)
            );

            this.Observe(() => Entity.IsPlaying)
                .Do((playing) => {
                    toggle.Value(playing);
                    if (playing) {
						Status.AsWorking();
					} else {
						Status.AsIdle();
					}
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

            this.Observe(() => Entity.Volume)
                .Do((sound) => {
                    volume.Value(sound.ToFloat() * 0.5f);
                });

            volume.OnValueChanged((value, _) =>
            {
                Entity.SetVolume(Percent.FromFloat(value * 2));
            });

            EmbedStatusToTheTop();
        }

        // Built lazily so the LocStr → LocStrFormatted conversions happen after ModTranslations.Load
        // has spliced/rebound translations. A static initializer would freeze English text if the
        // class cctor fires before rebind.
        public static ImmutableArray<KeyValuePair<string, LocStrFormatted>> BuildSounds()
            => new KeyValuePair<string, LocStrFormatted>[]
            {
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.ShipAlarm_prefab,
                        NewTr.Inspector.Sound_Alarm
                    ),
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.MoneyAction_prefab,
                        NewTr.Inspector.Sound_Cash
                    ),
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.TurretShot_prefab,
                        NewTr.Inspector.Sound_Shoot
                    ),
                    new KeyValuePair<string, LocStrFormatted>(
                        UserInterface.Audio.NewMessage_prefab,
                        NewTr.Inspector.Sound_Message
                    )
            }.ToImmutableArray();
    }
}
