using System.Collections.Generic;
using UnityModManagerNet;
using ModKit;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Viewers
{
    public class MultiClassViewer : IMenuSelectablePage
    {
        public string Name => "Multi Class Settings";

        public int Priority => 1;

        private static readonly Dictionary<RulesetCharacterHero, bool> showHeroClasses = new Dictionary<RulesetCharacterHero, bool> { };

        private static void DisplayClassSelector(RulesetCharacterHero.Snapshot snapshot)
        {        
            var classTitles = GetClassTitles().ToArray();
            var selected = System.Array.IndexOf(classTitles, GetHeroSelectedClassTitle(snapshot));

            if (UI.SelectionGrid(ref selected, classTitles, classTitles.Length, UI.Width(400)))
            {
                SetHeroSelectedClassFromTitle(snapshot, classTitles[selected]);
            }
        }

        private static void DisplayHeroStats(RulesetCharacterHero.Snapshot snapshot)
        {
            using (UI.HorizontalScope())
            {
                UI.Label($"{snapshot.Name} {snapshot.SurName}".orange().bold(), UI.Width(240));
                DisplayClassSelector(snapshot);
            }
            UI.Div();
        }

        private static void DisplayHeroes()
        {
            using (UI.VerticalScope(UI.AutoWidth(), UI.AutoHeight()))
            {
                if (InGame())
                {
                    foreach (var snapshot in GetHeroesParty())
                    {
                        DisplayHeroStats(snapshot);
                    }
                }
                else
                {
                    foreach (var snapshot in GetHeroesPool())
                    {
                        DisplayHeroStats(snapshot);
                    }
                }
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            UI.Label("Multi Class (ALPHA VERSION):".yellow().bold());
            UI.Div();

            DisplayHeroes();
        }
    }
}