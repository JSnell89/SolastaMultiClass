using UnityModManagerNet;
using ModKit;

namespace SolastaMultiClass.Viewers
{
    public class SettingsViewer : IMenuSelectablePage
    {
        public string Name => "Settings";

        public int Priority => 2;

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
            if (UI.Toggle("Enable the shared spell casting system", ref toggle, 0, UI.AutoWidth()))
            {
                Main.Settings.EnableSharedSpellCasting = toggle;
            }

            //toggle = Main.Settings.TurnOffSpellPreparationRestrictions;
            //if (UI.Toggle("Turn off multiclass spell preparation restrictions", ref toggle, 0, UI.AutoWidth()))
            //{
            //    Main.Settings.TurnOffSpellPreparationRestrictions = toggle;
            //}

            UI.Label("");

            UI.Label("Character Inspection Screen Instructions:".yellow());
            UI.Label(". press the " + "LEFT".yellow().bold() + " and " + "RIGHT".yellow().bold() + " arrows in the character tab to display other classes");
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            UI.Label("Welcome to Multi Class (EA VERSION)".yellow().bold());

            DisplaySettings();
        }
    }
}