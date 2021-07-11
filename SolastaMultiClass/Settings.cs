using System.Collections.Generic;
using UnityModManagerNet;
using SolastaMultiClass.Models;

namespace SolastaMultiClass
{


    public class Core
    {

    }
    
    public class Settings : UnityModManager.ModSettings
    {
        public int MaxAllowedClasses = 2;
        public bool EnableMinInOutAttributes = true;
        public bool EnableSharedSpellCasting = true;
        public bool EnableNonStackingExtraAttacks = true;

        public Dictionary<string, CasterType> ClassCasterType = new Dictionary<string, CasterType>()
        {
            { "Bard", CasterType.Full },
            { "BardClass", CasterType.Full }, // holic
            { "Cleric", CasterType.Full },
            { "Sorcerer", CasterType.Full },
            { "Wizard", CasterType.Full },
            { "Paladin", CasterType.Half },
            { "Ranger", CasterType.Half },
            { "ClassTinkerer", CasterType.HalfCeiling }, // chris
            { "BarbarianSubclassPrimalPathOfWarShaman", CasterType.OneThird }, // holic
            { "MartialEldritchKnight", CasterType.OneThird }, // holic
            { "MartialSpellblade", CasterType.OneThird },
            { "RoguishShadowCaster", CasterType.OneThird }
        };

        public const InputCommands.Id PLAIN_LEFT = (InputCommands.Id)22220003;
        public const InputCommands.Id PLAIN_RIGHT = (InputCommands.Id)22220004;
    }
}