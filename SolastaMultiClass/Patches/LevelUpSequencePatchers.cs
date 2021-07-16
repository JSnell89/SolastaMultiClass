using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class LevelUpSequencePatchers
    {        
        //
        // CHARACTER LEVEL UP SCREEN
        //

        // add the class selection stage panel to the Level Up screen
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "LoadStagePanels")]
        internal static class CharacterLevelUpScreen_LoadStagePanels_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, ref Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
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
                ___stagePanelsByName = stagePanelsByName;
            }
        }

        // bind the hero
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginShow")]
        internal static class CharacterLevelUpScreen_OnBeginShow_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance)
            {
                Models.LevelUpContext.SelectedHero = __instance.CharacterBuildingService.HeroCharacter;
            }     
        }

        // unbind the hero
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginHide")]
        internal static class CharacterLevelUpScreen_OnBeginHide_Patch
        {
            internal static void Postfix()
            {
                Models.LevelUpContext.SelectedHero = null;
            }
        }

        // removes the wizard spell book in case it was granted
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "DoAbort")]
        internal static class CharacterLevelUpScreen_DoAbort_Patch
        {
            internal static void Prefix(CharacterLevelUpScreen __instance)
            {
                Models.LevelUpContext.UngrantSpellbookIfRequired();
            }
        }

        //
        // CHARACTER BUILDING MANAGER
        //

        // ensure the level up process doesn't offer spells from a class not leveling up
        [HarmonyPatch(typeof(CharacterBuildingManager), "UpgradeSpellPointPools")]
        internal static class CharacterBuildingManager_UpgradeSpellPointPools_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance)
            {
                foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                {
                    string poolName = string.Empty;
                    CharacterClassDefinition characterClassDefinition;
                    int num;

                    if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class)
                    {
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out num);

                        // short circuit if the feature is for another class otherwise (change from native code)
                        if (spellRepertoire.SpellCastingClass != characterClassDefinition)
                            continue;

                        poolName = AttributeDefinitions.GetClassTag(characterClassDefinition, num);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                    {
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out num);
                        CharacterSubclassDefinition item = __instance.HeroCharacter.ClassesAndSubclasses[characterClassDefinition];

                        // short circuit if the feature is for another subclass (change from native code)
                        if (spellRepertoire.SpellCastingSubclass != characterClassDefinition)
                            continue;

                        poolName = AttributeDefinitions.GetSubclassTag(characterClassDefinition, num, item);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Race)
                    {
                        poolName = "02Race";
                    }
                    int maxPoints = 0;
                    if (__instance.HasAnyActivePoolOfType(HeroDefinitions.PointsPoolType.Cantrip) && __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools.ContainsKey(poolName))
                    {
                        maxPoints = __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools[poolName].MaxPoints;
                    }

                    var characterBuildingManagerType = typeof(CharacterBuildingManager);
                    var applyFeatureCastSpellMethod = characterBuildingManagerType.GetMethod("ApplyFeatureCastSpell", BindingFlags.NonPublic | BindingFlags.Instance);
                    var setPointPoolMethod = characterBuildingManagerType.GetMethod("SetPointPool", BindingFlags.NonPublic | BindingFlags.Instance);
                    var tempAcquiredCantripsNumberFieldInfo = characterBuildingManagerType.GetField("tempAcquiredCantripsNumber", BindingFlags.NonPublic | BindingFlags.Instance);
                    var tempAcquiredSpellsNumberFieldInfo = characterBuildingManagerType.GetField("tempAcquiredSpellsNumber", BindingFlags.NonPublic | BindingFlags.Instance);

                    tempAcquiredCantripsNumberFieldInfo.SetValue(__instance, 0);
                    tempAcquiredSpellsNumberFieldInfo.SetValue(__instance, 0);

                    //Make sure not to recurse indefinitely!  The call here is needed 
                    applyFeatureCastSpellMethod.Invoke(__instance, new object[] { spellRepertoire.SpellCastingFeature });

                    int tempCantrips = (int)tempAcquiredCantripsNumberFieldInfo.GetValue(__instance);
                    int tempSpells = (int)tempAcquiredSpellsNumberFieldInfo.GetValue(__instance);

                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Cantrip, poolName, tempCantrips + maxPoints });
                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Spell, poolName, tempSpells });
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterBuildingManager), "GetSpellFeature")]
        internal static class CharacterBuildingManager_GetSpellFeature_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance, string tag, ref FeatureDefinitionCastSpell __result)
            {
                FeatureDefinitionCastSpell featureDefinitionCastSpell = null;
                string str = tag;

                if (str.StartsWith("03Class"))
                {
                    str = tag.Substring(0, tag.Length - 2); // removes any levels from the tag examples are 03ClassRanger2, 03ClassRanger20. This is a bit lazy but no class will have a tag where the class name is only 1 character.  
                                                            // old Solasta code was str = "03Class"; which lead to getting the first spell feature from any class
                }
                else if (str.StartsWith("06Subclass"))
                {
                    str = tag.Substring(0, tag.Length - 2); // similar to above just with subclasses
                                                            // old Solasta code was str = "06Subclass"; which lead to getting the first spell feature from any subclass
                }

                Dictionary<string, List<FeatureDefinition>>.Enumerator enumerator = __instance.HeroCharacter.ActiveFeatures.GetEnumerator();

                try
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, List<FeatureDefinition>> current = enumerator.Current;
                        if (!current.Key.StartsWith(str))
                        {
                            continue;
                        }
                        List<FeatureDefinition>.Enumerator enumerator1 = current.Value.GetEnumerator();
                        try
                        {
                            while (enumerator1.MoveNext())
                            {
                                FeatureDefinition featureDefinition = enumerator1.Current;
                                if (!(featureDefinition is FeatureDefinitionCastSpell))
                                {
                                    continue;
                                }
                                featureDefinitionCastSpell = featureDefinition as FeatureDefinitionCastSpell;
                                __result = featureDefinitionCastSpell;
                                return false;
                            }
                        }
                        finally
                        {
                            ((IDisposable)enumerator1).Dispose();
                        }
                    }
                }
                finally
                {
                    ((IDisposable)enumerator).Dispose();
                }
                __result = featureDefinitionCastSpell;
                return false;
            }
        }

        // captures the desired class and ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "AssignClassLevel")]
        internal static class CharacterBuildingManager_AssignClassLevel_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance, CharacterClassDefinition classDefinition)
            {
                if (Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel)
                {
                    Models.LevelUpContext.SelectedClass = classDefinition;
                }
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "ClearWieldedConfigurations")]
        internal static class CharacterBuildingManager_ClearWieldedConfigurations_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantBaseEquipment")]
        internal static class CharacterBuildingManager_GrantBaseEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantFeatures")]
        internal static class CharacterBuildingManager_GrantFeatures_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "RemoveBaseEquipment")]
        internal static class CharacterBuildingManager_RemoveBaseEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignEquipment")]
        internal static class CharacterBuildingManager_UnassignEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignLastClassLevel")]
        internal static class CharacterBuildingManager_UnassignLastClassLevel_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        //
        // CHARACTER CLASS DEFINITION
        //

        [HarmonyPatch(typeof(CharacterClassDefinition), "FeatureUnlocks", MethodType.Getter)]
        internal static class CharacterClassDefinition_get_FeatureUnlocks_Patch
        {
            internal static void Postfix(CharacterClassDefinition __instance, ref List<FeatureUnlockByLevel> __result)
            {
                if (Models.LevelUpContext.LevelingUp && Models.LevelUpContext.SelectedClass != null)
                {
                    __result = Models.LevelUpContext.SelectClassFeaturesUnlock;
                }
            }
        }

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

        // hides de equipment panel group during a level up class selection
        public static bool MySetActive(bool show)
        {
            return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
        }

        // patches the method to hide the equipment panel group
        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "Refresh")]
        internal static class CharacterStageClassSelectionPanel_Refresh_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var setActiveFound = 0;
                var setActiveMethod = typeof(GameObject).GetMethod("SetActive");
                var mySetActiveMethod = typeof(LevelUpSequencePatchers).GetMethod("MySetActive");

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
                var getLastAssignedClassAndLevelCustomMethod = typeof(LevelUpSequencePatchers).GetMethod("GetLastAssignedClassAndLevelCustom");

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
        // CHARACTER STAGE SPELL SELECTION PANEL
        //

        // removes the ability to select spells of higher level than you should be able to when leveling up
        [HarmonyPatch(typeof(CharacterStageSpellSelectionPanel), "Refresh")]
        internal static class CharacterStageSpellSelectionPanel_Refresh_Patch
        {
            internal static void Postfix(CharacterStageSpellSelectionPanel __instance)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var characterStageSpellSelectionPanelType = typeof(CharacterStageSpellSelectionPanel);
                var spellsByLevelTableFieldInfo = characterStageSpellSelectionPanelType.GetField("spellsByLevelTable", BindingFlags.NonPublic | BindingFlags.Instance);
                var allTagsFieldInfo = characterStageSpellSelectionPanelType.GetField("allTags", BindingFlags.NonPublic | BindingFlags.Instance);
                var allTags = (List<string>)allTagsFieldInfo.GetValue(__instance);

                if (allTags == null)
                    return;

                string item = allTags[allTags.Count - 1];
                __instance.CharacterBuildingService.GetLastAssignedClassAndLevel(out CharacterClassDefinition characterClassDefinition, out int _);

                FeatureDefinitionCastSpell spellFeature = __instance.CharacterBuildingService.GetSpellFeature(item);

                // only need updates if for spell selection. this fixes an issue where Clerics were getting level 1 spells as cantrips
                if (spellFeature.SpellKnowledge != RuleDefinitions.SpellKnowledge.Selection && spellFeature.SpellKnowledge != RuleDefinitions.SpellKnowledge.Spellbook)
                    return;

                int count = __instance.CharacterBuildingService.HeroCharacter.ClassesAndLevels[characterClassDefinition]; // changed to use class level instead of hero level
                int highestSpellLevel = spellFeature.ComputeHighestSpellLevel(count);

                int accountForCantripsInt = spellFeature.SpellListDefinition.HasCantrips ? 1 : 0;

                UnityEngine.RectTransform spellsByLevelRect = (UnityEngine.RectTransform)spellsByLevelTableFieldInfo.GetValue(__instance);
                int currentChildCount = spellsByLevelRect.childCount;

                if (spellsByLevelRect != null && currentChildCount > highestSpellLevel + accountForCantripsInt)
                {
                    // deactivate the extra spell UI that can show up due to the original method using Character level instead of Class level
                    for (int i = highestSpellLevel + accountForCantripsInt; i < currentChildCount; i++)
                    {
                        var child = spellsByLevelRect.GetChild(i);
                        child?.gameObject?.SetActive(false);
                    }

                    // TODO: test if this is needed
                    LayoutRebuilder.ForceRebuildLayoutImmediate(spellsByLevelRect);
                }
            }
        }
    }
}