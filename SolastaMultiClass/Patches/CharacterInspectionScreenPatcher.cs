using HarmonyLib;
using static SolastaMultiClass.Settings;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterInspectionScreenPatcher
    {
        [HarmonyPatch(typeof(CharacterInspectionScreen), "Bind")]
        internal static class CharacterInspectionScreen_Show_Patch
        {
            internal static void Prefix(RulesetCharacterHero heroCharacter)
            {
                InspectionPanelBindHero(heroCharacter);
            }
        }

        [HarmonyPatch(typeof(CharacterInspectionScreen), "DoClose")]
        internal static class CharacterInspectionScreen_OnCloseCb_Patch
        {
            internal static void Postfix()
            {
                InspectionPanelUnbindHero();
            }
        }

        [HarmonyPatch(typeof(CharacterInspectionScreen), "HandleInput")]
        internal static class CharacterInspectionScreen_HandleInput_Patch
        {
            internal static void Postfix(InputCommands.Id command, CharacterInformationPanel ___characterInformationPanel)
            {
                switch (command)
                {
                    case PLAIN_LEFT:
                        InspectionPanelPickPreviousHeroClass();
                        ___characterInformationPanel.RefreshNow();
                        break;

                    case PLAIN_RIGHT:
                        InspectionPanelPickNextHeroClass();
                        ___characterInformationPanel.RefreshNow();
                        break;
                }
            }
        }
    }
}