using HarmonyLib;
using static SolastaMultiClass.Settings;
using static SolastaMultiClass.Models.MultiClass;
using UnityEngine;
using System.Collections.Generic;

namespace SolastaMultiClass.Patches
{
    internal static class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                ServiceRepository.GetService<IInputService>().RegisterCommand(PLAIN_RIGHT, 275, -1, -1, -1, -1, -1);
                ServiceRepository.GetService<IInputService>().RegisterCommand(PLAIN_LEFT, 276, -1, -1, -1, -1, -1);
                ForceDeityOnAllClasses();
            }
        }

        private static CharacterStagePanel classSelectionPanel;
        private static Transform classSelectionPanelTransform;

        [HarmonyPatch(typeof(CharacterCreationScreen), "LoadStagePanels")]
        internal static class CharacterCreationScreen_LoadStagePanels_Patch
        {
            internal static void Postfix(CharacterCreationScreen __instance)
            {
                Main.Log("CharacterCreationScreen");
                classSelectionPanel = __instance.StagePanelsByName["ClassSelection"];
                classSelectionPanelTransform = __instance.StagesPanelContainer.GetChild(1);
            }
        }

        [HarmonyPatch(typeof(CharacterLevelUpScreen), "LoadStagePanels")]
        internal static class CharacterLevelUpScreen_LoadStagePanels_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
            {
                var screen = Gui.GuiService.GetScreen<CharacterCreationScreen>();



                Dictionary<string, CharacterStagePanel> stagePanelsByName = new Dictionary<string, CharacterStagePanel>// { };
                {
                    { "ClassSelection", classSelectionPanel }
                };
                //classSelectionPanel = null;
                foreach (var stagePanelName in ___stagePanelsByName)
                {
                    stagePanelsByName.Add(stagePanelName.Key, stagePanelName.Value);
                }
                ___stagePanelsByName.Clear();
                foreach (var stagePanelName in stagePanelsByName)
                {
                    ___stagePanelsByName.Add(stagePanelName.Key, stagePanelName.Value);
                }

                classSelectionPanelTransform.SetParent(__instance.StagesPanelContainer);
            }
        }

        [HarmonyPatch(typeof(CharacterCreationScreen), "OnBeginShow")]
        internal static class CharacterCreationScreen_OnBeginShow_Patch
        {
            internal static void Prefix(CharacterCreationScreen __instance)
            {
                //classSelectionPanelTransform.SetParent(__instance.StagesPanelContainer);
            }
        }

        //[HarmonyPatch(typeof(CharacterLevelUpScreen), "OnBeginShow")]
        //internal static class CharacterLevelUpScreen_OnBeginShow_Patch
        //{
        //    internal static void Prefix(CharacterLevelUpScreen __instance)
        //    {
        //        classSelectionPanelTransform.SetParent(__instance.StagesPanelContainer);
        //    }
        //}
    }
}