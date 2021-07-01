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
            internal static bool Prefix(CharacterCreationScreen __instance,
                            Transform ___stagesPanelContainer,
                            GameObject[] ___stagePanelPrefabs,
                            Dictionary<string, CharacterStagePanel> ___stagePanelsByName,
                            Dictionary<string, StageDisk> ___stageDisksByName,
                            List<CharacterStagePanel> ___relevantStagePanels,
                            List<StageDisk> ___relevantStageDisks)
            {
                var raceSelectionPanel = ___stagePanelPrefabs[1];

                if (___stagesPanelContainer.transform.childCount > 0)
                {
                    Gui.ReleaseChildrenToPool(___stagesPanelContainer);
                    ___stageDisksByName.Clear();
                    ___stagePanelsByName.Clear();
                    ___relevantStageDisks.Clear();
                    ___relevantStagePanels.Clear();
                }

                foreach (GameObject stagePanelPrefab in ___stagePanelPrefabs)
                {
                    CharacterStagePanel component = Gui.GetPrefabFromPool(stagePanelPrefab, __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();
                        ___stagePanelsByName.Add(component.Name, component);
                }
                CharacterStagePanel raceComponent = Gui.GetPrefabFromPool(raceSelectionPanel, __instance.StagesPanelContainer).GetComponent<CharacterStagePanel>();
                ___stagePanelsByName.Add("MyClassSelection", raceComponent);

                return false;
            }

            internal static void Postfix(CharacterCreationScreen __instance)
            {
                classSelectionPanel = __instance.StagePanelsByName["MyClassSelection"];
                classSelectionPanelTransform = __instance.StagesPanelContainer.GetChild(__instance.StagesPanelContainer.childCount - 1);
                __instance.StagePanelsByName.Remove("MyClassSelection");
            }
        }

        [HarmonyPatch(typeof(CharacterLevelUpScreen), "LoadStagePanels")]
        internal static class CharacterLevelUpScreen_LoadStagePanels_Patch
        {
            internal static void Postfix(CharacterLevelUpScreen __instance, Dictionary<string, CharacterStagePanel> ___stagePanelsByName)
            {
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
    }
}