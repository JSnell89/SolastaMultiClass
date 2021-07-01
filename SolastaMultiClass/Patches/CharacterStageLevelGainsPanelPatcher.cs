using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SolastaMultiClass.Patches
{
    class CharacterStageLevelGainsPanelPatcher
    {
        [HarmonyPatch(typeof(CharacterStageLevelGainsPanel), "EnterStage")]
        internal static class CharacterStageLevelGainsPanel_EnterStage_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getLastAssignedClassAndLevelMethod = typeof(ICharacterBuildingService).GetMethod("GetLastAssignedClassAndLevel");
                var getHeroSelectedClassAndLevelMethod = typeof(SolastaMultiClass.Models.MultiClass).GetMethod("GetHeroSelectedClassAndLevel");
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