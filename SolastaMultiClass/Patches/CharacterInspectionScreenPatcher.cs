using HarmonyLib;
using static SolastaMultiClass.Settings;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterInspectionScreenPatcher
    {
        [HarmonyPatch(typeof(CharacterInspectionScreen), "Bind")]
        internal static class CharacterInspectionScreen_Bind_Patch
        {
            internal static void Prefix(RulesetCharacterHero heroCharacter)
            {
                Models.InspectionPanelContext.SelectedHero = heroCharacter;
            }
        }

        [HarmonyPatch(typeof(CharacterInspectionScreen), "DoClose")]
        internal static class CharacterInspectionScreen_DoClose_Patch
        {
            internal static void Postfix()
            {
                Models.InspectionPanelContext.SelectedHero = null;
            }
        }

        [HarmonyPatch(typeof(CharacterInspectionScreen), "HandleInput")]
        internal static class CharacterInspectionScreen_HandleInput_Patch
        {
            internal static bool displayClassesLabel = true;

            internal static void Postfix(InputCommands.Id command, CharacterInspectionScreen __instance, CharacterInformationPanel ___characterInformationPanel, CharacterPlateDetailed ___characterPlate)
            {
                switch (command)
                {
                    case PLAIN_UP:
                    case PLAIN_DOWN:
                        if (__instance?.InspectedCharacter?.RulesetCharacterHero?.ClassesAndLevels.Count > 1)
                        {
                            var classLabel = (GuiLabel)AccessTools.Field(___characterPlate.GetType(), "classLabel").GetValue(___characterPlate);

                            if (displayClassesLabel)
                            {
                                classLabel.Text = Models.GameUi.GetAllSubclassesLabel(__instance.InspectedCharacter);
                            }
                            else
                            {
                                classLabel.Text = Models.GameUi.GetAllClassesLabel(__instance.InspectedCharacter, classLabel.Text);
                            }
                            displayClassesLabel = !displayClassesLabel;
                        }
                        break;

                    case PLAIN_LEFT:
                        if (___characterInformationPanel.gameObject.activeSelf)
                        {
                            Models.InspectionPanelContext.InspectionPanelPickPreviousHeroClass();
                            ___characterInformationPanel.RefreshNow();
                        }
                        break;

                    case PLAIN_RIGHT:
                        if (___characterInformationPanel.gameObject.activeSelf)
                        {
                            Models.InspectionPanelContext.InspectionPanelPickNextHeroClass();
                            ___characterInformationPanel.RefreshNow();
                        }
                        break;
                }
            }
        }
    }
}