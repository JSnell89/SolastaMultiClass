using HarmonyLib;
using static SolastaMultiClass.Models.GameUi;

namespace SolastaMultiClass.Patches
{
    class CharacterPlateDetailedPatcher
    {
        [HarmonyPatch(typeof(CharacterPlateDetailed), "OnPortraitShowed")]
        internal static class CharacterPlateDetailed_OnPortraitShowed_Patch
        {
            internal static void Postfix(CharacterPlateDetailed __instance, GuiLabel ___classLabel)
            {
                ___classLabel.Text = GetAllClassesLabel(__instance.GuiCharacter);
            }
        }
    }
}