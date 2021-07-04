using SolastaModApi;
using SolastaModApi.Extensions;
using System.Collections.Generic;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionPointPools;

namespace SolastaMultiClass.Features
{
    internal class SkillProficiencyMulticlassBuilder : BaseDefinitionBuilder<FeatureDefinitionPointPool>
    {
        const string BardClassSkillProficiencyMulticlassName = "BarbarianClassArmorProficiencyMulticlass";
        const string BardClassSkillProficiencyGuid = "69430d6b82dc48bf9aaba0a887cba5b8";

        const string PointPoolRangerSkillPointsMulticlassName = "FighterArmorProficiencyMulticlass";
        const string PointPoolRangerSkillPointsMulticlassGuid = "680861ab72a04038a8e437fa5ff9dbe9";

        protected SkillProficiencyMulticlassBuilder(string name, string guid, List<string> restrictedChoices) : base(PointPoolRangerSkillPoints, name, guid)
        {
            Definition.SetPoolAmount(1);
            Definition.RestrictedChoices.Clear();
            Definition.RestrictedChoices.AddRange(restrictedChoices);
        }

        private static FeatureDefinitionPointPool CreateAndAddToDB(string name, string guid, List<string> proficiencysToReplace)
            => new SkillProficiencyMulticlassBuilder(name, guid, proficiencysToReplace).AddToDB();

        public static readonly FeatureDefinitionPointPool BardClassSkillProficiencyMulticlass =
            CreateAndAddToDB(BardClassSkillProficiencyMulticlassName, BardClassSkillProficiencyGuid, new List<string> {
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