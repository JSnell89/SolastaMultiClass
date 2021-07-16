using HarmonyLib;
using SolastaModApi;

namespace SolastaMultiClass.Patches
{
    class RulesetActorPatcher
    {
        [HarmonyPatch(typeof(RulesetActor), "RefreshAttributes")]
        internal static class RulesetActor_RefreshAttributes_Patch
        {
            internal static void Postfix(RulesetActor __instance)
            {
                if (__instance is RulesetCharacterHero hero)
                {
                    // fixes the Paladin pool to use the class level instead
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
                    // fixes the Sorcerer pool to use the class level instead
                    if (hero.ClassesAndLevels.ContainsKey(DatabaseHelper.CharacterClassDefinitions.Sorcerer))
                    {
                        var sorceryPointsAttributes = hero.GetAttribute("SorceryPoints", true);
                        if (sorceryPointsAttributes != null)
                        {
                            foreach (RulesetAttributeModifier activeModifier in sorceryPointsAttributes.ActiveModifiers)
                            {
                                if (activeModifier.Operation != FeatureDefinitionAttributeModifier.AttributeModifierOperation.MultiplyByCharacterLevel && activeModifier.Operation != FeatureDefinitionAttributeModifier.AttributeModifierOperation.MultiplyByClassLevel)
                                {
                                    continue;
                                }
                                activeModifier.Value = (float)hero.ClassesAndLevels[DatabaseHelper.CharacterClassDefinitions.Sorcerer];
                            }
                            sorceryPointsAttributes.Refresh();
                        }
                    }
                }
            }
        }
    }
}