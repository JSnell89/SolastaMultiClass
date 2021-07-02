using UnityModManagerNet;

namespace SolastaMultiClass
{
    public class Core
    {

    }
    
    public class Settings : UnityModManager.ModSettings
    {
        public int maxAllowedClasses = 2;
        public bool ForceMinInOutPreReqs = true;
        public bool EnableSharedSpellCasting = true;

        public const InputCommands.Id PLAIN_LEFT = (InputCommands.Id)22220003;
        public const InputCommands.Id PLAIN_RIGHT = (InputCommands.Id)22220004;
    }
}