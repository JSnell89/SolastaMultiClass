using System.Collections.Generic;
using SolastaModApi.Extensions;

namespace SolastaMultiClass.Models
{
    static class MultiClass
    {
        private static int selectedClass = 0;
        private static readonly List<CharacterClassDefinition> heroClasses = new List<CharacterClassDefinition>() { };

        public static int GetClassesCount => heroClasses.Count;

        public static void ForceDeityOnAllClasses()
        {
            var characterClassDefinitionDatabase = DatabaseRepository.GetDatabase<CharacterClassDefinition>();
            if (characterClassDefinitionDatabase != null)
            {
                foreach (var characterClassDefinition in characterClassDefinitionDatabase.GetAllElements())
                {
                    characterClassDefinition.SetRequiresDeity(true);
                }
            }
        }

        public static void CollectHeroClasses(RulesetCharacterHero hero = null)
        {
            selectedClass = 0;
            heroClasses.Clear();
            if (hero != null) 
            {
                heroClasses.AddRange(hero.ClassesAndLevels.Keys);
            }
        }

        public static string GetAllClassesLabel(GuiCharacter character)
        {
            var allClassesLabel = "";
            var classesLevelCount = new Dictionary<string, int>() { };
            var hero = character.RulesetCharacterHero;
            var snapshot = character.Snapshot;

            if (snapshot != null)
            {
                foreach (var className in snapshot.Classes)
                {
                    if (!classesLevelCount.ContainsKey(className))
                    {
                        classesLevelCount.Add(className, 0);
                    }
                    classesLevelCount[className] += 1;
                }
            }
            else
            {
                foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
                {
                    classesLevelCount.Add(characterClassDefinition.FormatTitle(), hero.ClassesAndLevels[characterClassDefinition]);
                }
            }
            foreach (var className in classesLevelCount.Keys)
            {
                allClassesLabel += $"lvl " + classesLevelCount[className] + " " + className + "\n";
            }
            
            return allClassesLabel;
        }

        public static string GetAllClassesHitDiceLabel(GuiCharacter character)
        {
            var hitDiceLabel = "";
            var dieTypesCount = new Dictionary<RuleDefinitions.DieType, int>() { };
            var hero = character.RulesetCharacterHero;
            var separator = " ";

            foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
            {
                if (!dieTypesCount.ContainsKey(characterClassDefinition.HitDice))
                {
                    dieTypesCount.Add(characterClassDefinition.HitDice, 0);
                }
                dieTypesCount[characterClassDefinition.HitDice] += hero.ClassesAndLevels[characterClassDefinition];
            }
            foreach (var dieType in dieTypesCount.Keys)
            {
                hitDiceLabel += dieTypesCount[dieType].ToString() + Gui.GetDieSymbol(dieType) + separator;
                separator = separator == " " ? "\n" : " ";
            }

            return hitDiceLabel;
        }

        public static CharacterClassDefinition GetSelectedClass(CharacterClassDefinition defaultClass = null)
        {
            return heroClasses.Count == 0 ? defaultClass : heroClasses[selectedClass];
        }

        public static string GetSelectedClassSearchTerm(string contains)
        {
            return contains + heroClasses[selectedClass].Name;
        }

        public static void PickPreviousClass()
        {
            selectedClass = selectedClass > 0 ? selectedClass - 1 : heroClasses.Count - 1;
        }

        public static void PickNextClass()
        {
            selectedClass = selectedClass < heroClasses.Count - 1 ? selectedClass + 1 : 0;
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
                case "BarbarianClass": // Holic92's Barbarian
                    return strength >= 13;

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
                    return intelligence >= 13 && wisdom >= 13;

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

            CollectHeroClasses(hero);

            if (!ApproveMultiClassInOut(hero, currentClass))
            {
                allowedClasses.Add(currentClass);
            }
            else if (heroClasses.Count >= Main.Settings.maxAllowedClasses)
            {
                foreach (var characterClassDefinition in heroClasses)
                {
                    if (ApproveMultiClassInOut(hero, characterClassDefinition))
                    {
                        allowedClasses.Add(characterClassDefinition);
                    }
                }
            }
            else
            {
                foreach (var classDefinition in DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetAllElements())
                {
                    if (ApproveMultiClassInOut(hero, classDefinition))
                    {
                        allowedClasses.Add(classDefinition);
                    }
                }
            }
            return allowedClasses;
        }
    }
}