using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using static SolastaMultiClass.Models.Rules;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionPointPools;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionProficiencys;

namespace SolastaMultiClass.Patches
{
    internal static class LevelUpSequencePatchers
    {
        private static bool levelingUp = false;
        private static bool displayingClassPanel = false;
        private static int levelsCount = 0;
        private static bool requiresDeity = false;
        private static bool hasSpellbookGranted = false;
        private static int selectedClassIndex = -1;
        private static CharacterClassDefinition selectedClass = null;

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
                var deitySelectionStagePanelPrefab = stagePanelPrefabs[2];
                var classSelectionPanel = Gui.GetPrefabFromPool(classSelectionStagePanelPrefab, __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();
                var deitySelectionPanel = Gui.GetPrefabFromPool(deitySelectionStagePanelPrefab, __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();

                Dictionary<string, CharacterStagePanel> stagePanelsByName = new Dictionary<string, CharacterStagePanel>
                {
                    { "ClassSelection", classSelectionPanel }
                };
                var idx = 0;
                foreach (var stagePanelName in ___stagePanelsByName)
                {
                    stagePanelsByName.Add(stagePanelName.Key, stagePanelName.Value);
                    if (++idx == 1)
                    {
                        stagePanelsByName.Add("DeitySelection", deitySelectionPanel);
                    }
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
                selectedClass = null;

                requiresDeity = false;
                hasSpellbookGranted = false;

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
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginHide")]
        internal static class CharacterLevelUpScreen_OnBeginHide_Patch
        {
            internal static void Postfix()
            {
                levelingUp = false;
            }
        }

        // removes the wizard spell book in case it was granted
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "DoAbort")]
        internal static class CharacterLevelUpScreen_DoAbort_Patch
        {
            internal static void Prefix(CharacterLevelUpScreen __instance)
            {
                if (hasSpellbookGranted)
                {
                    var hero = __instance.CharacterBuildingService.HeroCharacter;
                    var item = new RulesetItemSpellbook(SolastaModApi.DatabaseHelper.ItemDefinitions.Spellbook);
                    hero.LoseItem(item);
                }    
            }
        }

        //
        // CHARACTER BUILDING MANAGER - must blocks any call to it while leveling up and displaying the class selection panel
        //

        // exclude some features that would normally be added to a level 1 character but should not be added to a multiclass character
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantFeatures")]
        internal static class CharacterBuildingManager_GrantFeatures_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures)
            {
                // ensures this doesn't get executed in the class panel level up screen
                if (levelingUp)
                {
                    if (displayingClassPanel)
                    {
                        return false;
                    }
                    else
                    {
                        if (__instance.HeroCharacter.ClassesHistory.Count > 1)
                        {
                            grantedFeatures.RemoveAll(feature => FeaturesToExcludeFromMulticlassLevels.Contains(feature));

                            // need to add logic to add extra skill points here
                        }
                    }
                }
                return true;
            }

            private static readonly FeatureDefinition[] FeaturesToExcludeFromMulticlassLevels = new FeatureDefinition[]
            {
                PointPoolClericSkillPoints,
                PointPoolFighterSkillPoints,
                PointPoolPaladinSkillPoints,
                PointPoolRangerSkillPoints,
                PointPoolRogueSkillPoints,
                PointPoolWizardSkillPoints,
                ProficiencyClericSavingThrow,
                ProficiencyFighterSavingThrow,
                ProficiencyPaladinSavingThrow,
                ProficiencyRangerSavingThrow,
                ProficiencyRogueSavingThrow,
                ProficiencyWizardSavingThrow,
            };
        }

        // captures the desired class / ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "AssignClassLevel")]
        internal static class CharacterBuildingManager_AssignClassLevel_Patch
        {
            internal static bool Prefix(CharacterClassDefinition classDefinition)
            {
                selectedClass = classDefinition;

                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "ClearWieldedConfigurations")]
        internal static class CharacterBuildingManager_ClearWieldedConfigurations_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantBaseEquipment")]
        internal static class CharacterBuildingManager_GrantBaseEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "RemoveBaseEquipment")]
        internal static class CharacterBuildingManager_RemoveBaseEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignEquipment")]
        internal static class CharacterBuildingManager_UnassignEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(levelingUp && displayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
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

        // flags displaying the class panel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnBeginShow")]
        internal static class CharacterStageClassSelectionPanel_OnBeginShow_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance, ref int ___selectedClass)
            {
                displayingClassPanel = true;

                ___selectedClass = selectedClassIndex;

                if (levelingUp)
                {
                    __instance.CommonData.AttackModesPanel?.Hide();
                    __instance.CommonData.PersonalityMapPanel?.RefreshNow();
                }
            }
        }

        // provides my own classLevel to CharacterStageClassSelectionPanel.FillClassFeatures and CharacterStageClassSelectionPanel.RefreshCharacter
        public static int GetClassLevel(RulesetCharacterHero hero)
        {
            hero.ClassesAndLevels.TryGetValue(selectedClass, out int classLevel);
            return classLevel == 0 ? 1 : classLevel;
        }

        // patches the method to get my own classLevel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "FillClassFeatures")]
        internal static class CharacterStageClassSelectionPanel_FillClassFeatures_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getHeroCharacterMethod = typeof(ICharacterBuildingService).GetMethod("get_HeroCharacter");
                var getClassLevelMethod = typeof(LevelUpSequencePatchers).GetMethod("GetClassLevel");
                var instructionsToBypass = 0;

                /*
                2 0006 ldarg.0
                3 0007 call     instance class ICharacterBuildingService CharacterStagePanel::get_CharacterBuildingService()
                4 000C callvirt instance class RulesetCharacterHero ICharacterBuildingService::get_HeroCharacter()
                5 0011 callvirt instance class [mscorlib]System.Collections.Generic.Dictionary`2<class CharacterClassDefinition, int32> RulesetCharacterHero::get_ClassesAndLevels()
                6 0016 ldarg.1
                7 0017 callvirt instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<class CharacterClassDefinition, int32>::get_Item(!0)
                8 001C stloc.1
                */

                foreach (var instruction in instructions)
                {
                    if (instructionsToBypass > 0)
                    {
                        instructionsToBypass--;
                    }
                    else if (instruction.Calls(getHeroCharacterMethod))
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

        // patches the method to get my own classLevel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "RefreshCharacter")]
        internal static class CharacterStageClassSelectionPanel_RefreshCharacter_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getHeroCharacterMethod = typeof(ICharacterBuildingService).GetMethod("get_HeroCharacter");
                var getClassLevelMethod = typeof(LevelUpSequencePatchers).GetMethod("GetClassLevel");
                var instructionsToBypass = 0;

                /*
                2 0006 ldarg.0
                3 0007 call     instance class ICharacterBuildingService CharacterStagePanel::get_CharacterBuildingService()
                4 000C callvirt instance class RulesetCharacterHero ICharacterBuildingService::get_HeroCharacter()
                5 0011 callvirt instance class [mscorlib]System.Collections.Generic.Dictionary`2<class CharacterClassDefinition, int32> RulesetCharacterHero::get_ClassesAndLevels()
                6 0016 ldarg.1
                7 0017 callvirt instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<class CharacterClassDefinition, int32>::get_Item(!0)
                8 001C stloc.1
                */

                foreach (var instruction in instructions)
                {
                    if (instructionsToBypass > 0)
                    {
                        instructionsToBypass--;
                    }
                    else if (instruction.Calls(getHeroCharacterMethod))
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

        // hides de equipment panel group during a level up class selection
        public static bool MySetActive(bool show)
        {
            return !(levelingUp && displayingClassPanel);
        }

        // patches the method to get my own class and level for level up
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "Refresh")]
        internal static class CharacterStageClassSelectionPanel_Refresh_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var setActiveFound = 0;
                var setActiveMethod = typeof(UnityEngine.GameObject).GetMethod("SetActive");
                var mySetActiveMethod = typeof(LevelUpSequencePatchers).GetMethod("MySetActive");

                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(setActiveMethod))
                    {
                        if (++setActiveFound == 4)
                        {
                            yield return new CodeInstruction(OpCodes.Call, mySetActiveMethod);
                        }
                        yield return instruction;
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

        // unflags displaying the class panel / determines if deity / spellbook are required, provides mod class/classLevel to level up gain stage
        public static void GetHeroSelectedClassAndLevel(ICharacterBuildingService characterBuildingService, out CharacterClassDefinition lastClassDefinition, out int level)
        {
            var hero = characterBuildingService.HeroCharacter;
            var classesAndLevels = hero.ClassesAndLevels;
            var requiresSpellbook = !classesAndLevels.ContainsKey(Wizard) && selectedClass == Wizard;

            displayingClassPanel = false;

            requiresDeity = hero.DeityDefinition == null && selectedClass.RequiresDeity && !classesAndLevels.ContainsKey(Cleric) && !classesAndLevels.ContainsKey(Paladin);

            if (requiresSpellbook && !hasSpellbookGranted)
            {
                var item = new RulesetItemSpellbook(SolastaModApi.DatabaseHelper.ItemDefinitions.Spellbook);
                hero.GrantItem(item, false);
                hasSpellbookGranted = true;
            }

            lastClassDefinition = selectedClass;
            level = levelsCount;
        }

        // patches the method to get my own class and level for level up
        [HarmonyPatch(typeof(CharacterStageLevelGainsPanel), "EnterStage")]
        internal static class CharacterStageLevelGainsPanel_EnterStage_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getLastAssignedClassAndLevelMethod = typeof(ICharacterBuildingService).GetMethod("GetLastAssignedClassAndLevel");
                var getHeroSelectedClassAndLevelMethod = typeof(LevelUpSequencePatchers).GetMethod("GetHeroSelectedClassAndLevel");

                /*
                0 0000 ldarg.0
                1 0001 call     instance class ICharacterBuildingService CharacterStagePanel::get_CharacterBuildingService()
                2 0006 ldarg.0
                3 0007 ldflda   class CharacterClassDefinition CharacterStageLevelGainsPanel::lastGainedClassDefinition
                4 000C ldarg.0
                5 000D ldflda   int32 CharacterStageLevelGainsPanel::lastGainedClassLevel
                6 0012 callvirt instance void ICharacterBuildingService::GetLastAssignedClassAndLevel(class CharacterClassDefinition&, int32&)
                */

                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(getLastAssignedClassAndLevelMethod))
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

        //
        // CHARACTER STAGE DEITY SELECTION PANEL
        //

        [HarmonyPatch(typeof(CharacterStageDeitySelectionPanel), "UpdateRelevance")]
        internal static class CharacterEditionScreen_UpdateRelevance_Patch
        {
            internal static void Postfix(ref bool ___isRelevant)
            {
                ___isRelevant = (levelingUp && requiresDeity);
            }
        }

        //
        // CHARACTER STAGE SUB CLASS SELECTION PANEL
        //

        [HarmonyPatch(typeof(CharacterStageSubclassSelectionPanel), "UpdateRelevance")]
        internal static class CharacterStageSubclassSelectionPanel_UpdateRelevance_Patch
        {
            internal static void Postfix(ref bool ___isRelevant)
            {
                if (levelingUp && requiresDeity)
                {
                    ___isRelevant = false;
                }
            }
        }
    }
}