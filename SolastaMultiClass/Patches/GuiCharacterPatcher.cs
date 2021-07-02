using HarmonyLib;
using System.Collections.Generic;
using static SolastaMultiClass.Models.ClassPicker;

namespace SolastaMultiClass.Patches
{
    class GuiCharacterPatcher
    {
        [HarmonyPatch(typeof(GuiCharacter), "MainClassDefinition", MethodType.Getter)]
        internal static class GuiCharacter_MainClassDefinition_Patch
        {
            internal static bool Prefix(GuiCharacter __instance, ref CharacterClassDefinition __result)
            {
                if (__instance?.RulesetCharacterHero != null && GetClassesCount > 0)
                {
                    __result = GetSelectedClass();
                    return false;
                }
                return true;
            }
        }
            
        //[HarmonyPatch(typeof(GuiCharacter), "MainClassAndSubclassLineFeed", MethodType.Getter)]
        //internal static class GuiCharacter_MainClassAndSubclassLineFeed_Patch
        //{
        //    internal static bool Prefix(GuiCharacter __instance, ref string __result)
        //    {
        //        var character = __instance?.RulesetCharacter;

        //        if (character is RulesetCharacterHero hero && Main.Settings.maxAllowedClasses > 1)
        //        {
        //            var classesLevelCount = new Dictionary<string, int>() { };

        //            foreach (var characterClassDefinition in hero.ClassesHistory)
        //            {
        //                var title = Gui.Localize(characterClassDefinition.GuiPresentation.Title);
        //                if (!classesLevelCount.ContainsKey(title))
        //                {
        //                    classesLevelCount.Add(title, 0);
        //                }
        //                classesLevelCount[title] += 1;
        //            }
        //            __result = "";
        //            foreach (var className in classesLevelCount.Keys)
        //            {
        //                __result += $"{className} {classesLevelCount[className]:0#}\n";
        //            }
        //            return false;
        //        }
        //        return true;
        //    }
        //}
    }
}