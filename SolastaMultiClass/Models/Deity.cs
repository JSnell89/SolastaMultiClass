using System.Collections.Generic;

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
            if (selectedClass.RequiresDeity && characterBuildingService.HeroCharacter.DeityDefinition == null)
            {
                characterBuildingService.AssignDeity(GetDeityFromIndex(Main.Settings.SelectedDeity));
            }
        }
    }
}