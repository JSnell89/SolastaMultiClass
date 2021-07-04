using HarmonyLib;
using SolastaMultiClass.Features;

namespace SolastaMultiClass.Patches
{
    internal static class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                SolastaMultiClass.Models.GameUi.RegisterCommands();

                _ = ArmorProficiencyMulticlassBuilder.BarbarianArmorProficiencyMulticlass;
                _ = ArmorProficiencyMulticlassBuilder.FighterArmorProficiencyMulticlass;
                _ = ArmorProficiencyMulticlassBuilder.PaladinArmorProficiencyMulticlass;

                _ = SkillProficiencyMulticlassBuilder.BardClassSkillProficiencyMulticlass;
                _ = SkillProficiencyMulticlassBuilder.PointPoolRangerSkillPointsMulticlass;
            }
        }
    }
}