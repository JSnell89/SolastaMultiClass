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

        public static void FixMulticlassProficiencies(CharacterClassDefinition selectedClass, ref List<FeatureUnlockByLevel> featureUnlockByLevels)
        {
            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();
            var result = new List<FeatureUnlockByLevel>() { };
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

            result.AddRange(featureUnlockByLevels);

            foreach (var grouptoExclude in groupsToExclude)
            {
                foreach (var featureName in grouptoExclude)
                {
                    if (featureName.Contains(selectedClass.Name) && featuresDb.TryGetElement(featureName, out FeatureDefinition feature))
                    {
                        result.RemoveAll(x => x.Level == 1 && x.FeatureDefinition == feature);
                    }
                }
            }

            foreach (var grouptoInclude in groupsToInclude)
            {
                foreach (var featureName in grouptoInclude)
                {
                    if (featureName.Contains(selectedClass.Name) && featuresDb.TryGetElement(featureName, out FeatureDefinition feature))
                    {
                        result.Add(new FeatureUnlockByLevel(feature, 1));
                    }
                }
            }

            featureUnlockByLevels = result;
        }

        private static bool CannotAddExtraAttack()
        {
            var service = ServiceRepository.GetService<CharacterBuildingManager>();
            var hero = service?.HeroCharacter;
            var hasExtraAttack = false;

            if (hero != null)
            {
                foreach (var classAndLevel in hero.ClassesAndLevels)
                {
                    var className = classAndLevel.Key.Name;
                    if (className != hero.ClassesHistory[hero.ClassesHistory.Count - 1].Name && classAndLevel.Value >= 5 && extraAttacksClassNames.Contains(className))
                    {
                        hasExtraAttack = true;
                    }
                }
            }

            return hasExtraAttack;
        }

        public static void FixExtraAttacks(CharacterClassDefinition selectedClass, ref List<FeatureUnlockByLevel> featureUnlockByLevels)
        {
            if (Main.Settings.AllowExtraAttacksToStack) return;

            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();
            var result = new List<FeatureUnlockByLevel>() { };

            result.AddRange(featureUnlockByLevels);

            foreach (var featureName in extraAttacksToExclude)
            {
                if (featureName.Contains(selectedClass.Name) && CannotAddExtraAttack() && featuresDb.TryGetElement(featureName, out FeatureDefinition feature))
                {
                    result.RemoveAll(x => x.Level == 5 && x.FeatureDefinition == feature);
                }
            }

            featureUnlockByLevels = result;
        }

        private static readonly string[] extraAttacksToExclude = new string[]
        {
            "BarbarianClassExtraAttack",
            "MonkClassExtraAttack",
            "AttributeModifierFighterExtraAttack",
            "AttributeModifierRangerExtraAttack",
            "AttributeModifierPaladinExtraAttack"
        };

        private static readonly List<string> extraAttacksClassNames = new List<string>
        {
            "BarbarianClass",
            "MonkClass",
            "Fighter",
            "Ranger",
            "Paladin"
        };

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
            "ProficiencySorcererSavingThrow",
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
            "PointPoolSorcererSkillPoints",
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
            "ProficiencySorcererWeapon",
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