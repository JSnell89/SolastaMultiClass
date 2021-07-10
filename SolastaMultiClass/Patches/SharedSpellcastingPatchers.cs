using HarmonyLib;
using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static FeatureDefinitionCastSpell;

namespace SolastaMultiClass.Patches
{
    class SharedSpellcastingPatchers
    {
        [HarmonyPatch(typeof(CharacterActionSpendSpellSlot), "ExecuteImpl")]
        internal static class CharacterActionSpendSpellSlot_ExecuteImpl_Patch
        {
            internal static void Postfix(CharacterActionSpendSpellSlot __instance)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var allSpellRepertoires = __instance.ActionParams.ActingCharacter.RulesetCharacter.SpellRepertoires;
                if (allSpellRepertoires.Count < 2 || __instance.ActionParams.SpellRepertoire.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                RuleDefinitions.RechargeRate rechargeRateToSpendSlotsFrom = __instance.ActionParams.SpellRepertoire.SpellCastingFeature.SlotsRecharge;
                var additionalSpellRepetoriesToSpendSlotsFrom = allSpellRepertoires.Where(sr => sr != __instance.ActionParams.SpellRepertoire && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
                foreach (var spellRepetoire in additionalSpellRepetoriesToSpendSlotsFrom)
                    spellRepetoire.SpendSpellSlot(__instance.ActionParams.IntParameter);
            }
        }

        [HarmonyPatch(typeof(RulesetCharacter), "SpendSpellSlot")]
        internal static class RulesetCharacter_SpendSpellSlot_Patch
        {
            internal static void Postfix(RulesetCharacter __instance, RulesetEffectSpell activeSpell)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var allSpellRepertoires = __instance.SpellRepertoires;
                if (allSpellRepertoires.Count < 2 || activeSpell.SpellRepertoire.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                RuleDefinitions.RechargeRate rechargeRateToSpendSlotsFrom = activeSpell.SpellRepertoire.SpellCastingFeature.SlotsRecharge;
                var additionalSpellRepetoriesToSpendSlotsFrom = allSpellRepertoires.Where(sr => sr != activeSpell.SpellRepertoire && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
                foreach (var spellRepetoire in additionalSpellRepetoriesToSpendSlotsFrom)
                    spellRepetoire.SpendSpellSlot(activeSpell.SlotLevel);
            }
        }

        [HarmonyPatch(typeof(RulesetCharacter), "UsePower")]
        internal static class RulesetCharacter_UsePower_Patch
        {
            internal static void Postfix(RulesetCharacter __instance, RulesetUsablePower usablePower)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var allSpellRepertoires = __instance.SpellRepertoires;
                if (allSpellRepertoires.Count < 2 || usablePower.PowerDefinition.RechargeRate != RuleDefinitions.RechargeRate.SpellSlot || usablePower.PowerDefinition.SpellcastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                RuleDefinitions.RechargeRate rechargeRateToSpendSlotsFrom = usablePower.PowerDefinition.SpellcastingFeature.SlotsRecharge;
                var additionalSpellRepetoriesToSpendSlotsFrom = allSpellRepertoires.Where(sr => sr.SpellCastingFeature != usablePower.PowerDefinition.SpellcastingFeature && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
                foreach (var spellRepetoire in additionalSpellRepetoriesToSpendSlotsFrom)
                    spellRepetoire.SpendSpellSlot(spellRepetoire.GetLowestAvailableSlotLevel()); //Theoretically if we've done this correctly the lowest slot in the other repertoires will be the same as what the power used from the initial repetoire
            }
        }

        // This fixes the case where if you multiclass a caster that selects spells at level up (e.g. Wizard)
        // The Solasta engine will have you select spells even when you are leveling up another class (they don't get saved properly but seem to break some things)
        // This also fixes selecting spells with these caster types when multiclassing back into them.
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

                        //Short circuit if the feature is for another class otherwise (change from native code)
                        if (spellRepertoire.SpellCastingClass != characterClassDefinition)
                            continue;

                        poolName = AttributeDefinitions.GetClassTag(characterClassDefinition, num);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                    {
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out num);
                        CharacterSubclassDefinition item = __instance.HeroCharacter.ClassesAndSubclasses[characterClassDefinition];

                        //Short circuit if the feature is for another subclass (change from native code)
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

                    //Yay reflection to call private methods/use private fields
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

        [HarmonyPatch(typeof(RulesetSpellRepertoire), "MaxSpellLevelOfSpellCastingLevel", MethodType.Getter)]
        internal static class RulesetSpellRepertoire_MaxSpellLevelOfSpellCastingLevel_Getter_Patch
        {
            internal static void Postfix(RulesetSpellRepertoire __instance, ref int __result)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var heroWithSpellRepetoire = GetHero(__instance.CharacterName);

                // don't bother doing fancy work if there aren't multiple spell repertoires that are shared (multiple long rest spell features).
                if (heroWithSpellRepetoire != null && heroWithSpellRepetoire.SpellRepertoires.Where(sr => sr.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest).Count() > 1)
                {
                    int casterLevel = (int)Math.Floor(GetCasterLevelForGivenLevel(heroWithSpellRepetoire.ClassesAndLevels, heroWithSpellRepetoire.ClassesAndSubclasses));//Multiclassing always rounds down caster level

                    if (__instance.SpellCastingFeature == null)
                    {
                        Trace.LogWarning("Invalid SpellCastingFeature in RulesetSpellRepertoire.ComputeSpellSlots()");
                        return;
                    }
                    if (__instance.SpellCastingLevel < 1 || __instance.SpellCastingFeature != null && __instance.SpellCastingLevel > __instance.SpellCastingFeature.SlotsPerLevels.Count - 1)
                    {
                        Trace.LogWarning("Invalid spellcasting level in RulesetSpellRepertoire.ComputeSpellSlots()");
                        return;
                    }
                    if (__instance.SpellCastingFeature == null)
                    {
                        return;
                    }
                    FeatureDefinitionCastSpell.SlotsByLevelDuplet item = FullCastingSlots[casterLevel - 1];
                    if (item == null || item.Slots == null || item.Slots.Count == 0)
                    {
                        Trace.LogWarning("Invalid duplet in RulesetSpellRepertoire.ComputeSpellSlots()");
                        return;
                    }
                    int num = item.Slots.IndexOf(0); // should revisit for Warlocks - something like: int num = item.Slots.FindLastIndex(i => i > 0) + 1;//Switch to Last non-zero index (plus 1 since arrays start a 0)
                    if (num == -1)
                    {
                        num = (item.Slots.Count > 0 ? item.Slots.Count : __instance.SpellCastingFeature.SpellListDefinition.MaxSpellLevel);
                    }
                    __result = num;
                }
            }
        }

        [HarmonyPatch(typeof(RulesetSpellRepertoire), "ComputeSpellSlots")]
        internal static class RulesetSpellRepertoire_ComputeSpellSlots_Patch
        {
            internal static void Postfix(RulesetSpellRepertoire __instance, List<FeatureDefinition> spellCastingAffinities)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                // don't do anything for non-Long rest classes as they combine in different ways
                if (__instance.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                // if combined with shared spell slot usage from the other patches we want every spell feature to have the number of spell slots for the total caster level of the character
                var heroWithSpellRepetoire = GetHero(__instance.CharacterName);

                // don't bother doing extra work if there aren't multiple spell repertoires that are shared (multiple long rest spell features)
                if (heroWithSpellRepetoire == null || heroWithSpellRepetoire.SpellRepertoires.Where(sr => sr.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest).Count() < 2)
                    return;

                int maxSpellLevel = __instance.MaxSpellLevelOfSpellCastingLevel;
                int casterLevel = (int)Math.Floor(GetCasterLevelForGivenLevel(heroWithSpellRepetoire.ClassesAndLevels, heroWithSpellRepetoire.ClassesAndSubclasses));

                var currentInstanceSpellsSlotCapacities = (Dictionary<int, int>)AccessTools.Field(__instance.GetType(), "spellsSlotCapacities").GetValue(__instance);
                var legacyAvailableSpellsSlots = (Dictionary<int, int>)AccessTools.Field(__instance.GetType(), "legacyAvailableSpellsSlots").GetValue(__instance);
                var usedSpellsSlots = (Dictionary<int, int>)AccessTools.Field(__instance.GetType(), "usedSpellsSlots").GetValue(__instance);
                currentInstanceSpellsSlotCapacities.Clear();

                for (int i = 0; i < maxSpellLevel; i++)
                {
                    currentInstanceSpellsSlotCapacities[i + 1] = FullCastingSlots[casterLevel - 1].Slots[i]; //The real change right here

                    // I believe this is just to properly handle saves between patches, theoretically it is needed for higher level slots for MC saves between patches as well
                    if (legacyAvailableSpellsSlots.ContainsKey(i + 1))
                    {
                        usedSpellsSlots.Add(i + 1, currentInstanceSpellsSlotCapacities[i + 1] - legacyAvailableSpellsSlots[i + 1]);
                        legacyAvailableSpellsSlots.Remove(i + 1);
                    }
                }

                // seems to be new features that give extra spell slots
                if (spellCastingAffinities != null && spellCastingAffinities.Count > 0)
                {
                    foreach (FeatureDefinition spellCastingAffinity in spellCastingAffinities)
                    {
                        foreach (AdditionalSlotsDuplet additionalSlot in ((ISpellCastingAffinityProvider)spellCastingAffinity).AdditionalSlots)
                        {
                            if (!currentInstanceSpellsSlotCapacities.ContainsKey(additionalSlot.SlotLevel))
                            {
                                currentInstanceSpellsSlotCapacities[additionalSlot.SlotLevel] = additionalSlot.SlotsNumber;
                            }
                            else
                            {
                                Dictionary<int, int> item = currentInstanceSpellsSlotCapacities;
                                int slotLevel = additionalSlot.SlotLevel;
                                item[slotLevel] = item[slotLevel] + additionalSlot.SlotsNumber;
                            }
                        }
                    }
                }

                // now update all other long rest spellrepetoires to have the same spell slots that we just calculated.
                foreach (var spellRepetoire in heroWithSpellRepetoire.SpellRepertoires.Where(spellRep => spellRep.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest))
                {
                    for (int i = 0; i < maxSpellLevel; i++)
                    {
                        var spellSlots = (Dictionary<int, int>)AccessTools.Field(spellRepetoire.GetType(), "spellsSlotCapacities").GetValue(spellRepetoire);
                        spellSlots[i + 1] = currentInstanceSpellsSlotCapacities[i + 1];
                    }
                    spellRepetoire.RepertoireRefreshed?.Invoke(spellRepetoire);
                }

                RulesetSpellRepertoire.RepertoireRefreshedHandler repertoireRefreshed = __instance.RepertoireRefreshed;
                if (repertoireRefreshed == null)
                {
                    return;
                }
                repertoireRefreshed(__instance);
            }
        }

        // Removes the ability to select spells of higher level than you should be able to when leveling up
        // This doesn't fix the 'Auto' choices however.
        // Essentially the only change that is needed to use class level instead of hero level but requires a whole bunch of postfix code to do so :)
        [HarmonyPatch(typeof(CharacterStageSpellSelectionPanel), "Refresh")]
        internal static class CharacterStageSpellSelectionPanel_Refresh_Patch
        {
            internal static void Postfix(CharacterStageSpellSelectionPanel __instance)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var characterStageSpellSelectionPanelType = typeof(CharacterStageSpellSelectionPanel);
                // var currentLearnStepFieldInfo = charBMType.GetField("currentLearnStep", BindingFlags.NonPublic | BindingFlags.Instance);
                var allTagsFieldInfo = characterStageSpellSelectionPanelType.GetField("allTags", BindingFlags.NonPublic | BindingFlags.Instance);
                var spellsByLevelTableFieldInfo = characterStageSpellSelectionPanelType.GetField("spellsByLevelTable", BindingFlags.NonPublic | BindingFlags.Instance);

                // int currentLearnStep = (int)currentLearnStepFieldInfo.GetValue(__instance);
                List<string> allTags = (List<string>)allTagsFieldInfo.GetValue(__instance);

                if (allTags == null)
                    return;

                string item = allTags[allTags.Count - 1];
                __instance.CharacterBuildingService.GetLastAssignedClassAndLevel(out CharacterClassDefinition characterClassDefinition, out int _);

                FeatureDefinitionCastSpell spellFeature = __instance.CharacterBuildingService.GetSpellFeature(item);

                // only need updates if for spell selection.  This fixes an issue where Clerics were getting level 1 spells as cantrips :).
                if (spellFeature.SpellKnowledge != RuleDefinitions.SpellKnowledge.Selection && spellFeature.SpellKnowledge != RuleDefinitions.SpellKnowledge.Spellbook)
                    return;

                int count = __instance.CharacterBuildingService.HeroCharacter.ClassesAndLevels[characterClassDefinition]; //Changed to use class level instead of hero level
                int highestSpellLevel = spellFeature.ComputeHighestSpellLevel(count);

                int accountForCantripsInt = spellFeature.SpellListDefinition.HasCantrips ? 1 : 0;

                UnityEngine.RectTransform spellsByLevelRect = (UnityEngine.RectTransform)spellsByLevelTableFieldInfo.GetValue(__instance);
                int currentChildCount = spellsByLevelRect.childCount;

                if (spellsByLevelRect != null && currentChildCount > highestSpellLevel + accountForCantripsInt)
                {
                    // deactivate the extra spell UI that can show up do to the original method using Character level instead of Class level
                    for (int i = highestSpellLevel + accountForCantripsInt; i < currentChildCount; i++)
                    {
                        var child = spellsByLevelRect.GetChild(i);
                        child?.gameObject?.SetActive(false);
                    }

                    // TODO test if this is needed
                    LayoutRebuilder.ForceRebuildLayoutImmediate(spellsByLevelRect);
                }
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
                    str = tag.Substring(0, tag.Length - 2); // removes any levels from the tag examples are 03ClassRanger2, 03ClassRanger20.  This is a bit lazy but no class will have a tag where the class name is only 1 character.  
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

        [HarmonyPatch(typeof(SpellRepertoirePanel), "Bind")]
        internal static class SpellRepertoirePanel_Bind_Patch
        {
            internal static void Postfix(SpellRepertoirePanel __instance)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                //It would be nice to short-circuit here but since I'm setting the visibility of the subitems it needs to be redone every time since these subitems seem to be shared across 'tabs'
                //if (__instance.SpellRepertoire.SpellCastingFeature.SpellReadyness != RuleDefinitions.SpellReadyness.Prepared)
                //    return;

                //This may not work for subclasses that have 'Prepared' spells, but I don't think any do.
                CharacterClassDefinition characterClassDefinition = __instance.SpellRepertoire.SpellCastingClass;

                var hero = __instance.GuiCharacter.RulesetCharacterHero;
                if (hero == null || characterClassDefinition == null)
                    return;

                // Hide sorcery points if required
                if (__instance.SpellRepertoire.SpellCastingFeature.SpellReadyness == RuleDefinitions.SpellReadyness.Prepared)
                {
                    var sorceryPointsBox = (RectTransform)AccessTools.Field(__instance.GetType(), "sorceryPointsBox").GetValue(__instance);
                    var sorceryPointsLabel = (GuiLabel)AccessTools.Field(__instance.GetType(), "sorceryPointsLabel").GetValue(__instance);
                    sorceryPointsBox.gameObject.SetActive(false);
                    sorceryPointsLabel.gameObject.SetActive(false);
                }

                var currentCharacterClassAsDictionary = new Dictionary<CharacterClassDefinition, int>() { { characterClassDefinition, hero.ClassesAndLevels[characterClassDefinition] } };
                var currentCharacterSubclassAsDictionary = new Dictionary<CharacterClassDefinition, CharacterSubclassDefinition>() { { characterClassDefinition, hero.ClassesAndSubclasses.ContainsKey(characterClassDefinition) ? hero.ClassesAndSubclasses[characterClassDefinition] : null } };

                // Bit of an odd case here.  You actually want to prepare spells of the next caster level if you are part way into it.
                // E.g. a Level 5 paladin should be able to prepare level 2 spells like a 3rd level full caster, even though they are only actually a level 2.5 caster.
                int currentClassCasterPrepareSpellsLevel = (int)Math.Ceiling(GetCasterLevelForGivenLevel(currentCharacterClassAsDictionary, currentCharacterSubclassAsDictionary));

                int maxLevelOfSpellcastingForClass = currentClassCasterPrepareSpellsLevel % 2 == 0 ? currentClassCasterPrepareSpellsLevel / 2 : (currentClassCasterPrepareSpellsLevel + 1) / 2;

                //It would be nice to short-circuit here but since I'm setting the visibility of the subitems it needs to be redone every time
                //if (maxSpellLevelOfSpellCastingLevelForHero == maxLevelOfSpellcastingForClass)
                //    return;

                var spellRepertoirePanelType = typeof(SpellRepertoirePanel);
                var spellsByLevelTableFieldInfo = spellRepertoirePanelType.GetField("spellsByLevelTable", BindingFlags.NonPublic | BindingFlags.Instance);
                UnityEngine.RectTransform spellsByLevelRect = (UnityEngine.RectTransform)spellsByLevelTableFieldInfo.GetValue(__instance);

                int childCount = spellsByLevelRect.childCount;
                int accountForCantripsInt = __instance.SpellRepertoire.SpellCastingFeature.SpellListDefinition.HasCantrips ? 1 : 0;

                for (int i = 0; i < childCount; i++)
                {
                    Transform transforms = spellsByLevelRect.GetChild(i);
                    for (int k = 0; k < transforms.childCount; k++)
                    {
                        var child = transforms.GetChild(k);
                        //Don't hide the spell slot status so people can see how many slots they have even if they don't have spells of that level
                        if (child.TryGetComponent(typeof(SlotStatusTable), out Component _))
                            continue;
                        if (i > (maxLevelOfSpellcastingForClass + accountForCantripsInt) - 1 && !Main.Settings.TurnOffSpellPreparationRestrictions) //The toggle option needs to be here otherwise if you already had it on and opened a spell list it will mess things up potentially for all spellcasters
                            child.gameObject.SetActive(false);
                        else
                            child.gameObject.SetActive(true); //Need to set to true because when switching tabs the false from one spellcasting class is carried over.
                    }
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(spellsByLevelRect);
            }
        }

        private static double GetCasterLevelForGivenLevel(Dictionary<CharacterClassDefinition, int> classesAndLevels, Dictionary<CharacterClassDefinition, CharacterSubclassDefinition> classesAndSubclasses)
        {
            var context = new CasterLevelContext();
            foreach (var classAndLevel in classesAndLevels)
            {
                int numLevelsToUseFromNextClass = classAndLevel.Value;
                classesAndSubclasses.TryGetValue(classAndLevel.Key, out CharacterSubclassDefinition subclass);
                eAHCasterType casterType = GetCasterTypeForSingleLevelOfClass(classAndLevel.Key, subclass);

                //Only increment caster level when the class in question has actually gottent their spellcasting feature.  Artificer's get spell slots at level 1 just to complicate things :).
                if (casterType == eAHCasterType.Full || casterType == eAHCasterType.HalfArtificer || (numLevelsToUseFromNextClass >= 2 && casterType == eAHCasterType.Half) || (numLevelsToUseFromNextClass >= 3 && casterType == eAHCasterType.OneThird))
                {
                    for (int i = numLevelsToUseFromNextClass; i > 0; i--)
                        context.IncrementCasterLevel(casterType);
                }
            }

            return context.GetCasterLevel();
        }

        private static eAHCasterType GetCasterTypeForSingleLevelOfClass(CharacterClassDefinition charClass, CharacterSubclassDefinition subclass)
        {
            if (FullCasterList.Contains(charClass))
                return eAHCasterType.Full;
            else if (HalfCasterList.Contains(charClass))
                return eAHCasterType.Half;
            else if (OneThirdCasterList.Contains(subclass))
                return eAHCasterType.OneThird;

            //Fallback to get it from the class name.  TODO also do same for subclass (not necessary yet, should likely go through the class/subclass features to see if they have a spellcasting feature and get the type from that).
            return GetCasterTypeFromClassName(charClass.Name);
        }

        private static eAHCasterType GetCasterTypeFromClassName(string className)
        {
            switch (className)
            {
                case "Cleric":
                case "Bard":
                case "BardClass": // Holic92's Bard
                case "Sorcerer":
                case "Wizard":
                    return eAHCasterType.Full;

                case "ClassTinkerer": // CJD's Tinkerer
                    return eAHCasterType.HalfArtificer;
                case "Paladin":
                case "Ranger":
                    return eAHCasterType.Half;

                //Warlock is an odd case and should likely be handled completely separately
                case "Warlock":
                case "AHWarlockClass": // AceHigh's Warlock
                    return eAHCasterType.None;

                case "AHBarbarianClass": // AceHigh's Barbarian
                case "BarbarianClass": // Holic92's Barbarian
                case "Fighter":
                case "Monk":
                case "MonkClass": // Holic92's Monk
                case "Rogue":
                    return eAHCasterType.None;

                default:
                    return eAHCasterType.None;
            }
        }

        public class CasterLevelContext
        {
            public CasterLevelContext()
            {
                NumOneThirdLevels = 0;
                NumHalfLevels = 0;
                NumFullLevels = 0;
            }

            //I think technically this should be split by each OneThird and each Half caster but I can look at that later.
            public void IncrementCasterLevel(eAHCasterType casterLevelType)
            {
                if (casterLevelType == eAHCasterType.OneThird)
                    NumOneThirdLevels++;
                if (casterLevelType == eAHCasterType.Half)
                    NumHalfLevels++;
                if (casterLevelType == eAHCasterType.HalfArtificer)
                    NumHalfArtificerLevels++;
                if (casterLevelType == eAHCasterType.Full)
                    NumFullLevels++;
            }

            public double GetCasterLevel()
            {
                double casterLevel = 0;
                if (NumOneThirdLevels >= 3)
                    casterLevel += NumOneThirdLevels / 3.0;
                if (NumHalfLevels >= 2)
                    casterLevel += NumHalfLevels / 2.0;
                if (NumHalfArtificerLevels >= 1) //artificer spell level round up instead of down like other spellcasting classes
                    casterLevel += Math.Ceiling(NumHalfArtificerLevels / 2.0);

                casterLevel += NumFullLevels;

                return casterLevel;
            }

            double NumOneThirdLevels = 0;
            double NumHalfLevels = 0;
            double NumHalfArtificerLevels = 0;
            double NumFullLevels = 0;
        }

        public enum eAHCasterType
        {
            None,
            OneThird,
            Half,
            HalfArtificer,
            Full
        };

        //TODO add Bard and other potential full casters - Note Warlock should not be included handle everything on their own since pact/short rest spell slots will not be shared in any fashion with other classes.
        private static readonly CharacterClassDefinition[] FullCasterList = new CharacterClassDefinition[]
        {
                     DatabaseHelper.CharacterClassDefinitions.Cleric,
                     DatabaseHelper.CharacterClassDefinitions.Wizard,
        };

        private static readonly CharacterClassDefinition[] HalfCasterList = new CharacterClassDefinition[]
        {
                     DatabaseHelper.CharacterClassDefinitions.Paladin,
                     DatabaseHelper.CharacterClassDefinitions.Ranger,
        };

        private static readonly CharacterSubclassDefinition[] OneThirdCasterList = new CharacterSubclassDefinition[]
        {
                     DatabaseHelper.CharacterSubclassDefinitions.MartialSpellblade,
                     DatabaseHelper.CharacterSubclassDefinitions.RoguishShadowCaster,
        };

        private static readonly List<SlotsByLevelDuplet> FullCastingSlots = new List<SlotsByLevelDuplet>()
                 {
                     //Add 10th level slots that are always 0 since Solasta seems to rely on IndexOf(0) for certain things
                     new SlotsByLevelDuplet() { Slots = new List<int> {2,0,0,0,0,0,0,0,0,0}, Level = 1 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {3,0,0,0,0,0,0,0,0,0}, Level = 2 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,2,0,0,0,0,0,0,0,0}, Level = 3 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,0,0,0,0,0,0,0,0}, Level = 4 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,2,0,0,0,0,0,0,0}, Level = 5 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,0,0,0,0,0,0,0}, Level = 6 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,1,0,0,0,0,0,0}, Level = 7 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,2,0,0,0,0,0,0}, Level = 8 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,1,0,0,0,0,0}, Level = 9 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,0,0,0,0,0}, Level = 10 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,0,0,0,0}, Level = 11 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,0,0,0,0}, Level = 12 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,0,0,0}, Level = 13 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,0,0,0}, Level = 14 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,1,0,0}, Level = 15 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,1,0,0}, Level = 16 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,1,1,0}, Level = 17 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,3,1,1,1,1,0}, Level = 18 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,3,2,1,1,1,0}, Level = 19 },
                     new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,3,2,2,1,1,0}, Level = 20 },
                 };

        private static RulesetCharacterHero GetHero(string name)
        {
            
            var characterBuildingService = ServiceRepository.GetService<ICharacterBuildingService>();

            if (characterBuildingService?.HeroCharacter != null)
            {
                return characterBuildingService.HeroCharacter;
            }
            else
            {
                var gameService = ServiceRepository.GetService<IGameService>();
                var gameCampaignCharacter = gameService?.Game?.GameCampaign?.Party?.CharactersList.Find(x => x.RulesetCharacter.Name == name);

                return (RulesetCharacterHero)gameCampaignCharacter?.RulesetCharacter;
            }
        }
    }
}