using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SolastaMultiClass.Patches
{
    internal static class LevelUpSequencePatcher
    {
        /*

        Methods Call Sequence on the Level Up Screen

        [SolastaMultiClass] CharacterCreationScreen
        [SolastaMultiClass] CharacterStageClassSelectionPanel.OnEndHide
        [SolastaMultiClass] CharacterStageLevelGainsPanel.OnEndHide
        [SolastaMultiClass] CharacterStageClassSelectionPanel.EnterStage
        [SolastaMultiClass] CharacterStageClassSelectionPanel.OnBeginShow
        [SolastaMultiClass] CharacterStageLevelGainsPanel.EnterStage
        [SolastaMultiClass] CharacterStageClassSelectionPanel.OnEndHide
        [SolastaMultiClass] CharacterStageLevelGainsPanel.OnBeginShow
        [SolastaMultiClass] CharacterStageLevelGainsPanel.OnEndHide
        [SolastaMultiClass] CharacterStageClassSelectionPanel.OnBeginShow
        [SolastaMultiClass] CharacterStageLevelGainsPanel.EnterStage
        [SolastaMultiClass] CharacterStageClassSelectionPanel.OnEndHide
        [SolastaMultiClass] CharacterStageLevelGainsPanel.OnBeginShow
        [SolastaMultiClass] CharacterStageLevelGainsPanel.OnEndHide
        [SolastaMultiClass] CharacterStageClassSelectionPanel.OnBeginShow

        */

        internal static bool blockUnassign = false;
        internal static int classesAndLevelsCount = 0;
        internal static CharacterClassDefinition selectedClass = null;

        public static void GetHeroSelectedClassAndLevel(out CharacterClassDefinition lastClassDefinition, out int level)
        {
            lastClassDefinition = selectedClass;
            level = classesAndLevelsCount;
        }

        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignLastClassLevel")]
        internal static class CharacterBuildingManager_OnBeginShow_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance)
            {
                return !blockUnassign || __instance.HeroCharacter.ClassesHistory.Count > classesAndLevelsCount;
            }
        }

        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "EnterStage")]
        internal static class CharacterStageClassSelectionPanel_Bind_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance)
            {
                Main.Log("CharacterStageClassSelectionPanel.EnterStage");
                classesAndLevelsCount = __instance.CharacterBuildingService.HeroCharacter.ClassesHistory.Count;
            }
        }

        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnBeginShow")]
        internal static class CharacterStageClassSelectionPanel_EnterStage_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance)
            {
                Main.Log("CharacterStageClassSelectionPanel.OnBeginShow");
                blockUnassign = true;
            }
        }

        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnEndHide")]
        internal static class CharacterStageClassSelectionPanel_BeginHide_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance)
            {
                Main.Log("CharacterStageClassSelectionPanel.OnEndHide");
                blockUnassign = false;
            }
        }

        [HarmonyPatch(typeof(CharacterStageLevelGainsPanel), "EnterStage")]
        internal static class CharacterStageLevelGainsPanel_EnterStage_Patch
        {
            internal static void Prefix(CharacterStageLevelGainsPanel __instance)
            {
                Main.Log("CharacterStageLevelGainsPanel.EnterStage");
                selectedClass = __instance.CharacterBuildingService.HeroCharacter.ClassesHistory[classesAndLevelsCount];
                __instance.CharacterBuildingService.UnassignLastClassLevel();
            }

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getLastAssignedClassAndLevelMethod = typeof(ICharacterBuildingService).GetMethod("GetLastAssignedClassAndLevel");
                var getHeroSelectedClassAndLevelMethod = typeof(SolastaMultiClass.Patches.LevelUpSequencePatcher).GetMethod("GetHeroSelectedClassAndLevel");
                var instructionsToBypass = 2;

                /*

                0	0000	ldarg.0
                1	0001	call	instance class ICharacterBuildingService CharacterStagePanel::get_CharacterBuildingService()
                2	0006	ldarg.0
                3	0007	ldflda	class CharacterClassDefinition CharacterStageLevelGainsPanel::lastGainedClassDefinition
                4	000C	ldarg.0
                5	000D	ldflda	int32 CharacterStageLevelGainsPanel::lastGainedClassLevel
                6	0012	callvirt	instance void ICharacterBuildingService::GetLastAssignedClassAndLevel(class CharacterClassDefinition&, int32&)

                below removes lines 0 and 1 and then replaces 6 with a call to SolastaMultiClass.Models.MultiClass.GetHeroSelectedClassAndLevel (same signature as one replaced)

                */

                foreach (var instruction in instructions)
                {
                    if (instructionsToBypass > 0)
                    {
                        instructionsToBypass -= 1;
                    }
                    else if (instruction.Calls(getLastAssignedClassAndLevelMethod))
                    {
                        yield return new CodeInstruction(OpCodes.Call, getHeroSelectedClassAndLevelMethod);
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