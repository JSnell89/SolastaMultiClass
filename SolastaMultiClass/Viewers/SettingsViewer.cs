using UnityModManagerNet;
using ModKit;
using SolastaMultiClass.Models;
using System.Collections.Generic;

namespace SolastaMultiClass.Viewers
{
    public class SettingsViewer : IMenuSelectablePage
    {
        public string Name => "Settings";

        public int Priority => 1;

        private static bool hasUpdatedSettingsClassCasterType = false;
        private static bool hasUpdatedSettingsSubclassCasterType = false;

        private static readonly Dictionary<string, string> classNames = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> subclassNames = new Dictionary<string, string>();

        internal static void UpdateSettingsClassCasterType()
        {
            if (!hasUpdatedSettingsClassCasterType)
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
                        classNames.Add(characterClassDefinition.name, characterClassDefinition.FormatTitle());
                    }
                    hasUpdatedSettingsClassCasterType = true;
                }

            }
        }

        internal static void UpdateSettingsSubclassCasterType()
        {
            if (!hasUpdatedSettingsSubclassCasterType)
            {
                var characterSubclassDefinitionDB = DatabaseRepository.GetDatabase<CharacterSubclassDefinition>();

                if (characterSubclassDefinitionDB != null)
                {
                    foreach (var characterSubclassDefinition in characterSubclassDefinitionDB?.GetAllElements())
                    {
                        if (!Main.Settings.SubclassCasterType.ContainsKey(characterSubclassDefinition.Name))
                        {
                            Main.Settings.SubclassCasterType.Add(characterSubclassDefinition.Name, CasterType.None);
                        }
                        subclassNames.Add(characterSubclassDefinition.name, characterSubclassDefinition.FormatTitle());
                    }
                    hasUpdatedSettingsSubclassCasterType = true;
                }
            }
        }

        private static void DisplayClassCasterTypeSettings()
        {
            UI.Label("");
            UI.Label("Caster Type Class Settings:".yellow());

            foreach (var className in classNames)
            {
                using (UI.HorizontalScope())
                {
                    UI.Label(className.Value, UI.Width(240));
                    int choice = (int)Main.Settings.ClassCasterType[className.Key];
                    if (UI.SelectionGrid(ref choice, SharedSpells.CasterTypeNames, SharedSpells.CasterTypeNames.Length, UI.AutoWidth()))
                    {
                        Main.Settings.ClassCasterType[className.Key] = (CasterType)choice;
                    }
                }
            }
        }

        private static void DisplaySubclassCasterTypeSettings()
        {
            UI.Label("");
            UI.Label("Caster Type Subclass Settings (fine tuning. only if class is set to None):".yellow());

            foreach (var subclassName in subclassNames)
            {
                using (UI.HorizontalScope())
                {
                    UI.Label(subclassName.Value, UI.Width(240));
                    int choice = (int)Main.Settings.SubclassCasterType[subclassName.Key];
                    if (UI.SelectionGrid(ref choice, SharedSpells.CasterTypeNames, SharedSpells.CasterTypeNames.Length, UI.AutoWidth()))
                    {
                        Main.Settings.SubclassCasterType[subclassName.Key] = (CasterType)choice;
                    }
                }
            }
        }

        private static void DisplaySettings()
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
            if (UI.Toggle("Enable the shared spell casting system (fine tune progression below)", ref toggle, 0, UI.AutoWidth()))
            {
                Main.Settings.EnableSharedSpellCasting = toggle;
            }

            DisplayClassCasterTypeSettings();
            DisplaySubclassCasterTypeSettings();
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            UI.Label("Welcome to Multi Class (EA VERSION)".yellow().bold());

            DisplaySettings();
        }
    }
}