using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SolastaModApi;

namespace SolastaMultiClass.Patches
{
    class CharacterBuildingManagerPatcher
    {
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantFeatures")]
        internal static class CharacterBuildingManager_GrantFeatures_Patch
        {
            internal static void Prefix(CharacterBuildingManager __instance, List<FeatureDefinition> grantedFeatures, string tag, bool clearPrevious = true, string optionalTagToCheck = null)
            {
                //If we are adding a level higher than level 1, exclude some features that would normally be added to a level 1 character but should not be added to a multiclass character
                if (__instance.HeroCharacter.ClassesHistory.Count > 1)
                {
                    grantedFeatures.RemoveAll(feature => FeaturesToExcludeFromMulticlassLevels.Contains(feature));

                    //Also need to add logic to add extra skill points here
                }
            }

            private static readonly FeatureDefinition[] FeaturesToExcludeFromMulticlassLevels = new FeatureDefinition[]
            {
                DatabaseHelper.FeatureDefinitionPointPools.PointPoolClericSkillPoints,
                DatabaseHelper.FeatureDefinitionPointPools.PointPoolFighterSkillPoints,
                DatabaseHelper.FeatureDefinitionPointPools.PointPoolPaladinSkillPoints,
                DatabaseHelper.FeatureDefinitionPointPools.PointPoolRangerSkillPoints,
                DatabaseHelper.FeatureDefinitionPointPools.PointPoolRogueSkillPoints,
                DatabaseHelper.FeatureDefinitionPointPools.PointPoolWizardSkillPoints,
                DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyClericSavingThrow,
                DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyFighterSavingThrow,
                DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyPaladinSavingThrow,
                DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyRangerSavingThrow,
                DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyRogueSavingThrow,
                DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyWizardSavingThrow,
            };
        }
    }
}