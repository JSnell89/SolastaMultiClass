using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterLevelUpScreenPatcher
    {        
        // add the class selection stage panel to the level up screen
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "LoadStagePanels")]
        internal static class CharacterLevelUpScreen_LoadStagePanels_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, ref Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
            {
                var screen = Gui.GuiService.GetScreen<CharacterCreationScreen>();
                var stagePanelPrefabs = (GameObject[])AccessTools.Field(screen.GetType(), "stagePanelPrefabs").GetValue(screen);
                var classSelectionPanel = Gui.GetPrefabFromPool(stagePanelPrefabs[1], __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();
                var deitySelectionPanel = Gui.GetPrefabFromPool(stagePanelPrefabs[2], __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();

                Dictionary<string, CharacterStagePanel> stagePanelsByName = new Dictionary<string, CharacterStagePanel>
                {
                    { "ClassSelection", classSelectionPanel },
                    { "LevelGains", ___stagePanelsByName["LevelGains"] },
                    { "DeitySelection", deitySelectionPanel },
                    { "SubclassSelection", ___stagePanelsByName["SubclassSelection"] },
                    { "AbilityScores", ___stagePanelsByName["AbilityScores"] },
                    { "FightingStyleSelection", ___stagePanelsByName["FightingStyleSelection"] },
                    { "ProficiencySelection", ___stagePanelsByName["ProficiencySelection"] },
                    { "", ___stagePanelsByName[""] }
                };
                ___stagePanelsByName = stagePanelsByName;
            }
        }

        // binds the hero
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginShow")]
        internal static class CharacterLevelUpScreen_OnBeginShow_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance)
            {
                Models.LevelUpContext.SelectedHero = __instance.CharacterBuildingService.HeroCharacter;
            }     
        }

        // unbinds the hero
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginHide")]
        internal static class CharacterLevelUpScreen_OnBeginHide_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance)
            {
                Models.LevelUpContext.SelectedHero = null;
            }
        }

        // removes the wizard spell book in case it was granted
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "DoAbort")]
        internal static class CharacterLevelUpScreen_DoAbort_Patch
        {
            internal static void Prefix(CharacterLevelUpScreen __instance)
            {
                Models.LevelUpContext.UngrantSpellbookIfRequired();
            }
        }
    }
}