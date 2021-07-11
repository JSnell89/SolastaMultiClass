using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SolastaModApi;
using static FeatureDefinitionCastSpell;
using SolastaMultiClass.Models;

namespace SolastaMultiClass.Models
{
    public enum CasterType
    {
        None,
        OneThird,
        Half,
        HalfArtificer,
        Full
    };

    class SharedSpells
    {
        public static RulesetCharacterHero GetHero(string name)
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

                //Only increment caster level when the class in question has actually gottent their spellcasting feature.  Artificer's get spell slots at level 1 just to complicate things :).
                if (casterType == CasterType.Full || casterType == CasterType.HalfArtificer || (numLevelsToUseFromNextClass >= 2 && casterType == CasterType.Half) || (numLevelsToUseFromNextClass >= 3 && casterType == CasterType.OneThird))
                {
                    for (int i = numLevelsToUseFromNextClass; i > 0; i--)
                        context.IncrementCasterLevel(casterType);
                }
            }

            return context.GetCasterLevel();
        }

        private static CasterType GetCasterTypeForSingleLevelOfClass(CharacterClassDefinition charClass, CharacterSubclassDefinition subclass)
        {
            if (Main.Settings.ClassCasterType.ContainsKey(charClass.Name))
            {
                return Main.Settings.ClassCasterType[charClass.Name];
            }
            if (FullCasterList.Contains(charClass))
                return CasterType.Full;
            else if (HalfCasterList.Contains(charClass))
                return CasterType.Half;
            else if (OneThirdCasterList.Contains(subclass))
                return CasterType.OneThird;

            //Fallback to get it from the class name.  TODO also do same for subclass (not necessary yet, should likely go through the class/subclass features to see if they have a spellcasting feature and get the type from that).
            return GetCasterTypeFromClassName(charClass.Name);
        }

        private static CasterType GetCasterTypeFromClassName(string className)
        {
            switch (className)
            {
                case "Cleric":
                case "Bard":
                case "BardClass": // Holic92's Bard
                case "Sorcerer":
                case "Wizard":
                    return CasterType.Full;

                case "ClassTinkerer": // CJD's Tinkerer
                    return CasterType.HalfArtificer;
                case "Paladin":
                case "Ranger":
                    return CasterType.Half;

                //Warlock is an odd case and should likely be handled completely separately
                case "Warlock":
                case "AHWarlockClass": // AceHigh's Warlock
                    return CasterType.None;

                case "AHBarbarianClass": // AceHigh's Barbarian
                case "BarbarianClass": // Holic92's Barbarian
                case "Fighter":
                case "Monk":
                case "MonkClass": // Holic92's Monk
                case "Rogue":
                    return CasterType.None;

                default:
                    return CasterType.None;
            }
        }

        public class CasterLevelContext
        {
            public CasterLevelContext()
            {
                NumOneThirdLevels = 0;
                NumHalfLevels = 0;
                NumFullLevels = 0;
            }

            //I think technically this should be split by each OneThird and each Half caster but I can look at that later.
            public void IncrementCasterLevel(CasterType casterLevelType)
            {
                if (casterLevelType == CasterType.OneThird)
                    NumOneThirdLevels++;
                if (casterLevelType == CasterType.Half)
                    NumHalfLevels++;
                if (casterLevelType == CasterType.HalfArtificer)
                    NumHalfArtificerLevels++;
                if (casterLevelType == CasterType.Full)
                    NumFullLevels++;
            }

            public double GetCasterLevel()
            {
                double casterLevel = 0;
                if (NumOneThirdLevels >= 3)
                    casterLevel += NumOneThirdLevels / 3.0;
                if (NumHalfLevels >= 2)
                    casterLevel += NumHalfLevels / 2.0;
                if (NumHalfArtificerLevels >= 1) //artificer spell level round up instead of down like other spellcasting classes
                    casterLevel += Math.Ceiling(NumHalfArtificerLevels / 2.0);

                casterLevel += NumFullLevels;

                return casterLevel;
            }

            double NumOneThirdLevels = 0;
            double NumHalfLevels = 0;
            double NumHalfArtificerLevels = 0;
            double NumFullLevels = 0;
        }

        //TODO add Bard and other potential full casters - Note Warlock should not be included handle everything on their own since pact/short rest spell slots will not be shared in any fashion with other classes.
        private static readonly CharacterClassDefinition[] FullCasterList = new CharacterClassDefinition[]
        {
                     DatabaseHelper.CharacterClassDefinitions.Cleric,
                     DatabaseHelper.CharacterClassDefinitions.Wizard,
        };

        private static readonly CharacterClassDefinition[] HalfCasterList = new CharacterClassDefinition[]
        {
                     DatabaseHelper.CharacterClassDefinitions.Paladin,
                     DatabaseHelper.CharacterClassDefinitions.Ranger,
        };

        private static readonly CharacterSubclassDefinition[] OneThirdCasterList = new CharacterSubclassDefinition[]
        {
                     DatabaseHelper.CharacterSubclassDefinitions.MartialSpellblade,
                     DatabaseHelper.CharacterSubclassDefinitions.RoguishShadowCaster,
        };

        public static readonly List<SlotsByLevelDuplet> FullCastingSlots = new List<SlotsByLevelDuplet>()
                 {
                     //Add 10th level slots that are always 0 since Solasta seems to rely on IndexOf(0) for certain things
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