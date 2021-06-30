using System;
using System.Collections.Generic;

namespace SolastaMultiClass.Models
{
    class ClassPicker
    {
        class ClassCount
        {
            public CharacterClassDefinition characterClassDefinition;
            public string title;
            public int levels;
        }

        private static int selectedClass = 0;
        private static RulesetCharacterHero hero = null;
        private static readonly List<ClassCount> heroClasses = new List<ClassCount>() { };

        public static int GetClassesCount => heroClasses.Count;

        //public static void CollectHeroClasses(string name)
        //{
        //    var characterPoolService = ServiceRepository.GetService<ICharacterPoolService>();

        //    characterPoolService.LoadCharacter(
        //            characterPoolService.BuildCharacterFilename(name.Substring(0, name.Length - 4)),
        //            out RulesetCharacterHero hero,
        //            out RulesetCharacterHero.Snapshot snapshot);
        //    CollectHeroClasses(hero);
        //}

        public static void CollectHeroClasses(RulesetCharacterHero __hero)
        {
            selectedClass = 0;
            hero = __hero;
            heroClasses.Clear();
            foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
            {
                var title = Gui.Localize(characterClassDefinition.GuiPresentation.Title);
                heroClasses.Add(new ClassCount()
                    {
                        characterClassDefinition = characterClassDefinition,
                        title = title,
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
                    var title = Gui.Localize(characterClassDefinition.GuiPresentation.Title);
                    classesLevelCount.Add(title, hero.ClassesAndLevels[characterClassDefinition]);
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
            var separator = " ";
            var dieTypesCount = new Dictionary<RuleDefinitions.DieType, int>() { };

            foreach (var classCount in heroClasses)
            {
                var hitDice = classCount.characterClassDefinition.HitDice;

                if (!dieTypesCount.ContainsKey(hitDice))
                {
                    dieTypesCount.Add(hitDice, 0);
                }
                dieTypesCount[hitDice] += classCount.levels;
            }
            foreach(var dieType in dieTypesCount.Keys)
            {
                hitDiceLabel += dieTypesCount[dieType].ToString() + Gui.GetDieSymbol(dieType) + separator;
                separator = separator == "  " ? "\n" : " ";
            }
            return hitDiceLabel;
        }

        public static string GetSingleClassLabel()
        {
            return heroClasses[selectedClass].title;
        }

        //public static string GetSingleClassDescription()
        //{
        //    return heroClasses[selectedClass].characterClassDefinition.FormatDescription();
        //}

        public static CharacterClassDefinition GetSelectedClass()
        {
            return heroClasses[selectedClass].characterClassDefinition;
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