using System.Collections.Generic;

namespace SolastaMultiClass.Models
{
    internal static class InOutRules
    {
        internal static void EnumerateHeroAllowedClassDefinitions(RulesetCharacterHero hero, List<CharacterClassDefinition> allowedClasses, ref int selectedClass)
        {
            var currentClass = hero.ClassesHistory[hero.ClassesHistory.Count - 1];

            allowedClasses.Clear();
            if (!ApproveMultiClassInOut(hero, currentClass) && Main.Settings.EnableMinInOutAttributes)
            {
                allowedClasses.Add(currentClass);
            }
            else if (hero.ClassesAndLevels.Count >= Main.Settings.MaxAllowedClasses)
            {
                foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
                {
                    if (ApproveMultiClassInOut(hero, characterClassDefinition) || !Main.Settings.EnableMinInOutAttributes)
                    {
                        allowedClasses.Add(characterClassDefinition);
                    }
                }
            }
            else
            {
                foreach (var classDefinition in DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetAllElements())
                {
                    if (ApproveMultiClassInOut(hero, classDefinition) || !Main.Settings.EnableMinInOutAttributes)
                    {
                        allowedClasses.Add(classDefinition);
                    }
                }
            }
            allowedClasses.Sort((a, b) => a.FormatTitle().CompareTo(b.FormatTitle()));
            selectedClass = allowedClasses.IndexOf(hero.ClassesHistory[hero.ClassesHistory.Count - 1]);
        }

        private static bool ApproveMultiClassInOut(RulesetCharacterHero hero, CharacterClassDefinition classDefinition)
        {
            var strength = hero.GetAttribute("Strength").CurrentValue;
            var dexterity = hero.GetAttribute("Dexterity").CurrentValue;
            var intelligence = hero.GetAttribute("Intelligence").CurrentValue;
            var wisdom = hero.GetAttribute("Wisdom").CurrentValue;
            var charisma = hero.GetAttribute("Charisma").CurrentValue;

            switch (classDefinition.Name)
            {
                case "Barbarian":
                case "BarbarianClass": // Holic92's Barbarian
                    return strength >= 13;

                case "BardClass": // Holic92's Bard
                case "SolastaWarlockClass": // Holic92's Warlock
                case "Bard":
                case "Sorcerer":
                case "Warlock":
                    return charisma >= 13;

                case "Cleric":
                case "Druid":
                    return wisdom >= 13;

                case "Fighter":
                    return strength >= 13 || dexterity >= 13;

                case "MonkClass": // Holic92's Monk
                case "Monk":
                case "Ranger":
                    return dexterity >= 13 && wisdom >= 13;

                case "Paladin":
                    return strength >= 13 && charisma >= 13;

                case "Rogue":
                    return dexterity >= 13;

                case "ClassTinkerer": // CJD's Tinkerer
                case "Wizard":
                    return intelligence >= 13;

                default:
                    return false;
            }
        }
    }
}