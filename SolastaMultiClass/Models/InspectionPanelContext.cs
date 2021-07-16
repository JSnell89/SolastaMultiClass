using System.Collections.Generic;
using System.Linq;
using static SolastaMultiClass.Settings;

namespace SolastaMultiClass.Models
{
    internal static class InspectionPanelContext
    {
        private static RulesetCharacterHero selectedHero;
        private static int selectedClass = 0;
        private static readonly List<string> classesWithDeity = new List<string>() { "Paladin", "Cleric" };

        internal static RulesetCharacterHero SelectedHero
        {
            get => selectedHero;
            set
            {
                selectedHero = value;
                selectedClass = 0;
            }
        }

        internal static bool RequiresDeity
        {
            get
            {
                return selectedHero.DeityDefinition != null && classesWithDeity.Contains(GetSelectedClassName());
            }
        }

        internal static CharacterClassDefinition GetSelectedClass(CharacterClassDefinition defaultClass = null)
        {
            return selectedHero == null ? defaultClass : selectedHero.ClassesAndLevels.Keys.ElementAt(selectedClass);
        }
    
        private static string GetSelectedClassName()
        {
            return GetSelectedClass().Name;
        }

        internal static List<FightingStyleDefinition> GetTrainedFightingStyles(RulesetCharacterHero rulesetCharacterHero)
        {
            var classLevelFightingStyle = new Dictionary<string, FightingStyleDefinition>() { };
            var fightingStyleIdx = 0;
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
                            classLevelFightingStyle.Add(activeFeature.Key, rulesetCharacterHero.TrainedFightingStyles[fightingStyleIdx++]);
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

        internal static void InspectionPanelPickPreviousHeroClass()
        {
            selectedClass = selectedClass > 0 ? selectedClass - 1 : selectedHero.ClassesAndLevels.Count - 1;
        }

        internal static void InspectionPanelPickNextHeroClass()
        {
            selectedClass = selectedClass < selectedHero.ClassesAndLevels.Count - 1 ? selectedClass + 1 : 0;
        }

        internal static void RegisterCommands()
        {
            var inputService = ServiceRepository.GetService<IInputService>();

            inputService.RegisterCommand(PLAIN_UP, 273, -1, -1, -1, -1, -1);
            inputService.RegisterCommand(PLAIN_DOWN, 274, -1, -1, -1, -1, -1);
            inputService.RegisterCommand(PLAIN_RIGHT, 275, -1, -1, -1, -1, -1);
            inputService.RegisterCommand(PLAIN_LEFT, 276, -1, -1, -1, -1, -1);
        }

        // used in a transpiler
        public static string GetSelectedClassSearchTerm(string original)
        {
            return original + GetSelectedClassName();
        }
    }
}