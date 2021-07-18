using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class GuiCharacterPatcher
    {
        [HarmonyPatch(typeof(GuiCharacter), "MainClassDefinition", MethodType.Getter)]
        internal static class GuiCharacter_MainClassDefinition_Patch
        {
            internal static bool Prefix(ref CharacterClassDefinition __result)
            {
                __result = Models.InspectionPanelContext.GetSelectedClass( __result);
                return false;
            }
        }

        [HarmonyPatch(typeof(GuiCharacter), "LevelAndClassAndSubclass", MethodType.Getter)]
        internal static class GuiCharacter_LevelAndClassAndSubclass_Patch
        {
            internal static bool Postfix(GuiCharacter __instance, ref string __result)
            {
                __result = Models.GameUi.GetAllClassesLabel(__instance, __result, " - ");
                return false;
            }
        }

        [HarmonyPatch(typeof(GuiCharacter), "ClassAndLevel", MethodType.Getter)]
        internal static class GuiCharacter_ClassAndLevel_Patch
        {
            internal static bool Postfix(GuiCharacter __instance, ref string __result)
            {
                __result = Models.GameUi.GetAllClassesLabel(__instance, __result, " - ");
                return false;
            }
        }
    }
}