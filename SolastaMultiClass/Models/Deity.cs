using System.Collections.Generic;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;

namespace SolastaMultiClass.Models
{
    static class Deity
    {
        private static readonly List<string> deityList = new List<string>() { };

        public static List<string> GetDeityList()
        {
            if (deityList.Count == 0)
            {
                var database = DatabaseRepository.GetDatabase<DeityDefinition>();

                if (database != null)
                {
                    foreach (var deityDefinition in database.GetAllElements())
                    {
                        deityList.Add(deityDefinition.FormatTitle());
                    }
                }

            }
            return deityList;
        }

        public static DeityDefinition GetDeityFromIndex(int index)
        {
            return DatabaseRepository.GetDatabase<DeityDefinition>().GetAllElements()[index];
        }

        public static void AssignDeityIfRequired(ICharacterBuildingService characterBuildingService)
        {
            var hero = characterBuildingService.HeroCharacter;
            var selectedClass = hero.ClassesHistory[hero.ClassesHistory.Count - 1];

            if (selectedClass == Paladin && !hero.ClassesAndLevels.ContainsKey(Cleric) || 
                selectedClass == Cleric && !hero.ClassesAndLevels.ContainsKey(Paladin))
            {
                characterBuildingService.AssignDeity(GetDeityFromIndex(Main.Settings.SelectedDeity));
            }
        }
    }
}