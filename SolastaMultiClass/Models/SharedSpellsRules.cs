using System;
using System.Collections.Generic;
using static FeatureDefinitionCastSpell;

namespace SolastaMultiClass.Models
{
    public enum CasterType
    {
        None,
        Full,
        Half,
        HalfRoundUp,
        OneThird
    };

    class SharedSpellsRules
    {
        internal class CasterLevelContext
        {
            private readonly Dictionary<CasterType, int> levels;

            public CasterLevelContext()
            {
                levels = new Dictionary<CasterType, int>
                {
                    { CasterType.None, 0 },
                    { CasterType.Full, 0 },
                    { CasterType.Half, 0 },
                    { CasterType.HalfRoundUp, 0 },
                    { CasterType.OneThird, 0 },
                };
            }

            public void IncrementCasterLevel(CasterType casterType, int increment)
            {
                levels[casterType] += increment;
            }

            public int GetCasterLevel()
            {
                int casterLevel = 0;

                casterLevel += levels[CasterType.Full];

                if (levels[CasterType.Half] == 2)
                {
                    casterLevel += 1;
                }
                else if(levels[CasterType.Half] > 2)
                {
                    casterLevel += (int)Math.Ceiling(levels[CasterType.Half] / 2.0);
                }

                casterLevel += (int)Math.Ceiling(levels[CasterType.HalfRoundUp] / 2.0);

                if (levels[CasterType.OneThird] == 3)
                {
                    casterLevel += 1;
                }
                else if (levels[CasterType.Half] > 3)
                {
                    casterLevel += (int)Math.Ceiling(levels[CasterType.Half] / 3.0);
                }

                return casterLevel;
            }
        }

        internal static string[] CasterTypeNames = new string[5]
        {
            "None",
            "Full",
            "Half",
            "Half [round up]",
            "One-Third"
        };

        internal static RulesetCharacterHero GetHero(string name)
        {
            var characterBuildingService = ServiceRepository.GetService<ICharacterBuildingService>();

            // try to get the hero from the inspection panel context
            if (InspectionPanelContext.SelectedHero != null)
            {
                return InspectionPanelContext.SelectedHero;
            }

            // try to get the hero from the level up context
            if (LevelUpContext.SelectedHero != null)
            {
                return LevelUpContext.SelectedHero;
            }

            // try to cover the special case when heroes are built silently from template definitions
            if (characterBuildingService?.HeroCharacter != null)
            {
                return characterBuildingService.HeroCharacter;
            }

            // last resource
            var gameService = ServiceRepository.GetService<IGameService>();
            var gameCampaignCharacter = gameService?.Game?.GameCampaign?.Party?.CharactersList.Find(x => x.RulesetCharacter.Name == name);

            return (RulesetCharacterHero)gameCampaignCharacter?.RulesetCharacter;
        }

        private static bool IsWarlock(CharacterClassDefinition characterClassDefinition)
        {
            if (characterClassDefinition == null)
            {
                return false;
            }
            else
            {
                return characterClassDefinition.Name.Contains("Warlock");
            }
        }

        internal static int GetHeroSharedCasterLevel(RulesetCharacterHero rulesetCharacterHero, CharacterClassDefinition filterCharacterClassDefinition = null)
        {
            if (IsWarlock(filterCharacterClassDefinition))
            {
                // Warlock stops progressing Pact Magic Spell Levels at 10
                return Math.Min(rulesetCharacterHero.ClassesAndLevels[filterCharacterClassDefinition], 10);
            }
            else
            {
                var context = new CasterLevelContext();

                if (rulesetCharacterHero != null && rulesetCharacterHero.ClassesAndLevels != null)
                {
                    foreach (var classAndLevel in rulesetCharacterHero.ClassesAndLevels)
                    {
                        var currentCharacterClassDefinition = classAndLevel.Key;

                        // only apply a filter if one exists
                        if (filterCharacterClassDefinition == null || filterCharacterClassDefinition == currentCharacterClassDefinition)
                        {
                            rulesetCharacterHero.ClassesAndSubclasses.TryGetValue(currentCharacterClassDefinition, out CharacterSubclassDefinition characterSubclassDefinition);
                            CasterType casterType = GetCasterTypeForClassOrSubclass(currentCharacterClassDefinition, characterSubclassDefinition);
                            context.IncrementCasterLevel(casterType, classAndLevel.Value);
                        }
                    }
                }

                return context.GetCasterLevel();
            }
        }

        private static CasterType GetCasterTypeForClassOrSubclass(CharacterClassDefinition characterClassDefinition, CharacterSubclassDefinition characterSubclassDefinition)
        {
            if (characterClassDefinition != null && Main.Settings.ClassCasterType[characterClassDefinition.Name] != CasterType.None)
            {
                return Main.Settings.ClassCasterType[characterClassDefinition.Name];
            }
            if (characterSubclassDefinition != null)
            {
                return Main.Settings.SubclassCasterType[characterSubclassDefinition.Name];
            }
            return CasterType.None;
        }

        // add 10th level slots that are always 0 since game engine seems to rely on IndexOf(List.Count) for certain things
        public static readonly List<SlotsByLevelDuplet> FullCastingSlots = new List<SlotsByLevelDuplet>()
        {
            new SlotsByLevelDuplet() { Slots = new List<int> {2,0,0,0,0,0,0,0,0,0}, Level = 1 },
            new SlotsByLevelDuplet() { Slots = new List<int> {3,0,0,0,0,0,0,0,0,0}, Level = 2 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,2,0,0,0,0,0,0,0,0}, Level = 3 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,0,0,0,0,0,0,0,0}, Level = 4 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,2,0,0,0,0,0,0,0}, Level = 5 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,0,0,0,0,0,0,0}, Level = 6 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,1,0,0,0,0,0,0}, Level = 7 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,2,0,0,0,0,0,0}, Level = 8 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,1,0,0,0,0,0}, Level = 9 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,0,0,0,0,0}, Level = 10 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,0,0,0,0}, Level = 11 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,0,0,0,0}, Level = 12 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,0,0,0}, Level = 13 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,0,0,0}, Level = 14 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,1,0,0}, Level = 15 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,1,0,0}, Level = 16 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,2,1,1,1,1,0}, Level = 17 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,3,1,1,1,1,0}, Level = 18 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,3,2,1,1,1,0}, Level = 19 },
            new SlotsByLevelDuplet() { Slots = new List<int> {4,3,3,3,3,2,2,1,1,0}, Level = 20 },
        };
    }
}