using UnityModManagerNet;

namespace SolastaMultiClass
{
    public class Core
    {

    }
    
    public class Settings : UnityModManager.ModSettings
    {
        public int MaxAllowedClasses = 2;
        public bool ForceMinInOutPreReqs = true;
        public bool EnableSharedSpellCasting = true;
        public bool TurnOffSpellPreparationRestrictions = false;
        public bool AllowExtraAttacksToStack = false;

        public const InputCommands.Id PLAIN_LEFT = (InputCommands.Id)22220003;
        public const InputCommands.Id PLAIN_RIGHT = (InputCommands.Id)22220004;
    }
}