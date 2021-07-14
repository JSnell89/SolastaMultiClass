using System.Collections.Generic;
using System.Linq;
using static SolastaMultiClass.Settings;

namespace SolastaMultiClass.Models
{
    static class GameUi
    {
        private static int selectedClass = 0;

        private static RulesetCharacterHero selectedHero;

        public static RulesetCharacterHero GetHero => selectedHero;

        public static void InspectionPanelBindHero(RulesetCharacterHero hero)
        {
            selectedHero = hero;
            selectedClass = 0;
        }

        public static void InspectionPanelUnbindHero()
        {
            selectedHero = null;
        }

        public static string GetAllSubclassesLabel(GuiCharacter character)
        {
            var allSubclassesLabel = "";
            var hero = character.RulesetCharacterHero;

            foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
                //foreach (var characterSubclassDefinition in hero.ClassesAndSubclasses.Values)
            {
                if (hero.ClassesAndSubclasses.ContainsKey(characterClassDefinition))
                {
                    allSubclassesLabel += hero.ClassesAndSubclasses[characterClassDefinition].FormatTitle() + "\n";
                }
                else
                {
                    allSubclassesLabel += characterClassDefinition.FormatTitle() + " / " + hero.ClassesAndLevels[characterClassDefinition] + "\n";
                } 
            }
            return allSubclassesLabel;
        }

        public static string GetAllClassesLabel(GuiCharacter character, string defaultLabel = "", string separator = "\n")
        {
            var allClassesLabel = "";
            var snapshot = character?.Snapshot;

            if (snapshot != null)
            {
                if (snapshot.Classes.Length == 1)
                {
                    allClassesLabel = DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetElement(snapshot.Classes[0]).FormatTitle();
                }
                else
                {
                    allClassesLabel = "Multiclass";
                }
            }
            else
            {
                var hero = character.RulesetCharacterHero;
                
                if (hero.ClassesAndLevels.Count <= 1)
                {
                    allClassesLabel = defaultLabel;
                }
                else
                {
                    foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
                    {
                        allClassesLabel += characterClassDefinition.FormatTitle() + " / " + hero.ClassesAndLevels[characterClassDefinition] + separator;
                    }
                    allClassesLabel = allClassesLabel.Substring(0, allClassesLabel.Length - separator.Length);
                }
            }
            return allClassesLabel;
        }

        public static string GetAllClassesHitDiceLabel(GuiCharacter character)
        {
            var hitDiceLabel = "";
            var dieTypesCount = new Dictionary<RuleDefinitions.DieType, int>() { };
            var hero = character?.RulesetCharacterHero;
            var separator = " ";

            if (hero != null)
            {
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

        internal static string GetSelectedClassName()
        {
            return selectedHero.ClassesAndLevels.Keys.ToList()[selectedClass].Name;
        }

        public static string GetSelectedClassSearchTerm(string contains)
        {
            return contains + GetSelectedClassName();
        }

        public static List<FightingStyleDefinition> GetTrainedFightingStyles(RulesetCharacterHero rulesetCharacterHero)
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

        public static void RegisterCommands()
        {
            var inputService = ServiceRepository.GetService<IInputService>();

            inputService.RegisterCommand(PLAIN_UP, 273, -1, -1, -1, -1, -1);
            inputService.RegisterCommand(PLAIN_DOWN, 274, -1, -1, -1, -1, -1);
            inputService.RegisterCommand(PLAIN_RIGHT, 275, -1, -1, -1, -1, -1);
            inputService.RegisterCommand(PLAIN_LEFT, 276, -1, -1, -1, -1, -1);
        }
    }
}