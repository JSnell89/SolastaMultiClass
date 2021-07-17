using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static SolastaMultiClass.Models.SharedSpellsRules;

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

                var heroWithSpellRepertoire = GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null) 
                    return;

                int casterLevel;

                if (__instance.SpellCastingClass != null && __instance.SpellCastingClass.Name.Contains("Warlock"))
                {
                    casterLevel = GetHeroSharedCasterLevel(heroWithSpellRepertoire, __instance.SpellCastingClass);
                }
                else
                {
                    casterLevel = GetHeroSharedCasterLevel(heroWithSpellRepertoire);
                }
                
                FeatureDefinitionCastSpell.SlotsByLevelDuplet item = FullCastingSlots[casterLevel - 1];

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
                if (__instance.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                var heroWithSpellRepertoire = GetHero(__instance.CharacterName);

                // don't bother doing extra work if there aren't multiple spell repertoires that are shared (multiple long rest spell features)
                if (heroWithSpellRepertoire == null || heroWithSpellRepertoire.SpellRepertoires.Where(sr => sr.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest).Count() < 2)
                    return;

                int maxSpellLevel = __instance.MaxSpellLevelOfSpellCastingLevel;
                int casterLevel = GetHeroSharedCasterLevel(heroWithSpellRepertoire);

                ___spellsSlotCapacities.Clear();
                for (int i = 0; i < maxSpellLevel; i++)
                {
                    // the important change right here
                    ___spellsSlotCapacities[i + 1] = FullCastingSlots[casterLevel - 1].Slots[i]; 

                    // I believe this is just to properly handle saves between patches, theoretically it is needed for higher level slots for MC saves between patches as well
                    if (___legacyAvailableSpellsSlots.ContainsKey(i + 1))
                    {
                        ___usedSpellsSlots.Add(i + 1, ___spellsSlotCapacities[i + 1] - ___legacyAvailableSpellsSlots[i + 1]);
                        ___legacyAvailableSpellsSlots.Remove(i + 1);
                    }
                }

                // new features give the extra spell slots
                if (spellCastingAffinities != null && spellCastingAffinities.Count > 0)
                {
                    foreach (FeatureDefinition spellCastingAffinity in spellCastingAffinities)
                    {
                        foreach (AdditionalSlotsDuplet additionalSlot in ((ISpellCastingAffinityProvider)spellCastingAffinity).AdditionalSlots)
                        {
                            if (!___spellsSlotCapacities.ContainsKey(additionalSlot.SlotLevel))
                            {
                                ___spellsSlotCapacities[additionalSlot.SlotLevel] = 0;
                            }
                            ___spellsSlotCapacities[additionalSlot.SlotLevel] += additionalSlot.SlotsNumber;
                        }
                    }
                }

                __instance.RepertoireRefreshed?.Invoke(__instance);
            }
        }
    }
}