using UnityModManagerNet;
using ModKit;

namespace SolastaMultiClass.Viewers
{
    public class HelpViewer : IMenuSelectablePage
    {
        public string Name => "Help";

        public int Priority => 0;

        private static void DisplayHelp()
        {
            using (UI.HorizontalScope())
            {
                using (UI.VerticalScope())
                {
                    UI.Label("Multi Class (BETA VERSION)".yellow().bold());
                    UI.Div();
                    UI.Label("Current limitations:".yellow());
                    UI.Label(". spell system for multi-class is partially done");
                    UI.Label(". current progress of shared spellcasting can be turned off but it's not recommended");
                    UI.Label(". inspecting the character will not show things properly out of a game");
                    UI.Label(". pool leveled up characters need a long rest to have the proper spell slots refreshed");
                    UI.Label(". need to rework channel divinity, unarmored defense and other multi-class rules");
                    UI.Label(". need to automatically add a spellbook when multiclassing into a Wizard");
                    UI.Label("");
                    UI.Label("Inspection Screen Instructions:".yellow());
                    UI.Label(". press LEFT / RIGHT arrows in the character inspection tab to browser for other classes features...");
                }
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            DisplayHelp();
        }
    }
}