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
            __instance.UntrainLastFightingStyle();
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
                var previouslySelectedFightingStyleField = AccessTools.Field(typeof(CharacterStageFightingStyleSelectionPanelPatcher), "previouslySelectedFightingStyle");

                foreach (var instruction in instructions)
                {
                    if (instruction.StoresField(AccessTools.Field(typeof(CharacterStageFightingStyleSelectionPanel), "selectedFightingStyle")))
                    {
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Stsfld, previouslySelectedFightingStyleField);
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