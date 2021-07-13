using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    [HarmonyPatch(typeof(RulesetCharacterHero), "PostLoad")]
    internal static class RulesetCharacterHero_PostLoad_Patch
    {
        internal static void Prefix(RulesetCharacterHero __instance)
        {
            Models.GameUi.InspectionPanelBindHero(__instance);
        }
    }
}