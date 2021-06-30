using UnityModManagerNet;

namespace SolastaMultiClass
{
    public class Core
    {

    }
    
    public class Settings : UnityModManager.ModSettings
    {
        public int maxAllowedClasses = 2;

        public const InputCommands.Id CTRL_SHIFT_LEFT = (InputCommands.Id)22220001;
        public const InputCommands.Id CTRL_SHIFT_RIGHT = (InputCommands.Id)22220002;
        public const InputCommands.Id PLAIN_LEFT = (InputCommands.Id)22220003;
        public const InputCommands.Id PLAIN_RIGHT = (InputCommands.Id)22220004;
    }
}