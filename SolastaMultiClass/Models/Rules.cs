using System.Collections.Generic;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;

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

        public static void FixMulticlassProficiencies(RulesetCharacterHero hero, CharacterClassDefinition selectedClass, List<FeatureDefinition> grantedFeatures)
        {
            if (hero.ClassesHistory.Count > 1 && hero.ClassesAndLevels[selectedClass] == 1)
            {
                var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();
                var groupsToExclude = new List<string[]>()
                {
                    savingThrownsProficiencysToExclude,
                    skillProficiencysToExclude,
                    armorProficiencysToExclude,
                    weaponProficiencysToExclude
                };
                var groupsToInclude = new List<string[]>()
                {
                    armorProficiencysToInclude,
                    skillProficiencysToInclude
                };

                foreach (var grouptoExclude in groupsToExclude)
                {
                    foreach (var featureName in grouptoExclude)
                    {
                        if (featuresDb.TryGetElement(featureName, out FeatureDefinition feature))
                        {
                            grantedFeatures.Remove(feature);
                        }
                    }
                }
                foreach (var grouptoInclude in groupsToInclude)
                {
                    foreach (var featureName in grouptoInclude)
                    {
                        if (featuresDb.TryGetElement(featureName, out FeatureDefinition feature) && featureName.Contains(selectedClass.Name))
                        {
                            grantedFeatures.Add(feature);
                        }
                    }
                }
            }
        }

        public static void FixExtraAttacks(RulesetCharacterHero hero, CharacterClassDefinition selectedClass, List<FeatureDefinition> grantedFeatures)
        {
            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();
            var isHighLevelFighter = selectedClass == Fighter && hero.ClassesAndLevels.TryGetValue(Fighter, out int levels) && levels >= 11;
            var hasExtraAttack = false;
            var extraAttacksToExclude = new string[]
            {
                "BarbarianClassExtraAttack",
                "MonkClassExtraAttack",
                "AttributeModifierFighterExtraAttack",
                "AttributeModifierRangerExtraAttack",
                "AttributeModifierPaladinExtraAttack"
            };
            var extraAttacksClassNames = new List<string>
            {
                "BarbarianClass",
                "MonkClass",
                "Fighter",
                "Ranger",
                "Paladin"
            };

            foreach (var classAndLevel in hero.ClassesAndLevels)
            {
                var className = classAndLevel.Key.Name;
                if (className != selectedClass.Name && classAndLevel.Value >= 5 && extraAttacksClassNames.Contains(className))
                {
                    hasExtraAttack = true;
                }
            }
            foreach (var featureName in extraAttacksToExclude)
            {
                if (featuresDb.TryGetElement(featureName, out FeatureDefinition feature) && hasExtraAttack && !isHighLevelFighter)
                {
                    grantedFeatures.Remove(feature);
                }
            }
        }

        private static readonly string[] savingThrownsProficiencysToExclude = new string[]
        {
            "BarbarianSavingthrowProficiency",
            "BardSavingthrowProficiency",
            "MonkSavingthrowProficiency",
            "ProficiencyClericSavingThrow",
            "ProficiencyFighterSavingThrow",
            "ProficiencyPaladinSavingThrow",
            "ProficiencyRangerSavingThrow",
            "ProficiencyRogueSavingThrow",
            "ProficiencyWizardSavingThrow",
            "ProficiencyTinkererSavingThrow"
        };

        private static readonly string[] skillProficiencysToExclude = new string[]
        {
            "BarbarianSkillProficiency",
            "BardSkillProficiency",
            "MonkSkillProficiency",
            "PointPoolClericSkillPoints",
            "PointPoolFighterSkillPoints",
            "PointPoolPaladinSkillPoints",
            "PointPoolRangerSkillPoints",
            "PointPoolRogueSkillPoints",
            "PointPoolWizardSkillPoints",
            "PointPoolTinkererSkillPoints"
        };

        private static readonly string[] skillProficiencysToInclude = new string[]
        {
            "BardClassSkillProficiencyMulticlass",
            "PointPoolRangerSkillPointsMulticlass"
        };

        private static readonly string[] armorProficiencysToExclude = new string[]
        {
            "BarbarianArmorProficiency",
            "ProficiencyFighterArmor",
            "ProficiencyPaladinArmor",
            "ProficiencyWizardArmor"
        };

        private static readonly string[] armorProficiencysToInclude = new string[]
        {
            "BarbarianClassArmorProficiencyMulticlass",
            "FighterArmorProficiencyMulticlass",
            "PaladinArmorProficiencyMulticlass"
        };

        private static readonly string[] weaponProficiencysToExclude = new string[]
        {
            "BardWeaponProficiency",
            "ProficiencyClericWeapon",
            "ProficiencyRogueWeapon",
            "ProficiencyWizardWeapon",
            "ProficiencyWeaponTinkerer"
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