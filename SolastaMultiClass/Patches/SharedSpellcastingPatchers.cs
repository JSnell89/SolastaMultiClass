using HarmonyLib;
using SolastaModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static FeatureDefinitionCastSpell;
using static SolastaMultiClass.Models.MultiClass;

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

                var allSpellRepetoires = __instance.ActionParams.ActingCharacter.RulesetCharacter.SpellRepertoires;
                if (allSpellRepetoires.Count < 2 || __instance.ActionParams.SpellRepertoire.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                RuleDefinitions.RechargeRate rechargeRateToSpendSlotsFrom = __instance.ActionParams.SpellRepertoire.SpellCastingFeature.SlotsRecharge;
                var additionalSpellRepetoriesToSpendSlotsFrom = allSpellRepetoires.Where(sr => sr != __instance.ActionParams.SpellRepertoire && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
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

                var allSpellRepetoires = __instance.SpellRepertoires;
                if (allSpellRepetoires.Count < 2 || activeSpell.SpellRepertoire.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                RuleDefinitions.RechargeRate rechargeRateToSpendSlotsFrom = activeSpell.SpellRepertoire.SpellCastingFeature.SlotsRecharge;
                var additionalSpellRepetoriesToSpendSlotsFrom = allSpellRepetoires.Where(sr => sr != activeSpell.SpellRepertoire && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
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

                var allSpellRepetoires = __instance.SpellRepertoires;
                if (allSpellRepetoires.Count < 2 || usablePower.PowerDefinition.RechargeRate != RuleDefinitions.RechargeRate.SpellSlot || usablePower.PowerDefinition.SpellcastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                RuleDefinitions.RechargeRate rechargeRateToSpendSlotsFrom = usablePower.PowerDefinition.SpellcastingFeature.SlotsRecharge;
                var additionalSpellRepetoriesToSpendSlotsFrom = allSpellRepetoires.Where(sr => sr.SpellCastingFeature != usablePower.PowerDefinition.SpellcastingFeature && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
                foreach (var spellRepetoire in additionalSpellRepetoriesToSpendSlotsFrom)
                    spellRepetoire.SpendSpellSlot(spellRepetoire.GetLowestAvailableSlotLevel()); //Theoretically if we've done this correctly the lowest slot in the other repetoires will be the same as what the power used from the initial repetoire
            }
        }

        //Probably want this patch either way so not turned off by EnableSharedSpellCasting
        //This fixes the case where if you multiclass a caster that selects spells at level up (e.g. Wizard) the Solasta engine will have you select spells even when you are leveling up another class (they don't get saved properly but seem to break some things)
        //This also fixes selecting spells with these caster types when multiclassing back into them.  Note Wizard may have multiclass issues if it doesn't have a spellbook no matter what, it might be worth giving every char a spell book to prevent this.
        [HarmonyPatch(typeof(CharacterBuildingManager), "UpgradeSpellPointPools")]
        internal static class CharacterBuildingManager_UpgradeSpellPointPools_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance)
            {                
                foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                {
                    string empty = string.Empty;
                    if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class)
                    {
                        CharacterClassDefinition characterClassDefinition = null;
                        int num = 0;
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out num);

                        //Short circuit if the feature is for another class otherwise (change from native code)
                        if (spellRepertoire.SpellCastingClass != characterClassDefinition)
                            continue;

                        empty = AttributeDefinitions.GetClassTag(characterClassDefinition, num);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                    {
                        CharacterClassDefinition characterClassDefinition1 = null;
                        int num1 = 0;
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition1, out num1);
                        CharacterSubclassDefinition item = __instance.HeroCharacter.ClassesAndSubclasses[characterClassDefinition1];

                        //Short circuit if the feature is for another subclass (change from native code)
                        if (spellRepertoire.SpellCastingSubclass != characterClassDefinition1)
                            continue;

                        empty = AttributeDefinitions.GetSubclassTag(characterClassDefinition1, num1, item);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Race)
                    {
                        empty = "02Race";
                    }
                    int maxPoints = 0;
                    if (__instance.HasAnyActivePoolOfType(HeroDefinitions.PointsPoolType.Cantrip) && __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools.ContainsKey(empty))
                    {
                        maxPoints = __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools[empty].MaxPoints;
                    }

                    //Yay reflection to call private methods/use private fields
                    var charBMType = typeof(CharacterBuildingManager);
                    var applyFeatureCastSpellMethod = charBMType.GetMethod("ApplyFeatureCastSpell", BindingFlags.NonPublic | BindingFlags.Instance);
                    var setPointPoolMethod = charBMType.GetMethod("SetPointPool", BindingFlags.NonPublic | BindingFlags.Instance);
                    var tempAcquiredCantripsNumberFieldInfo = charBMType.GetField("tempAcquiredCantripsNumber", BindingFlags.NonPublic | BindingFlags.Instance);                    
                    var tempAcquiredSpellsNumberFieldInfo = charBMType.GetField("tempAcquiredSpellsNumber", BindingFlags.NonPublic | BindingFlags.Instance);

                    tempAcquiredCantripsNumberFieldInfo.SetValue(__instance, 0);
                    tempAcquiredSpellsNumberFieldInfo.SetValue(__instance, 0);
                    
                    //Make sure not to recurse indefinitely!  The call here is needed 
                    applyFeatureCastSpellMethod.Invoke(__instance, new object[] { spellRepertoire.SpellCastingFeature});

                    int tempCantrips = (int)tempAcquiredCantripsNumberFieldInfo.GetValue(__instance);
                    int tempSpells = (int)tempAcquiredSpellsNumberFieldInfo.GetValue(__instance);

                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Cantrip, empty, tempCantrips + maxPoints });
                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Spell, empty, tempSpells });
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(RulesetSpellRepertoire), "get_MaxSpellLevelOfSpellCastingLevel")]
        internal static class RulesetSpellRepertoire_get_MaxSpellLevelOfSpellCastingLevel_Patch
        {
            internal static void Postfix(RulesetSpellRepertoire __instance, ref int __result)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                // This only affects when loaded in game since looping through all created characters is quite slow.  This means that spell slots may be incorrect until the character is used/long rests in game though :(
                var heroes = GetHeroesParty();

                var heroWithSpellRepetoire = heroes?.FirstOrDefault(hero => string.Equals(hero.Name, __instance.CharacterName));

                //Don't bother doing fancy work if there aren't multiple spell repetoires that are shared (multiple long rest spell features).
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
                    int num = item.Slots.IndexOf(0);//Should revisit for Warlocks - something like: int num = item.Slots.FindLastIndex(i => i > 0) + 1;//Switch to Last non-zero index (plus 1 since arrays start a 0)
                    if (num == -1)
                    {
                        num = (item.Slots.Count > 0 ? item.Slots.Count : __instance.SpellCastingFeature.SpellListDefinition.MaxSpellLevel);
                    }
                    __result = num;
                }
            }
        }

        [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetSlotsNumber")]
        internal static class RulesetSpellRepertoire_GetSlotsNumber_Patch
        {
            //Maybe make this a prefix to not have to re-run the Solasta code if there is only one spell repetoire?
            internal static void Postfix(RulesetSpellRepertoire __instance, int spellLevel, out int remaining, out int max)
            {
                // This only affects when loaded in game since looping through all created characters is quite slow.  This means that spell slots may be incorrect until the character is used/long rests in game though :(
                var heroes = GetHeroesParty();
                
                var heroWithSpellRepetoire = heroes?.FirstOrDefault(hero => string.Equals(hero.Name, __instance.CharacterName));

                //Don't bother doing fancy work if there aren't multiple spell repetoires that are shared (multiple long rest spell features).
                if (Main.Settings.EnableSharedSpellCasting && heroWithSpellRepetoire != null && heroWithSpellRepetoire.SpellRepertoires.Where(sr => sr.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest).Count() > 1)
                {
                    int casterLevel = (int)Math.Floor(GetCasterLevelForGivenLevel(heroWithSpellRepetoire.ClassesAndLevels, heroWithSpellRepetoire.ClassesAndSubclasses));//Multiclassing always rounds down caster level

                    if (spellLevel == 0 || !__instance.AvailableSpellsSlots.ContainsKey(spellLevel))
                    {
                        remaining = 0;
                        max = 0;
                        return;
                    }
                    remaining = __instance.AvailableSpellsSlots[spellLevel];
                    if (__instance.SpellCastingFeature == null)
                    {
                        max = remaining;
                        return;
                    }
                    if (FullCastingSlots[casterLevel - 1].Slots.Count <= spellLevel - 1)
                    {
                        max = 0;
                        return;
                    }
                    max = FullCastingSlots[casterLevel - 1].Slots[spellLevel - 1];
                }
                else //Copy of Solasta code
                {
                    //I don't know how to nicely handle the out params another way - this is inefficient though :(
                    if (spellLevel == 0 || !__instance.AvailableSpellsSlots.ContainsKey(spellLevel))
                    {
                        remaining = 0;
                        max = 0;
                        return;
                    }
                    remaining = __instance.AvailableSpellsSlots[spellLevel];
                    if (__instance.SpellCastingFeature == null)
                    {
                        max = remaining;
                        return;
                    }
                    if (__instance.SpellCastingFeature.SlotsPerLevels[__instance.SpellCastingLevel - 1].Slots.Count <= spellLevel - 1)
                    {
                        max = 0;
                        return;
                    }
                    max = __instance.SpellCastingFeature.SlotsPerLevels[__instance.SpellCastingLevel - 1].Slots[spellLevel - 1];
                }

            }
        }


        [HarmonyPatch(typeof(RulesetSpellRepertoire), "RestoreAllSpellSlots")]
        internal static class RulesetSpellRepertoire_RestoreAllSpellSlots_Patch
        {
            internal static void Postfix(RulesetSpellRepertoire __instance)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                //Only do custom slot sharing for long rest (e.g. non-Warlock) slots
                if (__instance.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                //If combined with shared spell slot usage from the other patches we want every spell feature to have the number of spell slots for the total caster level of the character
                var heroes = GetHeroesParty();
                var heroWithSpellRepetoire = heroes?.FirstOrDefault(hero => string.Equals(hero.Name, __instance.CharacterName));

                ////Attempt to have the slots updated when not in game.  Much more expensive since it goes through the full hero list using IO methods.
                //if(heroWithSpellRepetoire == null)
                //{
                //    var fullHeroList = GetFullHeroesPool();
                //    heroWithSpellRepetoire = heroes?.FirstOrDefault(hero => string.Equals(hero.Name, __instance.CharacterName));
                //}
                
                //Don't bother doing extra work if there aren't multiple spell repetoires that are shared (multiple long rest spell features).
                if (heroWithSpellRepetoire != null && heroWithSpellRepetoire.SpellRepertoires.Where(sr =>sr.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest).Count() > 1)
                {
                    __instance.AvailableSpellsSlots.Clear();
                    //TODO hero pool may be outdated at this point (1 level behind)
                    int casterLevel = (int)Math.Floor(GetCasterLevelForGivenLevel(heroWithSpellRepetoire.ClassesAndLevels, heroWithSpellRepetoire.ClassesAndSubclasses));//Multiclassing always rounds down caster level
                    int casterLevelArrayIndex = casterLevel % 2 == 0 ? casterLevel / 2 : (casterLevel + 1) / 2;

                    //Update the instance
                    for (int i = 0; i < casterLevelArrayIndex; i++)
                    {
                        __instance.AvailableSpellsSlots[i + 1] = FullCastingSlots[casterLevel - 1].Slots[i];
                    }
                    __instance.RepertoireRefreshed?.Invoke(__instance);

                    //And update the spell repetoire on the hero?  They seem to be disjointed/decoupled at this point
                    foreach (var spellRepetoire in heroWithSpellRepetoire.SpellRepertoires.Where(spellRep => spellRep.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest))
                    {
                        for (int i = 0; i < casterLevelArrayIndex; i++)
                        {
                            spellRepetoire.AvailableSpellsSlots[i + 1] = FullCastingSlots[casterLevel - 1].Slots[i];
                        }
                        spellRepetoire.RepertoireRefreshed?.Invoke(spellRepetoire);
                    }
                }
            }
        }

        //Remove the ability to select spells of higher level than you should be able to when leveling up
        //This doesn't fix the 'Auto' choices however.
        //Essentially the only change that is needed to use class level instead of hero level but requires a whole bunch of postfix code to do so :)
        [HarmonyPatch(typeof(CharacterStageSpellSelectionPanel), "Refresh")]
        internal static class CharacterStageSpellSelectionPanel_Refresh_Patch
        {
            internal static void Postfix(CharacterStageSpellSelectionPanel __instance)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var charBMType = typeof(CharacterStageSpellSelectionPanel);
                var currentLearnStepFieldInfo = charBMType.GetField("currentLearnStep", BindingFlags.NonPublic | BindingFlags.Instance);
                var allTagsFieldInfo = charBMType.GetField("allTags", BindingFlags.NonPublic | BindingFlags.Instance);
                var spellsByLevelTableFieldInfo = charBMType.GetField("spellsByLevelTable", BindingFlags.NonPublic | BindingFlags.Instance);

                int currentLearnStep = (int)currentLearnStepFieldInfo.GetValue(__instance);
                List<string> allTags = (List<string>)allTagsFieldInfo.GetValue(__instance);

                if (allTags == null)
                    return;

                string item = "";
                if (currentLearnStep == allTags.Count)
                {
                    item = allTags[allTags.Count - 1];
                }

                CharacterClassDefinition characterClassDefinition = null;
                __instance.CharacterBuildingService.GetLastAssignedClassAndLevel(out characterClassDefinition, out int unused);

                FeatureDefinitionCastSpell spellFeature = __instance.CharacterBuildingService.GetSpellFeature(item);

                //Only need updates if for spell selection.  This fixes an issue where Clerics were getting level 1 spells as cantrips :).
                if (spellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.Selection || spellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.Spellbook)
                    return;

                bool flag = false;
                if (spellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.Selection || spellFeature.SpellKnowledge == RuleDefinitions.SpellKnowledge.Spellbook)
                {
                    flag = true;
                }
                int count = __instance.CharacterBuildingService.HeroCharacter.ClassesAndLevels[characterClassDefinition]; //Changed to use class level instead of hero level
                int num = (flag ? spellFeature.ComputeHighestSpellLevel(count) : 0);

                UnityEngine.RectTransform spellsByLevelRect = (UnityEngine.RectTransform)spellsByLevelTableFieldInfo.GetValue(__instance);
                int currentChildCount = spellsByLevelRect.childCount;

                if (spellsByLevelRect != null && currentChildCount > num+1)
                {
                    //Deactivate the extra spell UI that can show up do to the original method using Character level instead of Class level
                    for(int i = num+1; i < currentChildCount; i++)
                    {
                        var child = spellsByLevelRect.GetChild(i);
                        child?.gameObject?.SetActive(false);
                    }

                    //TODO test if this is needed
                    LayoutRebuilder.ForceRebuildLayoutImmediate(spellsByLevelRect);
                }
            }
        }        

        [HarmonyPatch(typeof(CharacterBuildingManager), "AutoAcquireSpells")]
        internal static class CharacterBuildingManager_AutoAcquireSpells_Patch
        {
            internal static void Postfix(CharacterStageSpellSelectionPanel __instance, string spellTag)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                //TODO remove 
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

                var hero = __instance.Caster as RulesetCharacterHero; //Cast to RulesetCharacterHero so we can figure out the level of the current class
                if (hero == null || characterClassDefinition == null)
                    return;

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
                        if (child.TryGetComponent(typeof(SlotStatusTable), out Component unused))
                            continue;
                        if(i > (maxLevelOfSpellcastingForClass + accountForCantripsInt) - 1 && !Main.Settings.TurnOffSpellPreparationRestrictions) //The toggle option needs to be here otherwise if you already had it on and opened a spell list it will mess things up potentially for all spellcasters
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
                for (int i = numLevelsToUseFromNextClass; i > 0; i--)
                {
                    CharacterSubclassDefinition subclass = null;
                    classesAndSubclasses.TryGetValue(classAndLevel.Key, out subclass);
                    context.IncrementCasterLevel(GetCasterTypeForSingleLevelOfClass(classAndLevel.Key, subclass));
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

            return eAHCasterType.None;
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
                casterLevel += NumFullLevels;

                return casterLevel;
            }

            double NumOneThirdLevels = 0;
            double NumHalfLevels = 0;
            double NumFullLevels = 0;
        }

        public enum eAHCasterType
        {
            None,
            OneThird,
            Half,
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

        public static readonly List<SlotsByLevelDuplet> FullCastingSlots = new List<SlotsByLevelDuplet>()
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

        public static List<RulesetCharacterHero> GetHeroesParty()
        {
            var gameService = ServiceRepository.GetService<IGameService>();
            var inGameHeroesPool = new List<RulesetCharacterHero>();

            if (gameService?.Game != null)
            {
                foreach (var gameCampaignCharacter in gameService.Game.GameCampaign.Party.CharactersList)
                    inGameHeroesPool.Add((RulesetCharacterHero)gameCampaignCharacter.RulesetCharacter);
            }
            return inGameHeroesPool;
        }
        //public static List<RulesetCharacterHero> GetFullHeroesPool(bool isDirty = false)
        //{
        //    if (isDirty)
        //    {
        //        HeroesPool.Clear();
        //    }
        //    if (HeroesPool.Count == 0)
        //    {
        //        var characterPoolService = ServiceRepository.GetService<ICharacterPoolService>();

        //        if (characterPoolService != null)
        //        {
        //            HeroesPool.Clear();
        //            foreach (var name in characterPoolService.Pool.Keys)
        //            {
        //                characterPoolService.LoadCharacter(
        //                    characterPoolService.BuildCharacterFilename(name.Substring(0, name.Length - 4)),
        //                    out RulesetCharacterHero hero,
        //                    out RulesetCharacterHero.Snapshot snapshot);
        //                HeroesPool.Add(hero);
        //            }
        //        }
        //    }
        //    return HeroesPool;
        //}

        internal static List<RulesetCharacterHero> HeroesPool = new List<RulesetCharacterHero>();
    }
}