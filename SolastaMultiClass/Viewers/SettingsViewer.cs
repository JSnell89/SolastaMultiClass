using UnityModManagerNet;
using ModKit;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Viewers
{
    public class SettingsViewer : IMenuSelectablePage
    {
        public string Name => "Settings";

        public int Priority => 2;

        private static void DisplaySettings()
        {
            int selected;
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

            UI.Label("Default Deity (set when using a divine caster from level 2)".cyan());

            var deityTitles = GetDeityList().ToArray();
            selected = Main.Settings.SelectedDeity;
            if (UI.SelectionGrid(ref selected, deityTitles, deityTitles.Length, UI.AutoWidth()))
            {
                Main.Settings.SelectedDeity = selected;
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            DisplaySettings();
        }
    }
}