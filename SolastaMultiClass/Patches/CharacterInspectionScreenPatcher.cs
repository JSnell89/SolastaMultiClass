using HarmonyLib;
using static SolastaMultiClass.Settings;
using static SolastaMultiClass.Models.ClassPicker;

namespace SolastaMultiClass.Patches
{
    class CharacterInspectionScreenPatcher
    {
        [HarmonyPatch(typeof(CharacterInspectionScreen), "Bind")]
        internal static class CharacterInspectionScreen_Show_Patch
        {
            internal static void Prefix(RulesetCharacterHero heroCharacter)
            {
                CollectHeroClasses(heroCharacter);
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
                        PickPreviousClass();
                        ___characterInformationPanel.RefreshNow();
                        break;

                    case PLAIN_RIGHT:
                        PickNextClass();
                        ___characterInformationPanel.RefreshNow();
                        break;
                }
            }
        }
    }
}