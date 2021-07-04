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
                    UI.Label(". can multiclass into up to 3 different classes");
                    UI.Label(". gain only some of new class's starting proficiencies");
                    UI.Label(". attributes prerequisites for class in/out");
                    UI.Label(". extra attacks / unarmored defenses won't stack if granted by more than 1 class");
                    UI.Label(". shared spell casting system");
                    UI.Label(". some of above rules can be customized in the Mod Settings panel");

                    UI.Label(""); 
                    UI.Label("Known limitations:".yellow());
                    UI.Label(". level up UI displays full level 1 proficiencies on a new class. They are correctly implemented though");
                    UI.Label(". level up UI displays extra attacks as granted. They aren't! Too much work to change that...");
                    UI.Label(". inspecting the character might not show correctly out of a game");
                    UI.Label(". leveled up characters in the pool need a long rest to properly refresh the spell slots");
                    UI.Label(". Paladin/Cleric Channel Divinity stacks");
                    
                    UI.Label("");
                    UI.Label("Character Inspection Screen Instructions:".yellow());
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