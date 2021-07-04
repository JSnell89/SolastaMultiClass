using SolastaModApi;
using System.Collections.Generic;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionProficiencys;

namespace SolastaMultiClass.Features
{
    internal class ArmorProficiencyMulticlassBuilder : BaseDefinitionBuilder<FeatureDefinitionProficiency>
    {
        const string BarbarianClassArmorProficiencyMulticlassName = "BarbarianClassArmorProficiencyMulticlass";
        const string BarbarianClassArmorProficiencyMulticlassGuid = "57cb4c5e87c545ad81bc88e05e17562a";

        const string FighterArmorProficiencyMulticlassName = "FighterArmorProficiencyMulticlass";
        const string FighterArmorProficiencyMulticlassGuid = "5df5ec907a424fccbfec103344421b51";

        const string PaladinArmorProficiencyMulticlassName = "PaladinArmorProficiencyMulticlass";
        const string PaladinArmorProficiencyMulticlassGuid = "69b18e44aabd4acca702c05f9d6c7fcb";

        protected ArmorProficiencyMulticlassBuilder(string name, string guid, List<string> proficiencysToReplace) : base(ProficiencyFighterArmor, name, guid)
        {
            Definition.Proficiencies.Clear();
            Definition.Proficiencies.AddRange(proficiencysToReplace);
        }

        private static FeatureDefinitionProficiency CreateAndAddToDB(string name, string guid, List<string> proficiencysToReplace)
            => new ArmorProficiencyMulticlassBuilder(name, guid, proficiencysToReplace).AddToDB();

        public static readonly FeatureDefinitionProficiency BarbarianArmorProficiencyMulticlass =
            CreateAndAddToDB(BarbarianClassArmorProficiencyMulticlassName, BarbarianClassArmorProficiencyMulticlassGuid, new List<string> {
                "ShieldCategory"
            });

        public static readonly FeatureDefinitionProficiency FighterArmorProficiencyMulticlass =
            CreateAndAddToDB(FighterArmorProficiencyMulticlassName, FighterArmorProficiencyMulticlassGuid, new List<string> {
                "LightArmorCategory",
                "MediumArmorCategory",
                "ShieldCategory"
            });

        public static readonly FeatureDefinitionProficiency PaladinArmorProficiencyMulticlass =
            CreateAndAddToDB(PaladinArmorProficiencyMulticlassName, PaladinArmorProficiencyMulticlassGuid, new List<string> {
                "LightArmorCategory",
                "MediumArmorCategory",
                "ShieldCategory"
            });
    }
}