using HarmonyLib;
using static SolastaMultiClass.Models.GameUi;

namespace SolastaMultiClass.Patches
{
    internal static class GuiCharacterPatcher
    {
        [HarmonyPatch(typeof(GuiCharacter), "MainClassDefinition", MethodType.Getter)]
        internal static class GuiCharacter_MainClassDefinition_Patch
        {
            internal static void Postfix(ref CharacterClassDefinition __result)
            {
                __result = GetSelectedClass( __result);
            }
        }

        [HarmonyPatch(typeof(GuiCharacter), "LevelAndClassAndSubclass", MethodType.Getter)]
        internal static class GuiCharacter_LevelAndClassAndSubclass_Patch
        {
            internal static void Postfix(GuiCharacter __instance, ref string __result)
            {
                var separator = " - ";
                var label = GetAllClassesLabel(__instance, __result).Replace("\n", separator);
                __result = label.Substring(0, label.Length - separator.Length);
            }
        }
    }
}