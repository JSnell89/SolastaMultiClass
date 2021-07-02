using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterStageFightingStyleSelectionPanelPatcher
    {
        public static int previouslySelectedFightingStyle = -1;

        public static void UntrainPreviouslySelectedFightStyle(ICharacterBuildingService __instance)
        {
            if (previouslySelectedFightingStyle > 0)
            {
                __instance.UntrainLastFightingStyle();
            }
        }

        public static void AssignSelectedFightingStyle(CharacterStageFightingStyleSelectionPanel __instance, int selectedFightingStyle)
        {
            previouslySelectedFightingStyle = selectedFightingStyle;
            AccessTools.Field(__instance.GetType(), "").SetValue(__instance, selectedFightingStyle);
        }

        [HarmonyPatch(typeof(CharacterStageFightingStyleSelectionPanel), "OnBeginShow")]
        internal static class CharacterStageFightingStyleSelectionPanel_OnBeginShow_Patch
        {
            internal static void Postfix(int ___selectedFightingStyle) 
            {
                previouslySelectedFightingStyle = ___selectedFightingStyle;
            }
        }

        [HarmonyPatch(typeof(CharacterStageFightingStyleSelectionPanel), "Refresh")]
        internal static class CharacterStageFightingStyleSelectionPanel_Refresh_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var untrainLastFightingStyleMethod = typeof(ICharacterBuildingService).GetMethod("UntrainLastFightingStyle");
                var untrainPreviouslySelectedFightStyleMethod = typeof(CharacterStageFightingStyleSelectionPanelPatcher).GetMethod("UntrainPreviouslySelectedFightStyle");

                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(untrainLastFightingStyleMethod))
                    {
                        yield return new CodeInstruction(OpCodes.Call, untrainPreviouslySelectedFightStyleMethod);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CharacterStageFightingStyleSelectionPanel), "OnFightingStyleValueChangedCb")]
        internal static class CharacterStageFightingStyleSelectionPanel_OnFightingStyleValueChangedCb_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var selectedFightingStyleField = AccessTools.Field(typeof(CharacterStageFightingStyleSelectionPanel), "selectedFightingStyle");
                var AssignSelectedFightingStyleMethod = typeof(CharacterStageFightingStyleSelectionPanelPatcher).GetMethod("AssignSelectedFightingStyle");

                foreach (var instruction in instructions)
                {
                    if (instruction.StoresField(selectedFightingStyleField))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AssignSelectedFightingStyleMethod);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }
    }
}