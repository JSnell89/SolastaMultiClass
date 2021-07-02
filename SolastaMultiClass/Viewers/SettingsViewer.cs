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

            var maxAllowedClasses = Main.Settings.maxAllowedClasses;
            if (UI.Slider("Max Allowed Classes", ref maxAllowedClasses, 1, 3, 2, "", UI.AutoWidth()))
            {
                Main.Settings.maxAllowedClasses = maxAllowedClasses;
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
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            DisplaySettings();
        }
    }
}