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

        private static DeityDefinition GetDeityFromIndex(int index)
        {
            return DatabaseRepository.GetDatabase<DeityDefinition>().GetAllElements()[index];
        }

        public static void AssignDeityIfRequired(ICharacterBuildingService characterBuildingService, CharacterClassDefinition selectedClass)
        {
            var hero = characterBuildingService.HeroCharacter;

            if ((selectedClass.Name == "Paladin" || selectedClass.Name == "Cleric" || selectedClass.Name == "AHWarlockClass") && hero.DeityDefinition == null)
            {
                characterBuildingService.AssignDeity(GetDeityFromIndex(Main.Settings.SelectedDeity));
            }
        }
    }
}