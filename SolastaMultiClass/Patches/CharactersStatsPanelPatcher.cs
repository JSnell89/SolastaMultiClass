using UnityEngine;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    class CharactersStatsPanelPatcher
    {
        [HarmonyPatch(typeof(CharacterStatsPanel), "Refresh")]
        internal static class CharacterStatsPanel_Refresh_Patch
        {
            internal static void Postfix(CharacterStatBox ___hitDiceBox, GuiCharacter ___guiCharacter)
            {
                if (___hitDiceBox.Activated && ___guiCharacter.RulesetCharacterHero?.ClassesHistory.Count > 1)
                {
                    switch (___guiCharacter.RulesetCharacterHero.ClassesHistory.Count)
                    {
                        case 1:
                            ___hitDiceBox.ValueLabel.RectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            break;

                        default:
                            ___hitDiceBox.ValueLabel.RectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                            break;
                    }
                    ___hitDiceBox.ValueLabel.Text = Models.GameUi.GetAllClassesHitDiceLabel(___guiCharacter);
                }
            }
        }
    }
}