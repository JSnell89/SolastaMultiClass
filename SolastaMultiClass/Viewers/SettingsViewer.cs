using UnityModManagerNet;
using ModKit;
using SolastaMultiClass.Models;
using System.Collections.Generic;

namespace SolastaMultiClass.Viewers
{
    public class SettingsViewer : IMenuSelectablePage
    {
        private class ClassCasterType
        {
            public string ClassName;
            public string ClassTitle;
            public Dictionary<string, string> SubclassNames = new Dictionary<string, string>();

            public ClassCasterType(string className, string classTitle, Dictionary<string, string> subclassNames)
            {
                this.ClassName = className;
                this.ClassTitle = classTitle;
                this.SubclassNames = subclassNames;
            }
        }

        public string Name => "Settings";

        public int Priority => 1;

        private static bool hasUpdatedClassCasterTypes = false;

        private static readonly List<ClassCasterType> classCasterTypes = new List<ClassCasterType>();

        private static Dictionary<string, string> GetSubclassNames(CharacterClassDefinition characterClassDefinition)
        {
            var characterSubclassDefinitionDB = DatabaseRepository.GetDatabase<CharacterSubclassDefinition>();
            var subclassNames = new Dictionary<string, string>();
            var subclassChoices = characterClassDefinition.FeatureUnlocks.FindAll(x => x.FeatureDefinition is FeatureDefinitionSubclassChoice);

            foreach (var subclassChoice in subclassChoices)
            {
                foreach (var subClassName in (subclassChoice.FeatureDefinition as FeatureDefinitionSubclassChoice).Subclasses)
                {
                    var characterSubclassDefinition = characterSubclassDefinitionDB.GetElement(subClassName);
                    if (!Main.Settings.SubclassCasterType.ContainsKey(subClassName))
                    {
                        Main.Settings.SubclassCasterType.Add(subClassName, CasterType.None);
                    }
                    subclassNames.Add(subClassName, characterSubclassDefinition.FormatTitle());
                }
            }
            return subclassNames;
        }

        private static void DisplayClassCasterTypeSettings()
        {
            foreach (var classCasterType in classCasterTypes)
            {
                using (UI.HorizontalScope())
                {
                    UI.Label(classCasterType.ClassTitle, UI.Width(264));
                    int choice = (int)Main.Settings.ClassCasterType[classCasterType.ClassName];
                    if (UI.SelectionGrid(ref choice, SharedSpellsRules.CasterTypeNames, SharedSpellsRules.CasterTypeNames.Length, UI.AutoWidth()))
                    {
                        Main.Settings.ClassCasterType[classCasterType.ClassName] = (CasterType)choice;
                    }
                }
                
                // subclasses display
                if (Main.Settings.ClassCasterType[classCasterType.ClassName] == CasterType.None)
                {
                    foreach (var subclassName in classCasterType.SubclassNames)
                    {
                        using (UI.HorizontalScope())
                        {
                            UI.Space(24);
                            UI.Label(subclassName.Value, UI.Width(240));
                            int choice = (int)Main.Settings.SubclassCasterType[subclassName.Key];
                            if (UI.SelectionGrid(ref choice, SharedSpellsRules.CasterTypeNames, SharedSpellsRules.CasterTypeNames.Length, UI.AutoWidth()))
                            {
                                Main.Settings.SubclassCasterType[subclassName.Key] = (CasterType)choice;
                            }
                        }
                    }
                }
            }
        }

        private static void DisplayMainSettings()
        {
            bool toggle;

            UI.Div();
            UI.Label("House Rules:".yellow());

            var maxAllowedClasses = Main.Settings.MaxAllowedClasses;
            if (UI.Slider("Max allowed classes", ref maxAllowedClasses, 1, 3, 2, "", UI.AutoWidth()))
            {
                Main.Settings.MaxAllowedClasses = maxAllowedClasses;
            }

            toggle = Main.Settings.EnableMinInOutAttributes;
            if (UI.Toggle("Enable ability scores minimum in/out pre-requisites", ref toggle, 0, UI.AutoWidth())) 
            {
                Main.Settings.EnableMinInOutAttributes = toggle;
            }

            toggle = Main.Settings.EnableNonStackingExtraAttacks;
            if (UI.Toggle("Enable non-stacking extra attacks (only on newly acquired levels)", ref toggle, 0, UI.AutoWidth()))
            {
                Main.Settings.EnableNonStackingExtraAttacks = toggle;
            }

            toggle = Main.Settings.EnableSharedSpellCasting;
            if (UI.Toggle("Enable the shared spell casting system", ref toggle, 0, UI.AutoWidth()))
            {
                Main.Settings.EnableSharedSpellCasting = toggle;
            }

            if (Main.Settings.EnableSharedSpellCasting)
            {
                UI.Label("");
                DisplayClassCasterTypeSettings();
            }
        }

        public static void UpdateClassCasterTypesAndSettings()
        {
            if (!hasUpdatedClassCasterTypes)
            {
                var characterClassDefinitionDB = DatabaseRepository.GetDatabase<CharacterClassDefinition>();

                if (characterClassDefinitionDB != null)
                {
                    foreach (var characterClassDefinition in characterClassDefinitionDB.GetAllElements())
                    {
                        if (!Main.Settings.ClassCasterType.ContainsKey(characterClassDefinition.Name))
                        {
                            Main.Settings.ClassCasterType.Add(characterClassDefinition.Name, CasterType.None);
                        }
                        classCasterTypes.Add(new ClassCasterType(characterClassDefinition.name, characterClassDefinition.FormatTitle(), GetSubclassNames(characterClassDefinition)));
                    }
                    hasUpdatedClassCasterTypes = true;
                }
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            UI.Label("Welcome to Multi Class".yellow().bold());

            DisplayMainSettings();
        }
    }
}