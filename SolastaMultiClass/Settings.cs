using System.Collections.Generic;
using UnityModManagerNet;

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
        //public bool TurnOffSpellPreparationRestrictions = false;
        public bool EnableNonStackingExtraAttacks = true;

        public Dictionary<string, string> ClassCasterType = new Dictionary<string, string>()
        {
            { "Bard", "Full" },
            { "BardClass", "Full" },
            { "Cleric", "Full" },
            { "Sorcerer", "Full" },
            { "Wizard", "Full" },
            { "Paladin", "Half" },
            { "Ranger", "Half" },
            { "ClassTinkerer", "HalfCeiling" },
            { "MartialSpellblade", "OneThird" },
            { "RoguishShadowCaster", "OneThird" }
        };

        public const InputCommands.Id PLAIN_LEFT = (InputCommands.Id)22220003;
        public const InputCommands.Id PLAIN_RIGHT = (InputCommands.Id)22220004;
    }
}