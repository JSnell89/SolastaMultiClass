using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static SolastaMultiClass.Models.SharedSpellsRules;

namespace SolastaMultiClass.Patches
{
    class SharedSpellCastingPatchers
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
                        if (i > (maxLevelOfSpellcastingForClass + accountForCantripsInt) - 1) // && !Main.Settings.TurnOffSpellPreparationRestrictions) //The toggle option needs to be here otherwise if you already had it on and opened a spell list it will mess things up potentially for all spellcasters
                            child.gameObject.SetActive(false);
                        else
                            child.gameObject.SetActive(true); //Need to set to true because when switching tabs the false from one spellcasting class is carried over.
                    }
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(spellsByLevelRect);
            }
        }
    }
}