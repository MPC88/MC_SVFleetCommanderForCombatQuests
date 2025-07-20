
using BepInEx;
using HarmonyLib;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

namespace MC_SVFleetCommanderForCombatQuests
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.fleetcommanderforcombatquests";
        public const string pluginName = "SV Fleet Commander for Combat Quests";
        public const string pluginVersion = "1.0.2";

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Main));
        }

        [HarmonyPatch(typeof(QuestDB), nameof(QuestDB.GetKnowledgeRequirementString))]
        [HarmonyPostfix]
        private static void QuestDBReqString_Post(Quest quest, ref string __result)
        {
            int req = QuestDB.GetKnowledgeRequirement(quest);
            if (req <= 0)
                return;

            __result = ColorSys.mediumGray + Lang.Get(5, 117) + " </color>";

            // Space pilot part
            if (PChar.SpacePilot() < req)
                __result += ColorSys.infoNeg;
            else
                __result += ColorSys.infoPos;
            __result += Lang.Get(0, 90) + " <b>" + req + "</b></color>" + ColorSys.mediumGray + " / </color>";

            // Fleet commander part
            if (PChar.FleetCommander(true) < req)
                __result += ColorSys.infoNeg;
            else
                __result += ColorSys.infoPos;
            __result += Lang.Get(0, 93) + " <b>" + req + "</b></color>";
        }

        [HarmonyPatch(typeof(DockingUI), nameof(DockingUI.OpenStationQuest))]
        [HarmonyPostfix]
        private static void DockUIOpenStationQuest_Post(DockingUI __instance, int index)
        {
            Quest quest = (__instance.station.GetModule(typeof(SM_MissionBoard)) as SM_MissionBoard).quests[index].quest;
            int requirement = QuestDB.GetKnowledgeRequirement(quest);

            if (quest.kind == QuestKind.Combat && 
                PChar.SpacePilot() < requirement && 
                PChar.FleetCommander(true) >= requirement)
            {                
                AccessTools.FieldRefAccess<InfoPanelControl, GameObject>("warningGO")(InfoPanelControl.inst).SetActive(false);
                InfoPanelControl.inst.HideText(false);
                QuestControl.instance.OpenQuestDialog(quest, 0, true);
                QuestControl.instance.isStationQuest = index;
            }
        }
    }
}


