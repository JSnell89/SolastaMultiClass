using HarmonyLib;
using static SolastaMultiClass.Settings;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    internal static class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                ServiceRepository.GetService<IInputService>().RegisterCommand(PLAIN_RIGHT, 275, -1, -1, -1, -1, -1);
                ServiceRepository.GetService<IInputService>().RegisterCommand(PLAIN_LEFT, 276, -1, -1, -1, -1, -1);
                ForceDeityOnAllClasses();
            }
        }
    }
}