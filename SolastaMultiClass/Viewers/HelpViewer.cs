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
                    UI.Label("Multi Class (EA VERSION)".yellow().bold());

                    UI.Div();
                    UI.Label("Features:".yellow());
                    UI.Label(". supports official game classes, Holic92's Barbarian/Bard/Monk, CJD's Tinkerer");
                    UI.Label(". can multiclass up to 3 different classes");
                    UI.Label(". attributes prerequisites for class in/out");
                    UI.Label(". gain only some of new class's starting proficiencies");
                    UI.Label(". extra attacks won't stack");
                    UI.Label(". unarmored defenses won't stack");
                    UI.Label(". shared spell casting system");

                    UI.Label(""); 
                    UI.Label("Known limitations:".yellow());
                    UI.Label(". inspecting the character might not show correctly out of a game");
                    UI.Label(". leveled up characters in the pool need a long rest to properly refresh the spell slots");
                    UI.Label(". Paladin/Cleric Channel Divinity stacks");
                    
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