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
        OneThird,
        HalfCeiling,
        OneThirdCeiling,
    };

    class SharedSpellsRules
    {
        private class CasterLevelContext
        {
            private readonly Dictionary<CasterType, int> levels;

            public CasterLevelContext()
            {
                levels = new Dictionary<CasterType, int>
                {
                    { CasterType.OneThird, 0 },
                    { CasterType.OneThirdCeiling, 0 },
                    { CasterType.Half, 0 },
                    { CasterType.HalfCeiling, 0 },
                    { CasterType.Full, 0 }
                };
            }

            public void IncrementCasterLevel(CasterType casterType)
            {
                levels[casterType]++;
            }

            public int GetCasterLevel()
            {
                int casterLevel = 0;

                casterLevel += (int)Math.Floor(levels[CasterType.OneThird] / 3.0);
                casterLevel += (int)Math.Ceiling(levels[CasterType.OneThirdCeiling] / 3.0);
                casterLevel += (int)Math.Floor(levels[CasterType.Half] / 2.0);
                casterLevel += (int)Math.Ceiling(levels[CasterType.HalfCeiling] / 2.0);
                casterLevel += levels[CasterType.Full];

                return casterLevel;
            }
        }

        internal static string[] CasterTypeNames = new string[6]
        {
            "None",
            "Full",
            "Half",
            "One-Third",
            "Half [round up]",
            "One-Third [round up]",
        };

        internal static RulesetCharacterHero GetHero(string name)
        {
            var characterBuildingService = ServiceRepository.GetService<ICharacterBuildingService>();

            if (characterBuildingService?.HeroCharacter != null)
            {
                return characterBuildingService.HeroCharacter;
            }
            else
            {
                var gameService = ServiceRepository.GetService<IGameService>();
                var gameCampaignCharacter = gameService?.Game?.GameCampaign?.Party?.CharactersList.Find(x => x.RulesetCharacter.Name == name);

                return (RulesetCharacterHero)gameCampaignCharacter?.RulesetCharacter;
            }
        }

        public static double GetCasterLevelForGivenLevel(Dictionary<CharacterClassDefinition, int> classesAndLevels, Dictionary<CharacterClassDefinition, CharacterSubclassDefinition> classesAndSubclasses)
        {
            var context = new CasterLevelContext();

            foreach (var classAndLevel in classesAndLevels)
            {
                int numLevelsToUseFromNextClass = classAndLevel.Value;
                classesAndSubclasses.TryGetValue(classAndLevel.Key, out CharacterSubclassDefinition subclass);
                CasterType casterType = GetCasterTypeForSingleLevelOfClass(classAndLevel.Key, subclass);

                //Only increment caster level when the class in question has actually gotten their spellcasting feature.  Artificer's get spell slots at level 1 just to complicate things :).
                if (casterType == CasterType.Full || casterType == CasterType.HalfCeiling || (numLevelsToUseFromNextClass >= 2 && casterType == CasterType.Half) || (numLevelsToUseFromNextClass >= 3 && casterType == CasterType.OneThird))
                {
                    for (int i = numLevelsToUseFromNextClass; i > 0; i--)
                        context.IncrementCasterLevel(casterType);
                }
            }
            return context.GetCasterLevel();
        }

        // class caster type always take precedence over subclass caster type
        private static CasterType GetCasterTypeForSingleLevelOfClass(CharacterClassDefinition characterClassDefinition, CharacterSubclassDefinition characterSubclassDefinition)
        {
            if (Main.Settings.ClassCasterType.ContainsKey(characterClassDefinition.Name) && Main.Settings.ClassCasterType[characterClassDefinition.Name] != CasterType.None)
            {
                return Main.Settings.ClassCasterType[characterClassDefinition.Name];
            }
            return Main.Settings.SubclassCasterType[characterSubclassDefinition.Name];
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