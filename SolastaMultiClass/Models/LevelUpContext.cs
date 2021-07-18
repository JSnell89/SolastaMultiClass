using System.Collections.Generic;
using HarmonyLib;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;

namespace SolastaMultiClass.Models
{
    internal static class LevelUpContext
    {
        private static readonly Dictionary<string, List<string>> extraAttacksToExclude = new Dictionary<string, List<string>>
        {
            {"BarbarianClass", new List<string> { "BarbarianClassExtraAttack" } },
            {"MonkClass", new List<string> { "MonkClassExtraAttack" } },
            {"Fighter", new List<string> { "AttributeModifierFighterExtraAttack" } },
            {"Paladin", new List<string> { "AttributeModifierPaladinExtraAttack" } },
            {"Ranger", new List<string> { "AttributeModifierRangerExtraAttack" } }
        };

        private static readonly Dictionary<string, Dictionary<string, string>> featuresToExclude = new Dictionary<string, Dictionary<string, string>>
        {
            {"Cleric", new Dictionary<string, string> {
                {"ProficiencyClericWeapon", null},
                {"PointPoolClericSkillPoints", null},
                {"ProficiencyClericSavingThrow", null} } },

            {"Fighter", new Dictionary<string, string> {
                {"ProficiencyFighterArmor", "FighterArmorProficiencyMulticlass"},
                {"PointPoolFighterSkillPoints", null},
                {"ProficiencyFighterSavingThrow", null} } },

            {"Paladin", new Dictionary<string, string> {
                {"ProficiencyPaladinArmor", "PaladinArmorProficiencyMulticlass"},
                {"PointPoolPaladinSkillPoints", null},
                {"ProficiencyPaladinSavingThrow", null} } },

            {"Ranger", new Dictionary<string, string> {
                {"PointPoolRangerSkillPoints", "PointPoolRangerSkillPointsMulticlass"},
                {"ProficiencyRangerSavingThrow", null} } },

            {"Rogue", new Dictionary<string, string> {
                {"ProficiencyRogueWeapon", null},
                {"PointPoolRogueSkillPoints", "PointPoolRogueSkillPointsMulticlass"},
                {"ProficiencyRogueSavingThrow", null} } },

            {"Sorcerer", new Dictionary<string, string> {
                {"ProficiencySorcererWeapon", null},
                {"ProficiencySorcererArmor", null},
                {"PointPoolSorcererSkillPoints", null},
                {"ProficiencySorcererSavingThrow", null}} },

            {"Wizard", new Dictionary<string, string> {
                {"ProficiencyWizardWeapon", null},
                {"ProficiencyWizardArmor", null},
                {"PointPoolWizardSkillPoints", null},
                {"ProficiencyWizardSavingThrow", null}} },

            {"ClassTinkerer", new Dictionary<string, string> {
                {"ProficiencyWeaponTinkerer", null},
                {"PointPoolTinkererSkillPoints", null},
                {"ProficiencyTinkererSavingThrow", null}} },

            {"BarbarianClass", new Dictionary<string, string> {
                {"BarbarianArmorProficiency", "BarbarianClassArmorProficiencyMulticlass" },
                {"BarbarianSkillProficiency", null},
                {"BarbarianSavingthrowProficiency", null} } },

            {"BardClass", new Dictionary<string, string> {
                {"BardWeaponProficiency", null},
                {"BardSkillProficiency", "BardClassSkillProficiencyMulticlass"},
                {"BardSavingthrowProficiency", null} } },

            {"MonkClass", new Dictionary<string, string> {
                {"MonkSkillProficiency", null},
                {"MonkSavingthrowProficiency", null} } },

            {"WarlockClass", new Dictionary<string, string> {
                {"WarlockWeaponProficiency", null},
                {"WarlockSkillProficiency", null},
                {"WarlockSavingthrowProficiency", null} } },
        };

        private static bool levelingUp = false;
        private static bool displayingClassPanel = false;
        private static bool requiresDeity = false;
        private static bool requiresSpellbook = false;
        private static bool hasSpellbookGranted = false;
        private static RulesetCharacterHero selectedHero = null;
        private static CharacterClassDefinition selectedClass = null;
        private static CharacterSubclassDefinition selectedSubclass = null;
        private static readonly List<FeatureUnlockByLevel> selectedClassFeaturesUnlock = new List<FeatureUnlockByLevel>();

        public static RulesetCharacterHero SelectedHero 
        {
            get => selectedHero;
            set 
            { 
                selectedHero = value;
                selectedClass = null;
                selectedSubclass = null;
                levelingUp = value != null;
                hasSpellbookGranted = false;
                requiresSpellbook = false;
                requiresDeity = false;
            }
        }

        public static CharacterClassDefinition SelectedClass
        {
            get => selectedClass;
            set 
            {
                selectedClass = value;
                selectedSubclass = null;
                if (selectedClass != null)
                {
                    var classesAndLevels = selectedHero.ClassesAndLevels;

                    selectedHero.ClassesAndSubclasses.TryGetValue(selectedClass, out selectedSubclass);
                    selectedClassFeaturesUnlock.Clear();
                    foreach (var featureUnlock in selectedClass.FeatureUnlocks)
                    {
                        selectedClassFeaturesUnlock.Add(new FeatureUnlockByLevel(featureUnlock.FeatureDefinition, featureUnlock.Level));
                    }
                    FixMulticlassProficiencies();
                    FixExtraAttacks();

                    hasSpellbookGranted = false;
                    requiresSpellbook = !selectedHero.ClassesAndLevels.ContainsKey(Wizard) && selectedClass == Wizard;
                    requiresDeity = (selectedClass == Cleric && !classesAndLevels.ContainsKey(Cleric)) || (selectedClass == Paladin && selectedHero.DeityDefinition == null);
                }
            }
        }

        public static int SelectedHeroLevel
        {
            get
            {
                var heroLevel = 0;

                if (SelectedHero != null)
                {
                    heroLevel = SelectedHero.ClassesHistory.Count;
                }
                return heroLevel;
            }
        }

        public static int SelectedClassLevel
        {
            get
            {
                var classLevel = 0;

                if (SelectedHero != null && SelectedClass != null)
                {
                    classLevel = SelectedHero.ClassesAndLevels[SelectedClass];
                }
                return classLevel;
            }
        }

        public static CharacterSubclassDefinition SelectedSubclass => selectedSubclass;

        public static List<FeatureUnlockByLevel> SelectedClassFeaturesUnlock => selectedClassFeaturesUnlock;

        public static bool DisplayingClassPanel
        {
            get => displayingClassPanel;
            set
            {
                displayingClassPanel = value;
            }
        }

        public static bool LevelingUp => levelingUp;

        public static bool RequiresDeity => requiresDeity;

        public static bool HasCantrips()
        {
            FeatureDefinitionCastSpell featureDefinitionCastSpell;
            bool hasCantrips = false;
            int level = SelectedHero.ClassesHistory.Count + 1;

            if (SelectedClass != null)
            {
                featureDefinitionCastSpell = (FeatureDefinitionCastSpell)SelectedClass.FeatureUnlocks.Find(x => x.FeatureDefinition is FeatureDefinitionCastSpell)?.FeatureDefinition;
                hasCantrips = featureDefinitionCastSpell?.KnownCantrips[level] > 0;
            }
            if (!hasCantrips && SelectedSubclass != null)
            {
                featureDefinitionCastSpell = (FeatureDefinitionCastSpell)SelectedSubclass.FeatureUnlocks.Find(x => x.FeatureDefinition is FeatureDefinitionCastSpell)?.FeatureDefinition;
                hasCantrips = featureDefinitionCastSpell?.KnownCantrips[level] > 0;
            }
            return hasCantrips;
        }

        public static void GrantSpellbookIfRequired()
        {
            if (requiresSpellbook && !hasSpellbookGranted)
            {
                var item = new RulesetItemSpellbook(SolastaModApi.DatabaseHelper.ItemDefinitions.Spellbook);

                selectedHero.GrantItem(item, false);
                hasSpellbookGranted = true;
            }
        }

        public static void UngrantSpellbookIfRequired()
        {
            if (hasSpellbookGranted)
            {
                var item = new RulesetItemSpellbook(SolastaModApi.DatabaseHelper.ItemDefinitions.Spellbook);

                selectedHero.LoseItem(item);
                hasSpellbookGranted = false;
            }
        }

        private static bool HasExtraAttack()
        {
            var hasExtraAttack = false;

            foreach (var classAndLevel in selectedHero.ClassesAndLevels)
            {
                var className = classAndLevel.Key.Name;

                if (extraAttacksToExclude.ContainsKey(className) && className != selectedClass.Name && classAndLevel.Value >= 5) // && !WasGrantedBy(selectedClass)
                {
                    hasExtraAttack = true;
                }
            }
            return hasExtraAttack;
        }

        public static void FixExtraAttacks()
        {
            if (!Main.Settings.EnableNonStackingExtraAttacks) return;

            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();

            if (extraAttacksToExclude.ContainsKey(selectedClass.name))
            {
                foreach (var featureName in extraAttacksToExclude[selectedClass.Name])
                {
                    if (HasExtraAttack())
                    {
                        selectedClassFeaturesUnlock.RemoveAll(x => x.FeatureDefinition.Name == featureName && x.Level == 5);
                    }
                }
            }
        }

        public static void FixMulticlassProficiencies()
        {
            var featuresDb = DatabaseRepository.GetDatabase<FeatureDefinition>();

            if (featuresToExclude.ContainsKey(selectedClass.Name))
            {
                foreach (var oldNewFeatureName in featuresToExclude[selectedClass.name])
                {
                    if (oldNewFeatureName.Value != null && featuresDb.TryGetElement(oldNewFeatureName.Value, out FeatureDefinition feature))
                    {
                        var featureUnlockByLevel = selectedClassFeaturesUnlock.Find(x => x.FeatureDefinition.Name == oldNewFeatureName.Key && x.Level == 1);
                        if (featureUnlockByLevel != null)
                        {
                            AccessTools.Field(featureUnlockByLevel.GetType(), "featureDefinition").SetValue(featureUnlockByLevel, feature);
                        }
                    }
                    else
                    {
                        selectedClassFeaturesUnlock.RemoveAll(x => x.FeatureDefinition.Name == oldNewFeatureName.Key && x.Level == 1);
                    }
                }
            }
        }

        // used on a transpiler
        public static int GetClassLevel(RulesetCharacterHero hero)
        {
            if (selectedClass == null || !selectedHero.ClassesAndLevels.ContainsKey(selectedClass))
            {
                return 1;
            }
            return selectedHero.ClassesAndLevels[selectedClass];
        }

        // used on a transpiler
        public static void GetLastAssignedClassAndLevel(ICharacterBuildingService characterBuildingService, out CharacterClassDefinition lastClassDefinition, out int level)
        {
            displayingClassPanel = false;
            GrantSpellbookIfRequired();
            lastClassDefinition = SelectedClass;
            level = SelectedHero.ClassesHistory.Count;
        }
    }
}