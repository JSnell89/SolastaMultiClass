using HarmonyLib;
using static SolastaMultiClass.Models.ClassPicker;

namespace SolastaMultiClass.Patches
{
    class GuiCharacterPatcher
    {
        [HarmonyPatch(typeof(GuiCharacter), "MainClassDefinition", MethodType.Getter)]
        internal static class GuiCharacter_MainClassDefinition_Patch
        {
            internal static void Postfix(GuiCharacter __instance, ref CharacterClassDefinition __result)
            {
                __result = GetSelectedClass(__result);
            }
        }
    }
}