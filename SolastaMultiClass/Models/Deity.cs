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

            //Should we consider just checking DeityDefinition for all classes that need a Deity?
            //Currently AHWarlock abuses Deity and puts the subclasses under the Deity so that's why Warlock needs it right now.
            if(string.Equals(selectedClass.Name, "AHWarlockClass") && hero.DeityDefinition == null)
                characterBuildingService.AssignDeity(GetDeityFromIndex(Main.Settings.SelectedDeity));
            else if (selectedClass == Paladin && !hero.ClassesAndLevels.ContainsKey(Cleric) || selectedClass == Cleric && !hero.ClassesAndLevels.ContainsKey(Paladin))
            {
                characterBuildingService.AssignDeity(GetDeityFromIndex(Main.Settings.SelectedDeity));
            }
        }
    }
}