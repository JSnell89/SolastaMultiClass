using System.Collections.Generic;

namespace SolastaMultiClass.Models
{
    class ClassPicker
    {
        private static int selectedClass = 0;
        private static string hitDiceLabel = "";
        private static string allClassesLabel = "";
        private static readonly List<CharacterClassDefinition> heroClasses = new List<CharacterClassDefinition>() { };

        public static int GetClassesCount => heroClasses.Count;

        public static void CollectHeroClasses(RulesetCharacterHero hero)
        {
            selectedClass = 0;
            hitDiceLabel = "";
            allClassesLabel = "";
            heroClasses.Clear();
            heroClasses.AddRange(hero.ClassesAndLevels.Keys);
        }

        public static string GetAllClassesLabel(GuiCharacter character)
        {
            if (true) // (allClassesLabel == "")
            {
                var classesLevelCount = new Dictionary<string, int>() { };
                var hero = character.RulesetCharacterHero;
                var snapshot = character.Snapshot;

                allClassesLabel = "";
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
                    allClassesLabel += $"lvl" + classesLevelCount[className] + className + "\n";
                }
            }
            return allClassesLabel;
        }

        public static string GetAllClassesHitDiceLabel(GuiCharacter character)
        {
            if (true) // (hitDiceLabel == "")
            {
                var dieTypesCount = new Dictionary<RuleDefinitions.DieType, int>() { };
                var hero = character.RulesetCharacterHero;
                var separator = " ";

                hitDiceLabel = "";
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
    }
}