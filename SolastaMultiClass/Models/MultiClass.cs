using System.Collections.Generic;
using System.Linq;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;

namespace SolastaMultiClass.Models
{
    static class MultiClass
    {
        private static int selectedClass = 0;

        private static RulesetCharacterHero selectedHero;

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

        public static string GetSelectedClassSearchTerm(string contains)
        {
            return contains + selectedHero.ClassesAndLevels.Keys.ToList()[selectedClass].Name;
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

        private static bool ApproveMultiClassInOut(RulesetCharacterHero hero, CharacterClassDefinition classDefinition)
        {
            var strength = hero.GetAttribute("Strength").CurrentValue;
            var dexterity = hero.GetAttribute("Dexterity").CurrentValue;
            var intelligence = hero.GetAttribute("Intelligence").CurrentValue;
            var wisdom = hero.GetAttribute("Wisdom").CurrentValue;
            var charisma = hero.GetAttribute("Charisma").CurrentValue;

            switch (classDefinition.Name)
            {
                case "AHBarbarianClass": // AceHigh's Barbarian
                case "BarbarianClass": // Holic92's Barbarian
                    return strength >= 13;

                case "AHWarlockClass": // AceHigh's Warlock
                case "BardClass": // Holic92's Bard
                case "Sorcerer":
                case "Warlock":
                    return charisma >= 13;

                case "Cleric":
                    return wisdom >= 13;

                case "Fighter":
                    return strength >= 13 || dexterity >= 13;

                case "MonkClass": // Holic92's Monk
                    return strength >= 13 && charisma >= 13;

                case "Paladin":
                    return strength >= 13 && charisma >= 13;

                case "Ranger":
                    return dexterity >= 13 && wisdom >= 13;

                case "ClassTinkerer": // CJD's Tinkerer
                    return intelligence >= 13;

                case "Rogue":
                    return dexterity >= 13;

                case "Wizard":
                    return intelligence >= 13;

                default:
                    return false;
            }
        }

        public static List<CharacterClassDefinition> GetHeroAllowedClassDefinitions(RulesetCharacterHero hero)
        {
            var allowedClasses = new List<CharacterClassDefinition>() { };
            var currentClass = hero.ClassesHistory[hero.ClassesHistory.Count - 1];

            if (!ApproveMultiClassInOut(hero, currentClass) && Main.Settings.ForceMinInOutPreReqs)
            {
                allowedClasses.Add(currentClass);
            }
            else if (hero.ClassesAndLevels.Count >= Main.Settings.MaxAllowedClasses)
            {
                foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
                {
                    if (ApproveMultiClassInOut(hero, characterClassDefinition) || !Main.Settings.ForceMinInOutPreReqs)
                    {
                        allowedClasses.Add(characterClassDefinition);
                    }
                }
            }
            else
            {
                foreach (var classDefinition in DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetAllElements())
                {
                    if (ApproveMultiClassInOut(hero, classDefinition) || !Main.Settings.ForceMinInOutPreReqs)
                    {
                        allowedClasses.Add(classDefinition);
                    }
                }
            }
            return allowedClasses;
        }
    }
}