using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                Viewers.SettingsViewer.UpdateClassCasterTypesAndSettings();

                Models.GameUi.RegisterCommands();

                _ = Features.ArmorProficiencyMulticlassBuilder.BarbarianArmorProficiencyMulticlass;
                _ = Features.ArmorProficiencyMulticlassBuilder.FighterArmorProficiencyMulticlass;
                _ = Features.ArmorProficiencyMulticlassBuilder.PaladinArmorProficiencyMulticlass;

                _ = Features.SkillProficiencyMulticlassBuilder.BardClassSkillProficiencyMulticlass;
                _ = Features.SkillProficiencyMulticlassBuilder.PointPoolRangerSkillPointsMulticlass;
            }
        }
    }
}