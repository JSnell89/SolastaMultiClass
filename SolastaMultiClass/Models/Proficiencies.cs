namespace SolastaMultiClass.Models
{
    internal static class Proficiencies
    {
        public static readonly string[] savingThrownsProficiencysToExclude = new string[]
        {
            "BarbarianSavingthrowProficiency",
            "BardSavingthrowProficiency",
            "MonkSavingthrowProficiency",
            "ProficiencyClericSavingThrow",
            "ProficiencyFighterSavingThrow",
            "ProficiencyPaladinSavingThrow",
            "ProficiencyRangerSavingThrow",
            "ProficiencyRogueSavingThrow",
            "ProficiencyWizardSavingThrow",
            "ProficiencyTinkererSavingThrow"
        };

        public static readonly string[] skillProficiencysToExclude = new string[]
        {
            "BarbarianSkillProficiency",
            "BardSkillProficiency",
            "MonkSkillProficiency",
            "PointPoolClericSkillPoints",
            "PointPoolFighterSkillPoints",
            "PointPoolPaladinSkillPoints",
            "PointPoolRangerSkillPoints",
            "PointPoolRogueSkillPoints",
            "PointPoolWizardSkillPoints",
            "PointPoolTinkererSkillPoints"
        };

        public static readonly string[] skillProficiencysToInclude = new string[]
        {
            "BardClassSkillProficiencyMulticlass",
            "PointPoolRangerSkillPointsMulticlass"
        };

        public static readonly string[] armorProficiencysToExclude = new string[]
        {
            "BarbarianArmorProficiency",
            "ProficiencyFighterArmor", 
            "ProficiencyPaladinArmor",
            "ProficiencyWizardArmor"
        };

        public static readonly string[] armorProficiencysToInclude = new string[]
        {
            "BarbarianClassArmorProficiencyMulticlass",
            "FighterArmorProficiencyMulticlass",
            "PaladinArmorProficiencyMulticlass"
        };

        public static readonly string[] weaponProficiencysToExclude = new string[]
        {
            "BardWeaponProficiency",
            "ProficiencyClericWeapon",
            "ProficiencyRogueWeapon",
            "ProficiencyWizardWeapon",
            "ProficiencyWeaponTinkerer"
        };
    }
}