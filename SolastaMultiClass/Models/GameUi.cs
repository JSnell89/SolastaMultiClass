﻿using System.Collections.Generic;
using System.Linq;

namespace SolastaMultiClass.Models
{
    static class GameUi
    {
        private static int selectedClass = 0;

        private static RulesetCharacterHero selectedHero;

        public static void InspectionPanelBindHero(RulesetCharacterHero hero)
        {
            selectedHero = hero;
        }

        public static void InspectionPanelUnbindHero()
        {
            selectedHero = null;
        }

        public static string GetAllClassesLabel(GuiCharacter character)
        {
            var allClassesLabel = "";
            var snapshot = character.Snapshot;

            if (snapshot != null)
            {
                allClassesLabel = DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetElement(snapshot.Classes[0]).FormatTitle();
            }
            else
            {
                var hero = character.RulesetCharacterHero;

                foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
                {
                    allClassesLabel += characterClassDefinition.FormatTitle() + " / " + hero.ClassesAndLevels[characterClassDefinition] + "\n";
                }
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

        internal static string GetSelectedClassName()
        {
            return selectedHero.ClassesAndLevels.Keys.ToList()[selectedClass].Name;
        }

        public static string GetSelectedClassSearchTerm(string contains)
        {
            return contains + GetSelectedClassName();
        }

        public static List<FightingStyleDefinition> GetClassBadges(RulesetCharacterHero rulesetCharacterHero)
        {
            var classLevelFightingStyle = new Dictionary<string, FightingStyleDefinition>() { };
            var fightingStyleidx = 0;
            var className = GetSelectedClassName();
            var classBadges = new List<FightingStyleDefinition>() { };

            foreach (var activeFeature in rulesetCharacterHero.ActiveFeatures)
            {
                if (activeFeature.Key.Contains(AttributeDefinitions.TagClass))
                {
                    foreach (FeatureDefinition featureDefinition in activeFeature.Value)
                    {
                        if (featureDefinition is FeatureDefinitionFightingStyleChoice featureDefinitionFightingStyleChoice)
                        {
                            classLevelFightingStyle.Add(activeFeature.Key, rulesetCharacterHero.TrainedFightingStyles[fightingStyleidx++]);
                        }
                    }
                }
            }
            foreach (var tuple in classLevelFightingStyle)
            {
                if (tuple.Key.Contains(className))
                {
                    classBadges.Add(tuple.Value);
                }
            }
            return classBadges;
        }

        public static CharacterClassDefinition GetSelectedClass(CharacterClassDefinition defaultClass = null)
        {
            return selectedHero == null ? defaultClass : selectedHero.ClassesAndLevels.Keys.ToList()[selectedClass];
        }

        public static void InspectionPanelPickPreviousHeroClass()
        {
            selectedClass = selectedClass > 0 ? selectedClass - 1 : selectedHero.ClassesAndLevels.Count - 1;
        }

        public static void InspectionPanelPickNextHeroClass()
        {
            selectedClass = selectedClass < selectedHero.ClassesAndLevels.Count - 1 ? selectedClass + 1 : 0;
        }
    }
}