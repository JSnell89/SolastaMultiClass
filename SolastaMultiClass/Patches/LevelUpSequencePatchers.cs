using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    internal static class LevelUpSequencePatchers
    {
        internal static bool blockUnassign = false;
        internal static int classesAndLevelsCount = 0;
        internal static int selectedClassIndex = -1;
        internal static CharacterClassDefinition selectedClass = null;

        // called by CharacterStageLevelGainsPanel.EnterStage with the transpiler injection
        public static void GetHeroSelectedClassAndLevel(out CharacterClassDefinition lastClassDefinition, out int level)
        {
            lastClassDefinition = selectedClass;
            level = classesAndLevelsCount;
            selectedClass = null;
        }

        // add the class selection stage panel to the Level Up process
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "LoadStagePanels")]
        internal static class CharacterLevelUpScreen_LoadStagePanels_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
            {
                var screen = Gui.GuiService.GetScreen<CharacterCreationScreen>();
                var stagePanelPrefabs = (GameObject[])AccessTools.Field(screen.GetType(), "stagePanelPrefabs").GetValue(screen);
                var classSelectionStagePanelPrefab = stagePanelPrefabs[1];
                var classSelectionPanel = Gui.GetPrefabFromPool(classSelectionStagePanelPrefab, __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();

                Dictionary<string, CharacterStagePanel> stagePanelsByName = new Dictionary<string, CharacterStagePanel>
                {
                    { "ClassSelection", classSelectionPanel }
                };
                foreach (var stagePanelName in ___stagePanelsByName)
                {
                    stagePanelsByName.Add(stagePanelName.Key, stagePanelName.Value);
                }
                ___stagePanelsByName.Clear();
                foreach (var stagePanelName in stagePanelsByName)
                {
                    ___stagePanelsByName.Add(stagePanelName.Key, stagePanelName.Value);
                }
            }
        }

        // filter the available classes per multi class in/out rules
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginShow")]
        internal static class CharacterLevelUpScreen_OnBeginShow_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
            {
                if (Main.Settings.ForceMinInOutPreReqs)
                {
                    var hero = __instance.CharacterBuildingService.HeroCharacter;
                    var classSelectionPanel = (CharacterStageClassSelectionPanel)___stagePanelsByName["ClassSelection"];
                    var compatibleClasses = (List<CharacterClassDefinition>)AccessTools.Field(classSelectionPanel.GetType(), "compatibleClasses").GetValue(classSelectionPanel);
                    var allowedClasses = new List<CharacterClassDefinition>() { };
                    
                    if (hero.ClassesHistory.Count == 0)
                    {
                        selectedClassIndex = -1;
                    }
                    else
                    {
                        allowedClasses = GetHeroAllowedClassDefinitions(hero);
                        selectedClassIndex = allowedClasses.IndexOf(hero.ClassesHistory[hero.ClassesHistory.Count - 1]);
                    }

                    compatibleClasses.Clear();
                    compatibleClasses.AddRange(allowedClasses);
                }
            }
        }

        // this avoids the last character level from being overwritten on a level up
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignLastClassLevel")]
        internal static class CharacterBuildingManager_OnBeginShow_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance)
            {
                return !blockUnassign || __instance.HeroCharacter.ClassesHistory.Count > classesAndLevelsCount;
            }
        }

        // this method only executes once, whenever the screen is displayed. perfect point for initialization
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "EnterStage")]
        internal static class CharacterStageClassSelectionPanel_Bind_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance)
            {
                classesAndLevelsCount = __instance.CharacterBuildingService.HeroCharacter.ClassesHistory.Count;
            }
        }

        // enables the trap on CharacterBuildingManager.UnassignLastClassLevel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnBeginShow")]
        internal static class CharacterStageClassSelectionPanel_EnterStage_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance, ref int ___selectedClass)
            {
                blockUnassign = true;

                ___selectedClass = selectedClassIndex;

                if (___selectedClass >= 0)
                {
                    __instance.CommonData.AttackModesPanel.Hide();
                    __instance.CommonData.PersonalityMapPanel.Hide();
                }
            }
        }

        // disables the trap on CharacterBuildingManager.UnassignLastClassLevel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnEndHide")]
        internal static class CharacterStageClassSelectionPanel_BeginHide_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance)
            {
                blockUnassign = false;
            }
        }

        // all the magic happens here
        [HarmonyPatch(typeof(CharacterStageLevelGainsPanel), "EnterStage")]
        internal static class CharacterStageLevelGainsPanel_EnterStage_Patch
        {
            internal static void Prefix(CharacterStageLevelGainsPanel __instance)
            {
                selectedClass = __instance.CharacterBuildingService.HeroCharacter.ClassesHistory[classesAndLevelsCount];
                __instance.CharacterBuildingService.UnassignLastClassLevel();
            }

            // replaces ICharacterBuildingService.GetLastAssignedClassAndLevel call with SolastaMultiClass.Patches.LevelUpSequencePatcher.GetHeroSelectedClassAndLevel
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getLastAssignedClassAndLevelMethod = typeof(ICharacterBuildingService).GetMethod("GetLastAssignedClassAndLevel");
                var getHeroSelectedClassAndLevelMethod = typeof(SolastaMultiClass.Patches.LevelUpSequencePatchers).GetMethod("GetHeroSelectedClassAndLevel");
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