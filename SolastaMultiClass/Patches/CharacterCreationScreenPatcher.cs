using HarmonyLib;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    class CharacterCreationScreenPatcher
    {
        [HarmonyPatch(typeof(CharacterCreationScreen), "OnBeginHide")]
        internal static class CharacterCreationScreen_OnBeginHides_Patch
        {
            internal static void Prefix()
            {
                GetHeroesPool(true);
            }
        }
    }
}