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
            UI.Label("House Rules".yellow());
            UI.Div();

            var maxAllowedClasses = Main.Settings.maxAllowedClasses;
            if (UI.Slider("Max Allowed Classes", ref maxAllowedClasses, 1, 3, 2, "", UI.AutoWidth()))
            {
                Main.Settings.maxAllowedClasses = maxAllowedClasses;
            }

            var toggle = Main.Settings.ForceMinInOutPreReqs;
            if (UI.Toggle("Enable ability scores minimum in/out pre-requisites", ref toggle, 0, UI.AutoWidth())) 
            {
                Main.Settings.ForceMinInOutPreReqs = toggle;
            }

            var sharedSpellToggle = Main.Settings.EnableSharedSpellcasting;
            if (UI.Toggle("Enable shared spellcasting", ref sharedSpellToggle, 0, UI.AutoWidth()))
            {
                Main.Settings.EnableSharedSpellcasting = toggle;
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            DisplaySettings();
        }
    }
}