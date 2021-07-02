using HarmonyLib;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    class CharacterPlateDetailedPatcher
    {
        [HarmonyPatch(typeof(CharacterPlateDetailed), "OnPortraitShowed")]
        internal static class CharacterPlateDetailed_OnPortraitShowed_Patch
        {
            internal static void Postfix(CharacterPlateDetailed __instance, GuiLabel ___classLabel)
            {
                if (__instance?.GuiCharacter.RulesetCharacterHero?.ClassesHistory.Count > 1 && GetClassesCount > 1)
                {
                    ___classLabel.Text = GetAllClassesLabel(__instance.GuiCharacter);
                }
            }
        }
    }
}