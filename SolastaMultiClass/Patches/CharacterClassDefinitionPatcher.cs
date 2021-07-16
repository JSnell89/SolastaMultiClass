using System.Collections.Generic;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    class CharacterClassDefinitionPatcher
    {
        [HarmonyPatch(typeof(CharacterClassDefinition), "FeatureUnlocks", MethodType.Getter)]
        internal static class CharacterClassDefinition_FeatureUnlocks_Patch
        {
            internal static void Postfix(CharacterClassDefinition __instance, ref List<FeatureUnlockByLevel> __result)
            {
                if (Models.LevelUpContext.LevelingUp && Models.LevelUpContext.SelectedClassFeaturesUnlock.Count > 0)
                {
                    __result = Models.LevelUpContext.SelectedClassFeaturesUnlock;
                }
            }
        }
    }
}