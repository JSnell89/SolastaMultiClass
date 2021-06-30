using HarmonyLib;
using static SolastaMultiClass.Models.ClassPicker;

namespace SolastaMultiClass.Patches
{
    class CharacterPlateDetailedPatcher
    {
        [HarmonyPatch(typeof(CharacterPlateDetailed), "OnPortraitShowed")]
        internal static class CharacterPlateDetailed_OnPortraitShowed_Patch
        {
            internal static void Postfix(CharacterPlateDetailed __instance, GuiLabel ___classLabel)
            {
                if (__instance?.GuiCharacter.RulesetCharacterHero != null && GetClassesCount > 1)
                {
                    ___classLabel.Text = GetAllClassesLabel(__instance.GuiCharacter);
                }
            }
        }
    }
}