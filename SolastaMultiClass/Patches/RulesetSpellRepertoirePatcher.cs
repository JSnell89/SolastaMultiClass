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
            internal static void Postfix(RulesetSpellRepertoire __instance, List<FeatureDefinition> spellCastingAffinities)
            {
                if (!Main.Settings.EnableSharedSpellCasting)
                    return;

                // don't do anything for non-Long rest classes as they combine in different ways
                if (__instance.SpellCastingFeature.SlotsRecharge != RuleDefinitions.RechargeRate.LongRest)
                    return;

                var heroWithSpellRepertoire = GetHero(__instance.CharacterName);

                // TODO: Add Warlock logic from here

                // don't bother doing extra work if there aren't multiple spell repertoires that are shared (multiple long rest spell features)
                if (heroWithSpellRepertoire == null || heroWithSpellRepertoire.SpellRepertoires.Where(sr => sr.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest).Count() < 2)
                    return;

                int maxSpellLevel = __instance.MaxSpellLevelOfSpellCastingLevel;
                int casterLevel = GetHeroSharedCasterLevel(heroWithSpellRepertoire);

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

                // now updates all other long rest spell repertoires to have the same spell slots that we just calculated
                //foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires.Where(spellRep => spellRep.SpellCastingFeature.SlotsRecharge == RuleDefinitions.RechargeRate.LongRest))
                //{
                //    for (int i = 0; i < maxSpellLevel; i++)
                //    {
                //        var spellSlots = (Dictionary<int, int>)AccessTools.Field(spellRepertoire.GetType(), "spellsSlotCapacities").GetValue(spellRepertoire);
                //        spellSlots[i + 1] = currentInstanceSpellsSlotCapacities[i + 1];
                //    }
                //    spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
                //}

                RulesetSpellRepertoire.RepertoireRefreshedHandler repertoireRefreshed = __instance.RepertoireRefreshed;

                if (__instance.RepertoireRefreshed != null)
                {
                    repertoireRefreshed(__instance);
                }
            }
        }
    }
}