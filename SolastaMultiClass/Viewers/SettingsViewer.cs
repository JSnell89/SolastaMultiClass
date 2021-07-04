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

            UI.Label("House Rules".yellow());
            UI.Div();

            var maxAllowedClasses = Main.Settings.MaxAllowedClasses;
            if (UI.Slider("Max Allowed Classes", ref maxAllowedClasses, 1, 3, 2, "", UI.AutoWidth()))
            {
                Main.Settings.MaxAllowedClasses = maxAllowedClasses;
            }

            toggle = Main.Settings.ForceMinInOutPreReqs;
            if (UI.Toggle("Enable ability scores minimum in/out pre-requisites", ref toggle, 0, UI.AutoWidth())) 
            {
                Main.Settings.ForceMinInOutPreReqs = toggle;
            }

            toggle = Main.Settings.EnableSharedSpellCasting;
            if (UI.Toggle("Enable shared spellcasting", ref toggle, 0, UI.AutoWidth()))
            {
                Main.Settings.EnableSharedSpellCasting = toggle;
            }

            toggle = Main.Settings.TurnOffSpellPreparationRestrictions;
            if (UI.Toggle("Turn off multiclass spell preparing restrictions", ref toggle, 0, UI.AutoWidth()))
            {
                Main.Settings.TurnOffSpellPreparationRestrictions = toggle;
            }

            toggle = Main.Settings.AllowExtraAttacksToStack;
            if (UI.Toggle("Allow extra attacks to stack (only on new acquired levels)", ref toggle, 0, UI.AutoWidth()))
            {
                Main.Settings.AllowExtraAttacksToStack = toggle;
            }

            UI.Div();
            UI.Label("Character Inspection Screen Instructions:".yellow().bold());
            UI.Label(". press LEFT / RIGHT arrows in the character inspection tab to browser for other classes features...");
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            DisplaySettings();
        }
    }
}