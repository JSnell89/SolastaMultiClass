using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterStageSelectionPanelPatchers
    {
        //
        // CHARACTER STAGE CLASS SELECTION PANEL
        // 

        // flags displaying the class panel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnBeginShow")]
        internal static class CharacterStageClassSelectionPanel_OnBeginShow_Patch
        {
            internal static void Prefix(CharacterStageClassSelectionPanel __instance, ref int ___selectedClass, List<CharacterClassDefinition> ___compatibleClasses, RectTransform ___classesTable)
            {
                Models.LevelUpContext.DisplayingClassPanel = true;

                if (Models.LevelUpContext.LevelingUp)
                {
                    Models.InOutRules.EnumerateHeroAllowedClassDefinitions(Models.LevelUpContext.SelectedHero, ___compatibleClasses, ref ___selectedClass);
                    __instance.CommonData.AttackModesPanel?.RefreshNow();
                    __instance.CommonData.PersonalityMapPanel?.RefreshNow();
                }
                else
                {
                    ___compatibleClasses.Sort((a, b) => a.FormatTitle().CompareTo(b.FormatTitle()));
                    ___selectedClass = -1;
                }
                ___classesTable.pivot = new Vector2(0.5f, 1.0f);
            }
        }

        // patches the method to get my own classLevel
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "FillClassFeatures")]
        internal static class CharacterStageClassSelectionPanel_FillClassFeatures_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getHeroCharacterMethod = typeof(ICharacterBuildingService).GetMethod("get_HeroCharacter");
                var getClassLevelMethod = typeof(Models.LevelUpContext).GetMethod("GetClassLevel");
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
                var getClassLevelMethod = typeof(Models.LevelUpContext).GetMethod("GetClassLevel");
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

        // hides the equipment panel group on level up
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "Refresh")]
        internal static class CharacterStageClassSelectionPanel_Refresh_Patch
        {
            public static bool SetActive(bool show) // need to keep this parameter here otherwise will FUP the stack
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var setActiveFound = 0;
                var setActiveMethod = typeof(GameObject).GetMethod("SetActive");
                var mySetActiveMethod = typeof(CharacterStageSelectionPanelPatchers.CharacterStageClassSelectionPanel_Refresh_Patch).GetMethod("SetActive");

                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(setActiveMethod))
                    {
                        if (++setActiveFound == 4)
                        {
                            yield return new CodeInstruction(OpCodes.Call, mySetActiveMethod);
                        }
                    }
                    yield return instruction;
                }
            }
        }

        //
        // CHARACTER STAGE LEVEL GAINS PANEL
        //

        // patches the method to get my own class and level for level up
        [HarmonyPatch(typeof(CharacterStageLevelGainsPanel), "EnterStage")]
        internal static class CharacterStageLevelGainsPanel_EnterStage_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getLastAssignedClassAndLevelMethod = typeof(ICharacterBuildingService).GetMethod("GetLastAssignedClassAndLevel");
                var getLastAssignedClassAndLevelCustomMethod = typeof(Models.LevelUpContext).GetMethod("GetLastAssignedClassAndLevel");

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
                        yield return new CodeInstruction(OpCodes.Call, getLastAssignedClassAndLevelCustomMethod);
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
                if (Models.LevelUpContext.LevelingUp)
                {
                    ___isRelevant = Models.LevelUpContext.RequiresDeity;
                }
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
                if (Models.LevelUpContext.LevelingUp && Models.LevelUpContext.RequiresDeity)
                {
                    ___isRelevant = false;
                }
            }
        }

        //
        // CHARACTER STAGE FIGHTING STYLE SELECTION PANEL
        //

        public static int previouslySelectedFightingStyle = -1;
        public static int newlySelectedFightingStyle = -1;

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
            public static void UntrainPreviouslySelectedFightStyle(ICharacterBuildingService __instance)
            {
                if (previouslySelectedFightingStyle > 0)
                {
                    __instance.UntrainLastFightingStyle();
                }
                previouslySelectedFightingStyle = newlySelectedFightingStyle;
            }

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var untrainLastFightingStyleMethod = typeof(ICharacterBuildingService).GetMethod("UntrainLastFightingStyle");
                var untrainPreviouslySelectedFightStyleMethod = typeof(CharacterStageFightingStyleSelectionPanel_Refresh_Patch).GetMethod("UntrainPreviouslySelectedFightStyle");

                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(untrainLastFightingStyleMethod))
                    {
                        yield return new CodeInstruction(OpCodes.Call, untrainPreviouslySelectedFightStyleMethod); // stack will have the game building service instance
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
            public static void AssignSelectedFightingStyle(CharacterStageFightingStyleSelectionPanel __instance, int selectedFightingStyle)
            {
                newlySelectedFightingStyle = selectedFightingStyle;
                AccessTools.Field(__instance.GetType(), "selectedFightingStyle").SetValue(__instance, selectedFightingStyle);
            }

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var selectedFightingStyleField = AccessTools.Field(typeof(CharacterStageFightingStyleSelectionPanel), "selectedFightingStyle");
                var AssignSelectedFightingStyleMethod = typeof(CharacterStageFightingStyleSelectionPanel_OnFightingStyleValueChangedCb_Patch).GetMethod("AssignSelectedFightingStyle");

                foreach (var instruction in instructions)
                {
                    if (instruction.StoresField(selectedFightingStyleField))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AssignSelectedFightingStyleMethod); // stack will have the instance and an int
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }

        //
        // CHARACTER STAGE SPELL SELECTION PANEL
        //

        // removes the ability to select spells of higher level than you should be able to when leveling up
        [HarmonyPatch(typeof(CharacterStageSpellSelectionPanel), "Refresh")]
        internal static class CharacterStageSpellSelectionPanel_Refresh_Patch
        {
            internal static void Postfix(CharacterStageSpellSelectionPanel __instance, List<string> ___allTags, RectTransform ___spellsByLevelTable, RectTransform ___levelButtonsTable)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                if (___allTags == null)
                    return;

                var tag = ___allTags[___allTags.Count - 1];
                var featureDefinitionCastSpell = __instance.CharacterBuildingService.GetSpellFeature(tag);

                // only need updates if spells are selected
                if (featureDefinitionCastSpell.SpellKnowledge != RuleDefinitions.SpellKnowledge.Selection && featureDefinitionCastSpell.SpellKnowledge != RuleDefinitions.SpellKnowledge.Spellbook)
                    return;

                // changes to use class level instead of hero level
                __instance.CharacterBuildingService.GetLastAssignedClassAndLevel(out CharacterClassDefinition characterClassDefinition, out int _);
                int classLevel = __instance.CharacterBuildingService.HeroCharacter.ClassesAndLevels[characterClassDefinition];
                int highestSpellLevel = featureDefinitionCastSpell.ComputeHighestSpellLevel(classLevel);
                int accountForCantripsInt = featureDefinitionCastSpell.SpellListDefinition.HasCantrips ? 1 : 0;

                // patches the spell level buttons to be hidden if no spells available at that level
                if (___levelButtonsTable != null && ___levelButtonsTable.childCount > highestSpellLevel + accountForCantripsInt)
                {
                    for (int i = highestSpellLevel + accountForCantripsInt; i < ___levelButtonsTable.childCount; i++)
                    {
                        var child = ___levelButtonsTable.GetChild(i);
                        child.gameObject.SetActive(false);
                    }
                }

                // patches the spell panel to be hidden if no spells available at that level
                if (___spellsByLevelTable != null && ___spellsByLevelTable.childCount > highestSpellLevel + accountForCantripsInt)
                {  
                    for (int i = highestSpellLevel + accountForCantripsInt; i < ___spellsByLevelTable.childCount; i++)
                    {
                        var child = ___spellsByLevelTable.GetChild(i);
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}