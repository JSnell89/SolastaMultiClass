namespace SolastaMultiClass.Models
{
    internal static class Proficiencies
    {
        public static readonly string[] savingThrownsProficiencysToExclude = new string[]
        {
            "BabarianSavingthrowProficiency",
            "BardSavingthrowProficiency",
            "MonkSavingthrowProficiency",
            "ProficiencyClericSavingThrow",
            "ProficiencyFighterSavingThrow",
            "ProficiencyPaladinSavingThrow",
            "ProficiencyRangerSavingThrow",
            "ProficiencyRogueSavingThrow",
            "ProficiencyWizardSavingThrow"
        };

        public static readonly string[] skillProficiencysToExclude = new string[]
        {
            "BarbarianSkillProficiency",
            "BardSkillProficiency", // need to dup and give only one skill
            "MonkSkillProficiency",
            "PointPoolClericSkillPoints",
            "PointPoolFighterSkillPoints",
            "PointPoolPaladinSkillPoints",
            "PointPoolRangerSkillPoints", // need to dup and give only one skill 
            "PointPoolRogueSkillPoints",
            "PointPoolWizardSkillPoints"
        };

        public static readonly string[] armorProficiencysToExclude = new string[]
        {
            "BarbarianArmorProficiency", // need to dup and remove light and medium
            //"BardArmorProficiency", // same
            //"ProficiencyClericArmor", // same
            "ProficiencyFighterArmor", // need to dup and remove heavy
            "ProficiencyPaladinArmor", // need to dup and remove heavy
            //"ProficiencyRangerArmor", // same
            //"ProficiencyRogueArmor", // same
            "ProficiencyWizardArmor" // remove all
        };

        public static readonly string[] armorProficiencysToInclude = new string[]
{
            "BarbarianClassArmorProficiencyMulticlass",
            "FighterArmorProficiencyMulticlass",
            "PaladinArmorProficiencyMulticlass"
};

        public static readonly string[] weaponProficiencysToExclude = new string[]
        {
            //"BarbarianWeaponProficiency", // same
            "BardWeaponProficiency", // remove all
            //"MonkWeaponProficiency", // same
            "ProficiencyClericWeapon", // remove all
            //"ProficiencyFighterWeapon", // same
            //"ProficiencyPaladinWeapon", // same
            //"ProficiencyRangerWeapon", // same
            "ProficiencyRogueWeapon", // remove all
            "ProficiencyWizardWeapon" // remove all
        };
    }
}
