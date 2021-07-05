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
                UI.Label(". can multiclass into up to 3 different classes");
                UI.Label(". only gains some of new class's starting proficiencies");
                UI.Label(". attributes prerequisites for class in/out");
                UI.Label(". " + "extra attacks".cyan() + " / " + "unarmored defenses".cyan() + " won't stack when granted by different classes");
                UI.Label(". shared spell casting system");
                UI.Label(". some of above rules can be customized in the Mod Settings panel");

                UI.Label(""); 
                UI.Label("Current Limitations:".yellow());
                UI.Label(". spell casting UI isn't patched for multi casters. Use " + "CJD's Solasta UI Update Mod".magenta() + " instead");
                UI.Label(". level up UI displays full level 1 proficiencies on a new class. Rules are correctly implemented thought");
                UI.Label(". level up UI displays extra attacks on level 5 progression. Rules are correctly implemented thought");
                UI.Label(". inspecting the character might not show up correctly in the pool. An in game long rest fixes that");
                UI.Label(". " + "Paladin".green() + " / " + "Cleric".green() + " " + "channel divinity".cyan() + " stacks");
                    
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