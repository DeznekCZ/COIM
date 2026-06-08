using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities.Static;
using Mafi.Core.Messages;
using Mafi.Core.Messages.Goals;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;

namespace ProgramableNetwork;

public partial class NewIds
{
    public partial class Messages
    {
        public static readonly Proto.ID NetworkUnlocked = new Proto.ID("ProgramableNetwork_Message_NetworkUnlocked");
        public static readonly Proto.ID AddTwoNumbers = new Proto.ID("ProgramableNetwork_Message_AddTwoNumbers");
        public static readonly Proto.ID ProducerToStorage = new Proto.ID("ProgramableNetwork_Message_ProducerToStorage");
    }

    public partial class Goals
    {
        public static readonly Proto.ID PlaceController = new Proto.ID("ProgramableNetwork_Goal_PlaceController");
    }
}

/// <summary>
/// Adds a single starter goal that asks the player to place a <see cref="NewIds.Controllers.Controller"/>
/// once the "Programable Network" research (<see cref="NewIds.Research.ProgramableNetwork_Stage1"/>) is unlocked.
///
/// Wiring mirrors the base game (see Mafi.Base GoalsData / TutorialMessagesData): goal lists cannot trigger
/// directly off a research/proto unlock, so we deliver a short intro message on unlock
/// (<see cref="MessageTriggerOnProtoUnlockedProto"/>) and trigger the goal list on that message delivery
/// (<see cref="GoalsListTriggerOnMessageDelivered"/>).
///
/// Note: GoalsManager only instantiates goal lists on a NEW game with tutorials enabled (non-sandbox), so
/// this goal appears for new playthroughs, not on existing saves.
/// </summary>
internal class GoalsData : AValidatedData
{
    protected override void RegisterDataInternal(ProtoRegistrator registrator)
    {
        ProtosDb db = registrator.PrototypesDb;

        // These all exist by now: the controller entity is registered by Entities, the research node by
        // Research, and the General message group by Mafi.Base. GoalsData is registered after both in ModDefinition.
        StaticEntityProto controller = db.GetOrThrow<StaticEntityProto>(NewIds.Controllers.Controller);
        Proto research = db.GetOrThrow<Proto>(NewIds.Research.ProgramableNetwork_Stage1);
        MessageGroupProto group = db.GetOrThrow<MessageGroupProto>(Ids.MessageGroups.General);

        // Loc.Str runs here at registration time, which is AFTER ModTranslations.Load in the mod ctor, so these
        // strings are born already-translated (the LocStr-snapshot problem only bites static readonly fields).
        LocStr1 part1 = Loc.Str1("ProgramableNetwork_Message_NetworkUnlocked__part1",
            "You have unlocked the <b>Programmable Network</b>. Place a {0} to start automating your factory with logic.",
            "{0} = Controller building name");
        LocStr part2 = Loc.Str("ProgramableNetwork_Message_NetworkUnlocked__part2",
            "Once it is built, open the controller to add modules, wire their pins together, and program how it reacts to the rest of your island.",
            "intro tutorial for the programmable network controller");

        string content = part1.Format($"<b>{controller.Strings.Name}</b>").Value + "\n\n" + part2.TranslatedString;

        MessageProto message = db.Add(new MessageProto(
            NewIds.Messages.NetworkUnlocked,
            "Programmable Network unlocked",
            content,
            InGameMessageType.Tutorial,
            forceOpen: false,
            alwaysNotify: false,
            unlockSilentlyFromStart: false,
            group));

        // Deliver the message ~10s after the research node is unlocked.
        db.Add(new MessageTriggerOnProtoUnlockedProto(message, research));

        // Worked example tutorial: add two numbers with constant modules and show the result on a display.
        // Module names are pulled live (and bolded) so the text stays correct and localized; pin labels
        // (A / B / Value / Number / Sum) are stable UI strings written inline.
        ModuleProto constFloat = db.GetOrThrow<ModuleProto>(new Proto.ID("Constant_Float".ModuleId()));
        ModuleProto sumModule = db.GetOrThrow<ModuleProto>(new Proto.ID("Sum".ModuleId()));
        ModuleProto display7 = db.GetOrThrow<ModuleProto>(new Proto.ID("Arithmetic_Display_7SEG_B".ModuleId()));

        LocStr intro = Loc.Str("ProgramableNetwork_Message_AddTwoNumbers__intro",
            "Controllers run small programs built from <b>modules</b> wired together pin to pin. Let's build one that adds two numbers and shows the result.",
            "intro line of the add-two-numbers worked example");
        LocStr1 step1 = Loc.Str1("ProgramableNetwork_Message_AddTwoNumbers__step1",
            "Open the controller and add a {0} module. Set its <b>Float</b> field to 2 — it outputs that number on its <b>Value</b> pin.",
            "{0} = Constant (float) module name");
        LocStr1 step2 = Loc.Str1("ProgramableNetwork_Message_AddTwoNumbers__step2",
            "Add a second {0} module and set its <b>Float</b> field to 3.",
            "{0} = Constant (float) module name");
        LocStr1 step3 = Loc.Str1("ProgramableNetwork_Message_AddTwoNumbers__step3",
            "Add a {0} module. Connect the first constant's <b>Value</b> output to input <b>A</b>, and the second constant's <b>Value</b> output to input <b>B</b>. The <b>Sum</b> output now equals A + B = 5.",
            "{0} = Sum module name (C = A + B)");
        LocStr1 step4 = Loc.Str1("ProgramableNetwork_Message_AddTwoNumbers__step4",
            "Add a {0} module and connect the <b>Sum</b> output into its <b>Number</b> input. Its on-module <b>Value</b> readout shows 5.",
            "{0} = 7-segment arithmetic display module name");
        LocStr1 tip = Loc.Str1("ProgramableNetwork_Message_AddTwoNumbers__tip",
            "Tip: you can skip the second constant — turn on <b>Use direct constant</b> on the {0} module and type the second number straight into its <b>B</b> field.",
            "{0} = Sum module name (C = A + B)");

        string constName = $"<b>{constFloat.Strings.Name}</b>";
        string sumName = $"<b>{sumModule.Strings.Name}</b>";
        string dispName = $"<b>{display7.Strings.Name}</b>";

        // preProcessContent splits on '\n': blank lines break paragraphs, lines starting with [STEP] become steps.
        string tutorial =
            intro.TranslatedString + "\n\n" +
            "[STEP] " + step1.Format(constName).Value + "\n\n" +
            "[STEP] " + step2.Format(constName).Value + "\n\n" +
            "[STEP] " + step3.Format(sumName).Value + "\n\n" +
            "[STEP] " + step4.Format(dispName).Value + "\n\n" +
            tip.Format(sumName).Value;

        db.Add(new MessageProto(
            NewIds.Messages.AddTwoNumbers,
            "Your first program: add two numbers",
            tutorial,
            InGameMessageType.Tutorial,
            forceOpen: false,
            alwaysNotify: false,
            unlockSilentlyFromStart: false,
            group));

        // Second tutorial: set up a producer -> belt -> storage chain (something the controller can monitor).
        LocStr p2sIntro = Loc.Str("ProgramableNetwork_Message_ProducerToStorage__intro",
            "A controller is most useful when it can watch your factory. Set up something for it to watch: a machine that produces a resource, feeding a storage through a belt.",
            "intro line of the producer-belt-storage tutorial");
        LocStr p2sStep1 = Loc.Str("ProgramableNetwork_Message_ProducerToStorage__step1",
            "Build a machine that produces a resource (for example a smelter or a maker) and make sure it is running.",
            "tutorial step");
        LocStr p2sStep2 = Loc.Str("ProgramableNetwork_Message_ProducerToStorage__step2",
            "Build a storage and assign it the product that the machine outputs.",
            "tutorial step");
        LocStr p2sStep3 = Loc.Str("ProgramableNetwork_Message_ProducerToStorage__step3",
            "Connect the machine's output to the storage with a <b>belt</b>. Once the resource flows and the storage holds it, this step is complete.",
            "tutorial step");
        LocStr p2sTip = Loc.Str("ProgramableNetwork_Message_ProducerToStorage__tip",
            "Next, wire a <b>Connection (storage)</b> module in your controller to this storage to read its fill level and react to it.",
            "tutorial closing tip linking to the storage connection module");

        string producerTutorial =
            p2sIntro.TranslatedString + "\n\n" +
            "[STEP] " + p2sStep1.TranslatedString + "\n\n" +
            "[STEP] " + p2sStep2.TranslatedString + "\n\n" +
            "[STEP] " + p2sStep3.TranslatedString + "\n\n" +
            p2sTip.TranslatedString;

        db.Add(new MessageProto(
            NewIds.Messages.ProducerToStorage,
            "Automate a resource: producer, belt, storage",
            producerTutorial,
            InGameMessageType.Tutorial,
            forceOpen: false,
            alwaysNotify: false,
            unlockSilentlyFromStart: false,
            group));

        // Goal 0 — place one controller. TITLE_BUILD ("Build {0}") is the base game's own LocStr1, already
        // translated, and auto-formats with the controller's name plus a live (n / 1) counter.
        GoalToConstructStaticEntity.Proto placeController = new GoalToConstructStaticEntity.Proto(
            "PlaceController",
            Make.Kvp(controller, 1),
            GoalToConstructStaticEntity.Proto.TITLE_BUILD);

        // Goal 1 — custom goal type: build the worked example (the three required modules) inside a controller.
        // lockedByIndex: -1 (not locked) so all goals in the list are visible together once it activates —
        // the GoalsTab hides locked goals by default. The add-two-numbers tutorial is attached here
        // (UnlockAndNotify); GoalsManager delivers it when the list activates, after the research unlock.
        LocStrFormatted buildExampleTitle = Loc.Str("ProgramableNetwork_Goal_BuildExample__title",
            "Add two numbers: place two constants, a sum, and a display",
            "goal title for the worked example program");
        GoalToBuildControllerWithModules.Proto buildExample = new GoalToBuildControllerWithModules.Proto(
            "BuildExample",
            buildExampleTitle,
            ImmutableArray.Create(constFloat.Id, sumModule.Id, display7.Id),
            NewIds.Messages.AddTwoNumbers,
            -1,
            GoalProto.TutorialUnlockMode.UnlockAndNotify);

        // Goal 2 — custom goal type: connect a storage to the machine producing its resource with a belt.
        // lockedByIndex: -1 (not locked) so it shows alongside the others. Gives the controller a real
        // storage to monitor next.
        LocStrFormatted connectStorageTitle = Loc.Str("ProgramableNetwork_Goal_ConnectStorage__title",
            "Connect a storage to its producer with a belt",
            "goal title for the producer-belt-storage step");
        GoalToConnectStorageToProducer.Proto connectStorage = new GoalToConnectStorageToProducer.Proto(
            "ConnectStorage",
            connectStorageTitle,
            NewIds.Messages.ProducerToStorage,
            -1,
            GoalProto.TutorialUnlockMode.UnlockAndNotify);

        // CreateGoals() is internal to Mafi.Base, so replicate it: register each goal proto, then list them.
        db.Add(placeController, false);
        db.Add(buildExample, false);
        db.Add(connectStorage, false);

        db.Add(new GoalListProto(
            NewIds.Goals.PlaceController,
            ImmutableArray.Create<GoalProto>(placeController, buildExample, connectStorage),
            new GoalsListTriggerOnMessageDelivered.Data(NewIds.Messages.NetworkUnlocked),
            ImmutableArray<ProductQuantity>.Empty,
            "Programmable Network"));
    }
}
