using System.Collections.Generic;
using HarmonyLib;

namespace SolastaMultiClass.Models
{
    static class Rules
    {
        public static List<CharacterClassDefinition> GetHeroAllowedClassDefinitions(RulesetCharacterHero hero)
        {
            var allowedClasses = new List<CharacterClassDefinition>() { };
            var currentClass = hero.ClassesHistory[hero.ClassesHistory.Count - 1];

            if (!ApproveMultiClassInOut(hero, currentClass) && Main.Settings.ForceMinInOutPreReqs)
            {
                allowedClasses.Add(currentClass);
            }
            else if (hero.ClassesAndLevels.Count >= Main.Settings.MaxAllowedClasses)
            {
                foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
                {
                    if (ApproveMultiClassInOut(hero, characterClassDefinition) || !Main.Settings.ForceMinInOutPreReqs)
                    {
                        allowedClasses.Add(characterClassDefinition);
                    }
                }
            }
            else
            {
                foreach (var classDefinition in DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetAllElements())
                {
                    if (ApproveMultiClassInOut(hero, classDefinition) || !Main.Settings.ForceMinInOutPreReqs)
                    {
                        allowedClasses.Add(classDefinition);
                    }
                }
            }
            return allowedClasses;
        }

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

        private static bool HasExtraAttack()
        {
            var service = ServiceRepository.GetService<CharacterBuildingManager>();
            var hero = service?.HeroCharacter;
            var hasExtraAttack = false;

            if (hero != null)
            {
                var lastClassName = hero.ClassesHistory[hero.ClassesHistory.Count - 1].Name;

                foreach (var classAndLevel in hero.ClassesAndLevels)
                {
                    var className = classAndLevel.Key.Name;

                    if (extraAttacksToExclude.ContainsKey(className) && className != lastClassName && classAndLevel.Value >= 5)
                    {
                        hasExtraAttack = true;
                    }
                }
            }
            return hasExtraAttack;
        }

        public static void FixExtraAttacks(CharacterClassDefinition selectedClass, List<FeatureUnlockByLevel> featureUnlockByLevels)
        {
            if (Main.Settings.AllowExtraAttacksToStack) return;

            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();

            if (extraAttacksToExclude.ContainsKey(selectedClass.name))
            {
                foreach (var featureName in extraAttacksToExclude[selectedClass.Name])
                {
                    if (HasExtraAttack())
                    {
                        featureUnlockByLevels.RemoveAll(x => x.FeatureDefinition.Name == featureName && x.Level == 5);
                    }
                }
            }
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

            {"Tinkerer", new Dictionary<string, string> {
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

        private static bool ApproveMultiClassInOut(RulesetCharacterHero hero, CharacterClassDefinition classDefinition)
        {
            var strength = hero.GetAttribute("Strength").CurrentValue;
            var dexterity = hero.GetAttribute("Dexterity").CurrentValue;
            var intelligence = hero.GetAttribute("Intelligence").CurrentValue;
            var wisdom = hero.GetAttribute("Wisdom").CurrentValue;
            var charisma = hero.GetAttribute("Charisma").CurrentValue;

            switch (classDefinition.Name)
            {
                case "AHBarbarianClass": // AceHigh's Barbarian
                case "BarbarianClass": // Holic92's Barbarian
                    return strength >= 13;

                case "AHWarlockClass": // AceHigh's Warlock
                case "BardClass": // Holic92's Bard
                case "Sorcerer":
                case "Warlock":
                    return charisma >= 13;

                case "Cleric":
                    return wisdom >= 13;

                case "Fighter":
                    return strength >= 13 || dexterity >= 13;

                case "MonkClass": // Holic92's Monk
                    return strength >= 13 && charisma >= 13;

                case "Paladin":
                    return strength >= 13 && charisma >= 13;

                case "Ranger":
                    return dexterity >= 13 && wisdom >= 13;

                case "ClassTinkerer": // CJD's Tinkerer
                    return intelligence >= 13;

                case "Rogue":
                    return dexterity >= 13;

                case "Wizard":
                    return intelligence >= 13;

                default:
                    return false;
            }
        }
    }
}