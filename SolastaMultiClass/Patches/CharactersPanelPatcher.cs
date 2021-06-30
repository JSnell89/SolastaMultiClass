using HarmonyLib;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    class CharactersPanelPatcher
    {
        [HarmonyPatch(typeof(CharactersPanel), "EnumeratePlates")]
        internal static class CharactersPanel_OnNewCharacterCb_Patch
        {
            internal static void Postfix()
            {
                GetHeroesPool(true);
            }
        }
    }
}