using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterLevelUpScreenPatcher
    {        
        // add the class selection stage panel to the Level Up screen
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "LoadStagePanels")]
        internal static class CharacterLevelUpScreen_LoadStagePanels_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, ref Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
            {
                var screen = Gui.GuiService.GetScreen<CharacterCreationScreen>();
                var stagePanelPrefabs = (GameObject[])AccessTools.Field(screen.GetType(), "stagePanelPrefabs").GetValue(screen);
                var classSelectionStagePanelPrefab = stagePanelPrefabs[1];
                var deitySelectionStagePanelPrefab = stagePanelPrefabs[2];
                var classSelectionPanel = Gui.GetPrefabFromPool(classSelectionStagePanelPrefab, __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();
                var deitySelectionPanel = Gui.GetPrefabFromPool(deitySelectionStagePanelPrefab, __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();

                Dictionary<string, CharacterStagePanel> stagePanelsByName = new Dictionary<string, CharacterStagePanel>
                {
                    { "ClassSelection", classSelectionPanel }
                };
                var idx = 0;
                foreach (var stagePanelName in ___stagePanelsByName)
                {
                    stagePanelsByName.Add(stagePanelName.Key, stagePanelName.Value);
                    if (++idx == 1)
                    {
                        stagePanelsByName.Add("DeitySelection", deitySelectionPanel);
                    }
                }
                ___stagePanelsByName = stagePanelsByName;
            }
        }

        // bind the hero
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginShow")]
        internal static class CharacterLevelUpScreen_OnBeginShow_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance)
            {
                Models.LevelUpContext.SelectedHero = __instance.CharacterBuildingService.HeroCharacter;
            }     
        }

        // unbind the hero
        [HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginHide")]
        internal static class CharacterLevelUpScreen_OnBeginHide_Patch
        {
            internal static void Postfix()
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