using System.Collections.Generic;

namespace SolastaMultiClass.Models
{
    static class Rules
    {
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
    }
}