using System.Collections.Generic;

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
                foreach (var featureName in featuresToExclude[selectedClass.name])
                {
                    featureUnlockByLevels.RemoveAll(x => x.FeatureDefinition.Name == featureName && x.Level == 1);
                }
            }
            if (featuresToInclude.ContainsKey(selectedClass.Name))
            {
                foreach (var featureName in featuresToInclude[selectedClass.name])
                {
                    if (featuresDb.TryGetElement(featureName, out FeatureDefinition feature))
                    {
                        featureUnlockByLevels.Add(new FeatureUnlockByLevel(feature, 1));
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

        private static readonly Dictionary<string, List<string>> featuresToExclude = new Dictionary<string, List<string>>
        {
            {"BarbarianClass", new List<string> {
                "BarbarianArmorProficiency",
                "BarbarianSkillProficiency",
                "BarbarianSavingthrowProficiency" } },

            {"BardClass", new List<string> {
                "BardWeaponProficiency",
                "BardSkillProficiency",
                "BardSavingthrowProficiency" } },

            {"MonkClass", new List<string> {
                "MonkSkillProficiency",
                "MonkSavingthrowProficiency" } },

            {"Cleric", new List<string> {
                "ProficiencyClericWeapon",
                "PointPoolClericSkillPoints",
                "ProficiencyClericSavingThrow" } },

            {"Fighter", new List<string> {
                "ProficiencyFighterArmor",
                "PointPoolFighterSkillPoints",
                "ProficiencyFighterSavingThrow" } },

            {"Paladin", new List<string> {
                "ProficiencyPaladinArmor",
                "PointPoolPaladinSkillPoints",
                "ProficiencyPaladinSavingThrow" } },

            {"Ranger", new List<string> {
                "PointPoolRangerSkillPoints",
                "ProficiencyRangerSavingThrow" } },

            {"Rogue", new List<string> {
                "ProficiencyRogueWeapon",
                "PointPoolRogueSkillPoints",
                "ProficiencyRogueSavingThrow" } },

            {"Sorcerer", new List<string> {
                "ProficiencySorcererWeapon",
                "ProficiencySorcererArmor",
                "PointPoolSorcererSkillPoints",
                "ProficiencySorcererSavingThrow" } },

            {"Wizard", new List<string> {
                "ProficiencyWizardWeapon",
                "ProficiencyWizardArmor",
                "PointPoolWizardSkillPoints",
                "ProficiencyWizardSavingThrow" } },

            {"Tinkerer", new List<string> {
                "ProficiencyWeaponTinkerer",
                "PointPoolTinkererSkillPoints",
                "ProficiencyTinkererSavingThrow" } }
        };

        private static readonly Dictionary<string, List<string>> featuresToInclude = new Dictionary<string, List<string>>
        {
            {"BarbarianClass", new List<string> { "BarbarianClassArmorProficiencyMulticlass" } },

            {"BardClass", new List<string> { "BardClassSkillProficiencyMulticlass" } },

            {"Fighter", new List<string> { "FighterArmorProficiencyMulticlass" } },

            {"Paladin", new List<string> { "PaladinArmorProficiencyMulticlass" } },

            {"Ranger", new List<string> { "PointPoolRangerSkillPointsMulticlass" } }
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