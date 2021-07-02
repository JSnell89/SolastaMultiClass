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
                    UI.Label(". don't multiclass into a cleric or paladin without a deity...");
                    UI.Label(". so, for now, deity selection is forced on all new characters");
                    UI.Label(". need to rework the spell system for multi-class");
                    UI.Label(". need to rework channel divinity, unarmored defense and other multi-class rules");
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