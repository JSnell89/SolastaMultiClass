using HarmonyLib;
using static SolastaMultiClass.Settings;

namespace SolastaMultiClass.Patches
{
    internal static class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                var inputService = ServiceRepository.GetService<IInputService>();

                inputService.RegisterCommand(PLAIN_RIGHT, 275, -1, -1, -1, -1, -1);
                inputService.RegisterCommand(PLAIN_LEFT, 276, -1, -1, -1, -1, -1);
            }
        }
    }
}