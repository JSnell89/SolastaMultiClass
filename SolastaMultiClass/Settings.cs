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

        public SerializableDictionary<string, CasterType> ClassCasterType = new SerializableDictionary<string, CasterType>()
        {
            { "Cleric", CasterType.Full },
            { "Sorcerer", CasterType.Full },
            { "Wizard", CasterType.Full },
            { "Paladin", CasterType.Half },
            { "Ranger", CasterType.Half },
            // modders' classes
            { "BardClass", CasterType.Full }, // holic92
            { "ClassTinkerer", CasterType.HalfRoundUp }, // ChrisJohnDigital
        };

        public SerializableDictionary<string, CasterType> SubclassCasterType = new SerializableDictionary<string, CasterType>()
        {
            { "MartialSpellblade", CasterType.OneThird },
            { "RoguishShadowCaster", CasterType.OneThird },
            // modders' subclasses
            { "BarbarianSubclassPrimalPathOfWarShaman", CasterType.OneThird }, // holic92
            { "MartialEldritchKnight", CasterType.OneThird }, // holic92
            { "RoguishConArtist", CasterType.OneThird }, // ChrisJohnDigital
            { "FighterSpellShield", CasterType.OneThird }, // ChrisJohnDigital
        };

        public const InputCommands.Id PLAIN_LEFT = (InputCommands.Id)22220003;
        public const InputCommands.Id PLAIN_RIGHT = (InputCommands.Id)22220004;
        public const InputCommands.Id PLAIN_UP = (InputCommands.Id)22220005;
        public const InputCommands.Id PLAIN_DOWN = (InputCommands.Id)22220006;
    }
}