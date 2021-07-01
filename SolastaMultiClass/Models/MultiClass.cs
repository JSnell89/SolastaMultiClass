using System.Collections.Generic;
using SolastaModApi.Extensions;

namespace SolastaMultiClass.Models
{
    class MultiClass
    {
        //private static readonly List<string> classNames = new List<string>();
        //private static readonly List<string> classTitles = new List<string>();
        //private static readonly List<RulesetCharacterHero.Snapshot> heroesPool = new List<RulesetCharacterHero.Snapshot>();
        //private static readonly Dictionary<string, string> heroesSelectedClass = new Dictionary<string, string> { };

        //public static List<string> GetClassTitles()
        //{
        //    if (classTitles.Count == 0)
        //    {
        //        var characterClassDefinitionDatabase = DatabaseRepository.GetDatabase<CharacterClassDefinition>();

        //        if (characterClassDefinitionDatabase != null)
        //        {
        //            foreach (var characterClassDefinition in characterClassDefinitionDatabase.GetAllElements())
        //            {
        //                classTitles.Add(characterClassDefinition.FormatTitle());
        //                classNames.Add(characterClassDefinition.Name);
        //            }
        //        }
        //    }
        //    return classTitles;
        //}

        //private static string GetHeroFullName(RulesetCharacterHero hero)
        //{
        //    return hero.Name + hero.SurName;
        //}

        //private static string GetHeroFullName(RulesetCharacterHero.Snapshot snapshot)
        //{
        //    return snapshot.Name + snapshot.SurName;
        //}

        //private static void SetHeroSelectedClassFromName(RulesetCharacterHero.Snapshot snapshot, string className)
        //{
        //    heroesSelectedClass.AddOrReplace(GetHeroFullName(snapshot), className);
        //}

        //public static void SetHeroSelectedClassFromTitle(RulesetCharacterHero.Snapshot snapshot, string classTitle)
        //{
        //    var selected = classTitles.FindIndex(x => x == classTitle);
        //    var className = classNames[selected];
        //    SetHeroSelectedClassFromName(snapshot, className);
        //}

        //private static string GetHeroSelectedClassName(RulesetCharacterHero.Snapshot snapshot)
        //{
        //    var heroFullName = GetHeroFullName(snapshot);

        //    if (!heroesSelectedClass.ContainsKey(heroFullName))
        //    {
        //        heroesSelectedClass.Add(heroFullName, snapshot.Classes[snapshot.Classes.Length - 1]);
        //    }

        //    return heroesSelectedClass[heroFullName];
        //}

        //public static string GetHeroSelectedClassTitle(RulesetCharacterHero.Snapshot snapshot)
        //{
        //    var className = GetHeroSelectedClassName(snapshot);
        //    var index = classNames.FindIndex(x => x == className);

        //    return classTitles[index];
        //}

        //public static void GetHeroSelectedClassAndLevel(out CharacterClassDefinition lastClassDefinition, out int level)
        //{
        //    var characterBuildingService = ServiceRepository.GetService<ICharacterBuildingService>();
        //    var hero = characterBuildingService.HeroCharacter;
        //    var snapshot = new RulesetCharacterHero.Snapshot();

        //    hero.FillSnapshot(snapshot, true);
        //    lastClassDefinition = DatabaseRepository.GetDatabase<CharacterClassDefinition>().GetElement(GetHeroSelectedClassName(snapshot));
        //    level = hero.ClassesHistory.Count;
        //}

        //public static List<RulesetCharacterHero.Snapshot> GetHeroesPool(bool isDirty = false)
        //{
        //    if (isDirty)
        //    {
        //        heroesPool.Clear();
        //    }
        //    if (heroesPool.Count == 0)    
        //    {
        //        var characterPoolService = ServiceRepository.GetService<ICharacterPoolService>();

        //        if (characterPoolService?.Pool != null)
        //        {
        //            heroesPool.AddRange(characterPoolService.Pool.Values);
        //        }
        //    }
        //    return heroesPool;
        //}

        //public static List<RulesetCharacterHero.Snapshot> GetHeroesParty()
        //{
        //    var gameService = ServiceRepository.GetService<IGameService>();
        //    var heroesPool = new List<RulesetCharacterHero.Snapshot>();

        //    if (gameService?.Game != null)
        //    {
        //        foreach(var gameCampaignCharacter in gameService.Game.GameCampaign.Party.CharactersList)
        //        {
        //            var hero = (RulesetCharacterHero)gameCampaignCharacter.RulesetCharacter;
        //            var snapshot = new RulesetCharacterHero.Snapshot();
                    
        //            hero.FillSnapshot(snapshot, true);
        //            heroesPool.Add(snapshot);
        //        }
        //    }
        //    return heroesPool;
        //}

        public static void ForceDeityOnAllClasses()
        {
            var characterClassDefinitionDatabase = DatabaseRepository.GetDatabase<CharacterClassDefinition>();
            if (characterClassDefinitionDatabase != null)
            {
                foreach (var characterClassDefinition in characterClassDefinitionDatabase.GetAllElements())
                {
                    characterClassDefinition.SetRequiresDeity(true);
                }
            }
        }

        //private static bool ApproveMultiClassInOut(RulesetCharacterHero hero, string classTitle)
        //{
        //    var strength = hero.GetAttribute("Strength").CurrentValue;
        //    var dexterity = hero.GetAttribute("Dexterity").CurrentValue;
        //    var constitution = hero.GetAttribute("Constitution").CurrentValue;
        //    var intelligence = hero.GetAttribute("Intelligence").CurrentValue;
        //    var wisdom = hero.GetAttribute("Wisdom").CurrentValue;
        //    var charisma = hero.GetAttribute("Charisma").CurrentValue;

        //    switch (classTitle)
        //    {
        //        case "Barbarian":
        //            return strength >= 13;

        //        case "Bard":
        //            return charisma >= 13;

        //        case "Cleric":
        //            return wisdom >= 13;

        //        case "Fighter":
        //            return strength >= 13 || dexterity >= 13;

        //        case "Paladin":
        //            return strength >= 13 && charisma >= 13;

        //        case "Ranger":
        //            return dexterity >= 13 && wisdom >= 13;

        //        case "Tinkerer":
        //            return intelligence >= 13 && wisdom >= 13;

        //        case "Rogue":
        //            return dexterity >= 13;

        //        case "Wizard":
        //            return intelligence >= 13;

        //        default:
        //            return false;
        //    }
        //}
    }
}