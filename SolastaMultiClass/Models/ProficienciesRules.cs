using System.Collections.Generic;
using HarmonyLib;

namespace SolastaMultiClass.Models
{
    static class ProficienciesRules
    {
        public static void FixMulticlassProficiencies(CharacterClassDefinition selectedClass, List<FeatureUnlockByLevel> featureUnlockByLevels)
        {
            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();

            if (featuresToExclude.ContainsKey(selectedClass.Name))
            {
                foreach (var oldNewFeatureName in featuresToExclude[selectedClass.name])
                {
                    if (oldNewFeatureName.Value != null && featuresDb.TryGetElement(oldNewFeatureName.Value, out FeatureDefinition feature))
                    {
                        var featureUnlockByLevel = featureUnlockByLevels.Find(x => x.FeatureDefinition.Name == oldNewFeatureName.Key && x.Level == 1);
                        if (featureUnlockByLevel != null)
                        {
                            AccessTools.Field(featureUnlockByLevel.GetType(), "featureDefinition").SetValue(featureUnlockByLevel, feature);
                        }
                    }
                    else
                    {
                        featureUnlockByLevels.RemoveAll(x => x.FeatureDefinition.Name == oldNewFeatureName.Key && x.Level == 1);
                    }
                }
            }
        }

        public static void FixExtraAttacks(CharacterClassDefinition selectedClass, List<FeatureUnlockByLevel> featureUnlockByLevels)
        {
            if (!Main.Settings.EnableNonStackingExtraAttacks) return;

            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();

            if (extraAttacksToExclude.ContainsKey(selectedClass.name))
            {
                foreach (var featureName in extraAttacksToExclude[selectedClass.Name])
                {
                    if (HasExtraAttack(selectedClass))
                    {
                        featureUnlockByLevels.RemoveAll(x => x.FeatureDefinition.Name == featureName && x.Level == 5);
                    }
                }
            }
        }

        private static bool HasExtraAttack(CharacterClassDefinition selectedClass)
        {
            var service = ServiceRepository.GetService<ICharacterBuildingService>();
            var hero = service?.HeroCharacter;
            var hasExtraAttack = false;

            if (hero != null)
            {
                foreach (var classAndLevel in hero.ClassesAndLevels)
                {
                    var className = classAndLevel.Key.Name;

                    if (extraAttacksToExclude.ContainsKey(className) && className != selectedClass.Name && classAndLevel.Value >= 5) // && !WasGrantedBy(selectedClass)
                    {
                        hasExtraAttack = true;
                    }
                }
            }
            return hasExtraAttack;
        }

        private static readonly Dictionary<string, Dictionary<string, string>> featuresToExclude = new Dictionary<string, Dictionary<string, string>>
        {
            {"BarbarianClass", new Dictionary<string, string> {
                {"BarbarianArmorProficiency", "BarbarianClassArmorProficiencyMulticlass" },
                {"BarbarianSkillProficiency", null},
                {"BarbarianSavingthrowProficiency", null} } },

            {"BardClass", new Dictionary<string, string> {
                {"BardWeaponProficiency", null},
                {"BardSkillProficiency", "BardClassSkillProficiencyMulticlass"},
                {"BardSavingthrowProficiency", null} } },

            {"MonkClass", new Dictionary<string, string> {
                {"MonkSkillProficiency", null},
                {"MonkSavingthrowProficiency", null} } },

            {"Cleric", new Dictionary<string, string> {
                {"ProficiencyClericWeapon", null},
                {"PointPoolClericSkillPoints", null},
                {"ProficiencyClericSavingThrow", null} } },

            {"Fighter", new Dictionary<string, string> {
                {"ProficiencyFighterArmor", "FighterArmorProficiencyMulticlass"},
                {"PointPoolFighterSkillPoints", null},
                {"ProficiencyFighterSavingThrow", null} } },

            {"Paladin", new Dictionary<string, string> {
                {"ProficiencyPaladinArmor", "PaladinArmorProficiencyMulticlass"},
                {"PointPoolPaladinSkillPoints", null},
                {"ProficiencyPaladinSavingThrow", null} } },

            {"Ranger", new Dictionary<string, string> {
                {"PointPoolRangerSkillPoints", "PointPoolRangerSkillPointsMulticlass"},
                {"ProficiencyRangerSavingThrow", null} } },

            {"Rogue", new Dictionary<string, string> {
                {"ProficiencyRogueWeapon", null},
                {"PointPoolRogueSkillPoints", null},
                {"ProficiencyRogueSavingThrow", null} } },

            {"Sorcerer", new Dictionary<string, string> {
                {"ProficiencySorcererWeapon", null},
                {"ProficiencySorcererArmor", null},
                {"PointPoolSorcererSkillPoints", null},
                {"ProficiencySorcererSavingThrow", null}} },

            {"Wizard", new Dictionary<string, string> {
                {"ProficiencyWizardWeapon", null},
                {"ProficiencyWizardArmor", null},
                {"PointPoolWizardSkillPoints", null},
                {"ProficiencyWizardSavingThrow", null}} },

            {"ClassTinkerer", new Dictionary<string, string> {
                {"ProficiencyWeaponTinkerer", null},
                {"PointPoolTinkererSkillPoints", null},
                {"ProficiencyTinkererSavingThrow", null}} }
        };

        private static readonly Dictionary<string, List<string>> extraAttacksToExclude = new Dictionary<string, List<string>>
        {
            {"BarbarianClass", new List<string> { "BarbarianClassExtraAttack" } },

            {"MonkClass", new List<string> { "MonkClassExtraAttack" } },

            {"Fighter", new List<string> { "AttributeModifierFighterExtraAttack" } },

            {"Paladin", new List<string> { "AttributeModifierPaladinExtraAttack" } },

            {"Ranger", new List<string> { "AttributeModifierRangerExtraAttack" } }
        };
    }
}