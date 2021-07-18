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
                UI.Label(". combines up to 3 different classes");
                UI.Label(". enforces ability scores minimum in/out pre-requisites");
                UI.Label(". only gains a subset of new classes starting proficiencies");
                UI.Label(". " + "extra attacks".cyan() + " / " + "unarmored defenses".cyan() + " won't stack whenever combining Barbarian, Fighter, Monk, Paladin or Ranger");
                UI.Label(". supports official game classes, subclasses and any unofficial ones by choosing their caster type in the settings panel");

                UI.Label("");
                UI.Label("Character Inspection Screen instructions:".yellow());
                UI.Label(". press the " + "UP".yellow().bold() + " and " + "DOWN".yellow().bold() + " arrows to switch between different label styles");
                UI.Label(". press the " + "LEFT".yellow().bold() + " and " + "RIGHT".yellow().bold() + " arrows to present other hero classes details");

                UI.Label(""); 
                UI.Label("Current limitations:".yellow());
                UI.Label(". independent pact magic and shared slot system");
                UI.Label(". channel divinity stacks");
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            UI.Label("Welcome to Multi Class".yellow().bold());

            DisplayHelp();
        }
    }
}