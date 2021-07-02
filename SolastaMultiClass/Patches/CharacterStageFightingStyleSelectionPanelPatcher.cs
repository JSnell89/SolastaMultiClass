using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterStageFightingStyleSelectionPanelPatcher
    {
        internal static int previouslySelectedFightingStyle = -1;

        internal static void UntrainPreviouslySelectedFightStyle(CharacterStageFightingStyleSelectionPanel __instance, FightingStyleDefinition fightingStyleDefinition)
        {
            var x = 0;
        }
        
        internal static void RefreshNow(CharacterStageFightingStyleSelectionPanel __instance)
        {
            __instance.CommonData.AbilityScoresListingPanel.RefreshNow();
            __instance.CommonData.CharacterStatsPanel.RefreshNow();
        }
        // OnBeginShow

        [HarmonyPatch(typeof(CharacterStageFightingStyleSelectionPanel), "OnBeginShow")]
        internal static class CharacterStageFightingStyleSelectionPanel_OnBeginShow_Patch
        {
            internal static void Postfix(int ___selectedFightingStyle) 
            {
                previouslySelectedFightingStyle = ___selectedFightingStyle;
            }
        }

        [HarmonyPatch(typeof(CharacterStageFightingStyleSelectionPanel), "Refresh")]
        internal static class CharacterStageFightingStyleSelectionPanel_Refresh_Patch
        {
            internal static bool Prefix(CharacterStageFightingStyleSelectionPanel __instance,
                                        List<FightingStyleDefinition> ___compatibleFightingStyles,
                                        RectTransform ___fightingStylesTable,
                                        RectTransform ___selectedFightingStyleGroup,
                                        int ___selectedFightingStyle,
                                        GuiLabel ___selectedFightingStyleTitle,
                                        GuiLabel ___fightingStyleDescription)
            {
                for (int index = 0; index < ___compatibleFightingStyles.Count; ++index)
                    ___fightingStylesTable.GetChild(index).GetComponent<DiskSelectionSlot>().RefreshState(___selectedFightingStyle == index, true);
                if (___selectedFightingStyle >= 0)
                {
                    ___selectedFightingStyleGroup.gameObject.SetActive(true);
                    ___selectedFightingStyleTitle.Text = ___compatibleFightingStyles[___selectedFightingStyle].GuiPresentation.Title;
                    FightingStyleDefinition compatibleFightingStyle = ___compatibleFightingStyles[___selectedFightingStyle];
                    //__instance.CharacterBuildingService.UntrainLastFightingStyle();
                    if (previouslySelectedFightingStyle >= 0)
                    {
                        UntrainPreviouslySelectedFightStyle(__instance, ___compatibleFightingStyles[previouslySelectedFightingStyle]);
                    }
                    __instance.CharacterBuildingService.UntrainLastFightingStyle();
                    __instance.CharacterBuildingService.TrainFightingStyle(compatibleFightingStyle);
                    ___fightingStyleDescription.Text = compatibleFightingStyle.FormatDescription();
                    RefreshNow(__instance);
                }
                else
                {
                    ___selectedFightingStyleGroup.gameObject.SetActive(false);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterStageFightingStyleSelectionPanel), "OnFightingStyleValueChangedCb")]
        internal static class CharacterStageFightingStyleSelectionPanel_OnFightingStyleValueChangedCb_Patch
        {
            internal static bool Prefix(CharacterStageFightingStyleSelectionPanel __instance,
                                        RectTransform ___fightingStylesTable,
                                        ref int ___selectedFightingStyle,
                                        BaseSelectionSlot selectedSlot)
            {
                for (int index = 0; index < ___fightingStylesTable.childCount; ++index)
                {
                    if (___fightingStylesTable.GetChild(index).GetComponent<BaseSelectionSlot>() == (Object)selectedSlot)
                    {
                        previouslySelectedFightingStyle = ___selectedFightingStyle;
                        ___selectedFightingStyle = index;
                        RefreshNow(__instance);
                    }
                }
                return false;
            }
        }
    }
}