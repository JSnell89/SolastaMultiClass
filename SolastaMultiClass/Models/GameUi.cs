using System.Collections.Generic;

namespace SolastaMultiClass.Models
{
    internal static class GameUi
    {
        internal static string GetAllSubclassesLabel(GuiCharacter character)
        {
            var allSubclassesLabel = "";
            var hero = character.RulesetCharacterHero;

            foreach (var characterClassDefinition in hero.ClassesAndLevels.Keys)
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

        internal static string GetAllClassesLabel(GuiCharacter character, string defaultLabel = "", string separator = "\n")
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

        internal static string GetAllClassesHitDiceLabel(GuiCharacter character)
        {
            var hitDiceLabel = "";
            var hero = character?.RulesetCharacterHero;
            var dieTypesCount = new Dictionary<RuleDefinitions.DieType, int>() { };
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
                }
            }
            return hitDiceLabel;
        }
    }
}