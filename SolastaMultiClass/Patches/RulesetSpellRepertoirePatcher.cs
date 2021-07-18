using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    class RulesetSpellRepertoirePatcher
    {
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "MaxSpellLevelOfSpellCastingLevel", MethodType.Getter)]
        internal static class RulesetSpellRepertoire_MaxSpellLevelOfSpellCastingLevel_Getter_Patch
        {
            internal static void Postfix(RulesetSpellRepertoire __instance, ref int __result)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                var heroWithSpellRepertoire = Models.SharedSpellsRules.GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null) 
                    return;

                int casterLevel = Models.SharedSpellsRules.GetCasterLevel(heroWithSpellRepertoire);
                FeatureDefinitionCastSpell.SlotsByLevelDuplet item = Models.SharedSpellsRules.FullCastingSlots[casterLevel];

                int num = item.Slots.IndexOf(0);

                if (num == -1)
                {
                    num = (item.Slots.Count > 0 ? item.Slots.Count : __instance.SpellCastingFeature.SpellListDefinition.MaxSpellLevel);
                }

                __result = num;
            }
        }

        // we want every spell feature to have the number of spell slots for the total caster level of the character
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "ComputeSpellSlots")]
        internal static class RulesetSpellRepertoire_ComputeSpellSlots_Patch
        {
            internal static void Postfix(
                RulesetSpellRepertoire __instance, 
                List<FeatureDefinition> spellCastingAffinities, 
                Dictionary<int, int> ___spellsSlotCapacities, 
                Dictionary<int, int> ___legacyAvailableSpellsSlots, 
                Dictionary<int, int> ___usedSpellsSlots)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                // don't do anything for non-Long rest classes as they combine in different ways
                // if (__instance.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                //     return;

                var heroWithSpellRepertoire = Models.SharedSpellsRules.GetHero(__instance.CharacterName);

                // don't bother doing extra work if there aren't multiple spell repertoires that are shared (multiple long rest spell features)
                if (heroWithSpellRepertoire == null) // || heroWithSpellRepertoire.SpellRepertoires.Where(sr => sr.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest).Count() < 2)
                    return;

                // get the spell level
                int warlockLevel = Models.SharedSpellsRules.GetWarlockLevel(heroWithSpellRepertoire);
                int sharedCasterLevel = Models.SharedSpellsRules.GetSharedCasterLevel(heroWithSpellRepertoire);

                int warlockSpellLevel = Models.SharedSpellsRules.GetWarlockSpellLevel(heroWithSpellRepertoire);
                int sharedSpellLevel = Models.SharedSpellsRules.GeSharedSpellLevel(heroWithSpellRepertoire);

                int spellLevel = Math.Max(warlockSpellLevel, sharedSpellLevel);

                ___spellsSlotCapacities.Clear();

                for (int i = 1; i < spellLevel + 1; i++)
                {
                    // add the shared slots
                    ___spellsSlotCapacities[i] = Models.SharedSpellsRules.FullCastingSlots[sharedCasterLevel].Slots[i - 1]; 

                    // add the warlock slots
                    if (warlockSpellLevel == i)
                    {
                        ___spellsSlotCapacities[i] += Models.SharedSpellsRules.WarlockCastingSlots[warlockLevel];
                    }

                    // I believe this is just to properly handle saves between patches, theoretically it is needed for higher level slots for MC saves between patches as well
                    if (___legacyAvailableSpellsSlots.ContainsKey(i + 1))
                    {
                        ___usedSpellsSlots.Add(i + 1, ___spellsSlotCapacities[i + 1] - ___legacyAvailableSpellsSlots[i + 1]);
                        ___legacyAvailableSpellsSlots.Remove(i + 1);
                    }
                }

                // new features give the extra spell slots
                //if (spellCastingAffinities != null && spellCastingAffinities.Count > 0)
                //{
                //    foreach (FeatureDefinition spellCastingAffinity in spellCastingAffinities)
                //    {
                //        foreach (AdditionalSlotsDuplet additionalSlot in ((ISpellCastingAffinityProvider)spellCastingAffinity).AdditionalSlots)
                //        {
                //            if (!___spellsSlotCapacities.ContainsKey(additionalSlot.SlotLevel))
                //            {
                //                ___spellsSlotCapacities[additionalSlot.SlotLevel] = 0;
                //            }
                //            ___spellsSlotCapacities[additionalSlot.SlotLevel] += additionalSlot.SlotsNumber;
                //        }
                //    }
                //}

                // now updates all other long rest spell repertoires to have the same spell slots that we just calculated
                //foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires)
                //{
                //    for (int i = 0; i < spellLevel; i++)
                //    {
                //        ___spellsSlotCapacities[i + 1] = ___spellsSlotCapacities[i + 1];
                //    }
                //    spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
                //}

                __instance.RepertoireRefreshed?.Invoke(__instance);
            }
        }
    }
}