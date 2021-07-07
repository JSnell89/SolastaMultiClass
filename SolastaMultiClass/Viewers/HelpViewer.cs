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
            UI.Div();
            using (UI.VerticalScope())
            {
                UI.Div();
                UI.Label("Features:".yellow());
                UI.Label(". supports official game classes, Holic92's " + "Barbarian".green() + " / " + "Bard".green() + " / " + "Monk".green() +", CJD's " + "Tinkerer".green());
                UI.Label(". can combine up to 3 different classes");
                UI.Label(". only gains some of new classes starting proficiencies");
                UI.Label(". enforces attributes prerequisites for in/out classes");
                UI.Label(". " + "extra attacks".cyan() + " / " + "unarmored defenses".cyan() + " won't stack when granted by different classes");
                UI.Label(". shared spell casting system");
                UI.Label(". some of above rules can be customized in the Mod Settings panel");

                UI.Label(""); 
                UI.Label("Current Limitations:".yellow());
                UI.Label(". inspecting the character might not show up correctly in the pool. Take a long rest to fix that");
                UI.Label(". " + "Paladin".green() + " / " + "Cleric".green() + " " + "channel divinity".cyan() + " stack");
                    
                UI.Label("");
                UI.Label("Character Inspection Screen Instructions:".yellow());
                UI.Label(". press the " + "LEFT".yellow().bold() + " and " +  "RIGHT".yellow().bold() + " arrows in the character tab to display other classes");
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            UI.Label("Welcome to Multi Class (EA VERSION)".yellow().bold());

            DisplayHelp();
        }
    }
}