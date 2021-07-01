using System.Collections.Generic;

namespace SolastaMultiClass.Models
{
    class ClassPicker
    {
        private class HeroClass
        {
            public CharacterClassDefinition characterClassDefinition;
            public string title;
            public int levels;
        }

        private static int selectedClass = 0;
        private static RulesetCharacterHero hero = null;
        private static readonly List<HeroClass> heroClasses = new List<HeroClass>() { };

        public static int GetClassesCount => heroClasses.Count;

        public static void CollectHeroClasses(RulesetCharacterHero __hero)
        {
            selectedClass = 0;
            hero = __hero;
            heroClasses.Clear();
            foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
            {
                heroClasses.Add(new HeroClass()
                    {
                        characterClassDefinition = characterClassDefinition,
                        title = Gui.Localize(characterClassDefinition.GuiPresentation.Title),
                        levels = hero.ClassesAndLevels[characterClassDefinition]
                    }
                );
            }
        }

        public static string GetAllClassesLabel(GuiCharacter character)
        {
            var classLabel = "";
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
                classLabel += $"{className} / {classesLevelCount[className]:0#}\n";
            }
            return classLabel;
        }

        public static string GetAllClassesHitDiceLabel()
        {
            var hitDiceLabel = "";
            var dieTypesCount = new Dictionary<RuleDefinitions.DieType, int>() { };
            var separator = " ";

            foreach (var HeroClass in heroClasses)
            {
                var hitDice = HeroClass.characterClassDefinition.HitDice;

                if (!dieTypesCount.ContainsKey(hitDice))
                {
                    dieTypesCount.Add(hitDice, 0);
                }
                dieTypesCount[hitDice] += HeroClass.levels;
            }
            foreach(var dieType in dieTypesCount.Keys)
            {
                hitDiceLabel += dieTypesCount[dieType].ToString() + Gui.GetDieSymbol(dieType) + separator;
                separator = separator == "  " ? "\n" : " ";
            }
            return hitDiceLabel;
        }

        public static CharacterClassDefinition GetSelectedClass(CharacterClassDefinition defaultClass = null)
        {
            return heroClasses.Count == 0 ? defaultClass : heroClasses[selectedClass].characterClassDefinition;
        }

        public static string GetSelectedClassSearchTerm(string contains)
        {
            return contains + heroClasses[selectedClass].characterClassDefinition.Name;
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