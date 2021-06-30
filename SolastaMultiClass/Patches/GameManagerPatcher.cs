using HarmonyLib;
using static SolastaMultiClass.Settings;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                ForceDeityOnAllClasses();

                //ServiceRepository.GetService<IInputService>().RegisterCommand(CTRL_SHIFT_RIGHT, 275, 304, 306, -1, -1, -1);
                //ServiceRepository.GetService<IInputService>().RegisterCommand(CTRL_SHIFT_LEFT, 276, 304, 306, -1, -1, -1);
                ServiceRepository.GetService<IInputService>().RegisterCommand(PLAIN_RIGHT, 275, -1, -1, -1, -1, -1);
                ServiceRepository.GetService<IInputService>().RegisterCommand(PLAIN_LEFT, 276, -1, -1, -1, -1, -1);
            }
        }
    }
}