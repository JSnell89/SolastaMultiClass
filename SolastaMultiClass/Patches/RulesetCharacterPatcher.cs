using System.Linq;
using HarmonyLib;

namespace SolastaMultiClass.Patches
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
            var additionalSpellRepertoiresToSpendSlotsFrom = allSpellRepertoires.Where(sr => sr != __instance.ActionParams.SpellRepertoire && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
            foreach (var spellRepertoire in additionalSpellRepertoiresToSpendSlotsFrom)
                spellRepertoire.SpendSpellSlot(__instance.ActionParams.IntParameter);
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
            var additionalSpellRepertoiresToSpendSlotsFrom = allSpellRepertoires.Where(sr => sr != activeSpell.SpellRepertoire && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
            foreach (var spellRepertoire in additionalSpellRepertoiresToSpendSlotsFrom)
                spellRepertoire.SpendSpellSlot(activeSpell.SlotLevel);
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
            var additionalSpellRepertoiresToSpendSlotsFrom = allSpellRepertoires.Where(sr => sr.SpellCastingFeature != usablePower.PowerDefinition.SpellcastingFeature && sr.SpellCastingFeature.SlotsRecharge == rechargeRateToSpendSlotsFrom);
            foreach (var spellRepertoire in additionalSpellRepertoiresToSpendSlotsFrom)
                spellRepertoire.SpendSpellSlot(spellRepertoire.GetLowestAvailableSlotLevel()); //Theoretically if we've done this correctly the lowest slot in the other repertoires will be the same as what the power used from the initial repetoire
        }
    }
}