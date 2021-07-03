using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using static SolastaMultiClass.Models.Deity;
using static SolastaMultiClass.Models.Rules;

namespace SolastaMultiClass.Patches
{
    internal static class LevelUpSequencePatchers
    {
        internal static bool levelingUp = false;
        internal static bool displayingClassPanel = false;
        internal static int levelsCount = 0;
        internal static int selectedClassIndex = -1;
        internal static CharacterClassDefinition selectedClass = null;

        //
        // CHARACTER LEVEL UP SCREEN
        //

        // add the class selection stage panel to the Level Up screen
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

        // flags leveling up and filter compatible classes per rules
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginShow")]
        internal static class CharacterLevelUpScreen_OnBeginShow_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
            {
                var hero = __instance.CharacterBuildingService.HeroCharacter;

                levelingUp = true;
                levelsCount = hero.ClassesHistory.Count;

                // filter the available classes per multi class in/out rules
                if (hero.ClassesHistory.Count == 0)
                {
                    selectedClassIndex = -1;
                }
                else
                {
                    var classSelectionPanel = (CharacterStageClassSelectionPanel)___stagePanelsByName["ClassSelection"];
                    var compatibleClasses = (List<CharacterClassDefinition>)AccessTools.Field(classSelectionPanel.GetType(), "compatibleClasses").GetValue(classSelectionPanel);
                    var allowedClasses = GetHeroAllowedClassDefinitions(hero);

                    compatibleClasses.Clear();
                    compatibleClasses.AddRange(allowedClasses);
                    selectedClassIndex = allowedClasses.IndexOf(hero.ClassesHistory[hero.ClassesHistory.Count - 1]);
                }

            }     
        }

        // unflags leveling up
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnEndHide")]
        internal static class CharacterLevelUpScreen_OnEndHide_Patch
        {
            internal static void Postfix()
            {
                levelingUp = false;
            }
        }

        //
        // CHARACTER BUILDING MANAGER - must block any call to it while leveling up and displaying the class selection panel
        //

        // capture the desired class but ensure rest doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "AssignClassLevel")]
        internal static class CharacterBuildingManager_AssignClassLevel_Patch
        {
            internal static bool Prefix(CharacterClassDefinition classDefinition)
            {
                selectedClass = classDefinition;
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensure this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "ClearWieldedConfigurations")]
        internal static class CharacterBuildingManager_ClearWieldedConfigurations_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensure this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantFeatures")]
        internal static class CharacterBuildingManager_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensure this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "RemoveBaseEquipment")]
        internal static class CharacterBuildingManager_RemoveBaseEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensure this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignEquipment")]
        internal static class CharacterBuildingManager_UnassignEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensure this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignLastClassLevel")]
        internal static class CharacterBuildingManager_UnassignLastClassLevel_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        //
        // CHARACTER STAGE CLASS SELECTION PANEL
        //

        public static int GetClassLevel(RulesetCharacterHero hero)
        {
            hero.ClassesAndLevels.TryGetValue(selectedClass, out int classLevel);
            return classLevel == 0 ? 1 : classLevel;
        }

        // enables the trap on CharacterBuildingManager.UnassignLastClassLevel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnBeginShow")]
        internal static class CharacterStageClassSelectionPanel_OnBeginShow_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance, ref int ___selectedClass)
            {
                displayingClassPanel = true;

                ___selectedClass = selectedClassIndex;

                if (levelingUp)
                {
                    __instance.CommonData.AttackModesPanel.Hide();
                    __instance.CommonData.PersonalityMapPanel.RefreshNow();
                }
            }
        }

        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "FillClassFeatures")]
        internal static class CharacterStageClassSelectionPanel_FillClassFeatures_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var enumerateActiveFeaturesMethod = typeof(ICharacterBuildingService).GetMethod("get_HeroCharacter");
                var getClassLevelMethod = typeof(LevelUpSequencePatchers).GetMethod("GetClassLevel");
                var instructionsToBypass = 0;

                foreach (var instruction in instructions)
                {
                    if (instructionsToBypass > 0)
                    {
                        instructionsToBypass--;
                    }
                    else if (instruction.Calls(enumerateActiveFeaturesMethod))
                    {
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Call, getClassLevelMethod);
                        instructionsToBypass = 3;
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "RefreshCharacter")]
        internal static class CharacterStageClassSelectionPanel_RefreshCharacter_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var enumerateActiveFeaturesMethod = typeof(ICharacterBuildingService).GetMethod("get_HeroCharacter");
                var getClassLevelMethod = typeof(LevelUpSequencePatchers).GetMethod("GetClassLevel");
                var instructionsToBypass = 0;

                foreach (var instruction in instructions)
                {
                    if (instructionsToBypass > 0)
                    {
                        instructionsToBypass--;
                    }
                    else if (instruction.Calls(enumerateActiveFeaturesMethod))
                    {
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Call, getClassLevelMethod);
                        instructionsToBypass = 3;
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }

        //
        // CHARACTER STAGE LEVEL GAINS PANEL
        //

        // called by CharacterStageLevelGainsPanel.EnterStage with the transpiler injection
        public static void GetHeroSelectedClassAndLevel(out CharacterClassDefinition lastClassDefinition, out int level)
        {
            lastClassDefinition = selectedClass;
            level = levelsCount;
            displayingClassPanel = false;
            selectedClass = null;
        }

        // patches the stage to get my own class selection
        [HarmonyPatch(typeof(CharacterStageLevelGainsPanel), "EnterStage")]
        internal static class CharacterStageLevelGainsPanel_EnterStage_Patch
        {
            // replaces ICharacterBuildingService.GetLastAssignedClassAndLevel call with SolastaMultiClass.Patches.LevelUpSequencePatcher.GetHeroSelectedClassAndLevel
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getLastAssignedClassAndLevelMethod = typeof(ICharacterBuildingService).GetMethod("GetLastAssignedClassAndLevel");
                var getHeroSelectedClassAndLevelMethod = typeof(LevelUpSequencePatchers).GetMethod("GetHeroSelectedClassAndLevel");
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
                        instructionsToBypass--;
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

        //// assigns a deity if moving ahead on workflow
        //[HarmonyPatch(typeof(CharacterEditionScreen), "OnNextCb")]
        //internal static class CharacterEditionScreen_OnNextCb_Patch
        //{
        //    internal static void Postfix()
        //    {
        //        if (levelingUp && !displayingClassPanel)
        //        {
        //            AssignDeityIfRequired();
        //        }
        //    }
        //}
    }
}