using SolastaModApi;
using SolastaModApi.Extensions;
using System.Collections.Generic;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionPointPools;

namespace SolastaMultiClass.Features
{
    internal class SkillProficiencyMulticlassBuilder : BaseDefinitionBuilder<FeatureDefinitionPointPool>
    {
        const string BardClassSkillProficiencyMulticlassName = "BardClassSkillProficiencyMulticlass";
        const string BardClassSkillProficiencyMulticlassGuid = "a69b2527569b4893abe57ad1f80e97ed";

        const string PointPoolRangerSkillPointsMulticlassName = "PointPoolRangerSkillPointsMulticlass";
        const string PointPoolRangerSkillPointsMulticlassGuid = "096e4e01b52b490e807cf8d458845aa5";

        protected SkillProficiencyMulticlassBuilder(string name, string guid, List<string> restrictedChoices) : base(PointPoolRangerSkillPoints, name, guid)
        {
            Definition.SetPoolAmount(1);
            Definition.RestrictedChoices.Clear();
            Definition.RestrictedChoices.AddRange(restrictedChoices);
        }

        private static FeatureDefinitionPointPool CreateAndAddToDB(string name, string guid, List<string> proficiencysToReplace)
            => new SkillProficiencyMulticlassBuilder(name, guid, proficiencysToReplace).AddToDB();

        public static readonly FeatureDefinitionPointPool BardClassSkillProficiencyMulticlass =
            CreateAndAddToDB(BardClassSkillProficiencyMulticlassName, BardClassSkillProficiencyMulticlassGuid, new List<string> {
                "AnimalHandling",
                "Athletics",
                "Insight",
                "Investigation",
                "Nature",
                "Perception",
                "Survival",
                "Stealth"
            });

        public static readonly FeatureDefinitionPointPool PointPoolRangerSkillPointsMulticlass =
            CreateAndAddToDB(PointPoolRangerSkillPointsMulticlassName, PointPoolRangerSkillPointsMulticlassGuid, new List<string> {
                "Acrobatics",
                "AnimalHandling",
                "Arcana",
                "Athletics",
                "Deception",
                "History",
                "Insight",
                "Intimidation",
                "Investigation",
                "Medecine",
                "Nature",
                "Perception",
                "Performance",
                "Persuasion",
                "Religion",
                "SleightOfHand",
                "Stealth",
                "Survival"
            });
    }
}