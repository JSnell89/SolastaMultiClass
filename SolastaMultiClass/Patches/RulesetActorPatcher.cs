using HarmonyLib;
using SolastaModApi;

namespace SolastaMultiClass.Patches
{
    class RulesetActorPatcher
    {
        //Paladin pool fix
        [HarmonyPatch(typeof(RulesetActor), "RefreshAttributes")]
        internal static class RulesetActor_RefreshAttributes_Patch
        {
            internal static void Postfix(RulesetActor __instance)
            {
                if (__instance is RulesetCharacterHero hero)
                {
                    //Fix Paladin pool - It would be great to find out what class the modifier came from but I'm unsure how easy that will be to do, the actual active modifier portion doesn't seem to know that information.
                    //If we could figure out what class each attribute modifier came from we could make a generic solution to this issue.
                    if (hero.ClassesAndLevels.ContainsKey(DatabaseHelper.CharacterClassDefinitions.Paladin))
                    {
                        var healingPoolAttribute = hero.GetAttribute("HealingPool", true);
                        if (healingPoolAttribute != null)
                        {
                            foreach (RulesetAttributeModifier activeModifier in healingPoolAttribute.ActiveModifiers)
                            {
                                if (activeModifier.Operation != FeatureDefinitionAttributeModifier.AttributeModifierOperation.MultiplyByCharacterLevel && activeModifier.Operation != FeatureDefinitionAttributeModifier.AttributeModifierOperation.MultiplyByClassLevel)
                                {
                                    continue;
                                }
                                activeModifier.Value = (float)hero.ClassesAndLevels[DatabaseHelper.CharacterClassDefinitions.Paladin];
                            }
                            healingPoolAttribute.Refresh();
                        }
                    }
                }
            }
        }
    }
}