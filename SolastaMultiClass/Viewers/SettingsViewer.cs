﻿using UnityModManagerNet;
using ModKit;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Viewers
{
    public class SettingsViewer : IMenuSelectablePage
    {
        public string Name => "Settings";

        public int Priority => 2;

        private static void DisplaySettings()
        {
            UI.Label("Welcome to Level 20 with Multi Class".yellow().bold());
            UI.Div();

            var maxAllowedClasses = Main.Settings.maxAllowedClasses;
            if (UI.Slider("Max Allowed Classes", ref maxAllowedClasses, 1, 4, 2, "", UI.AutoWidth()))
            {
                Main.Settings.maxAllowedClasses = maxAllowedClasses;
                GetHeroesPool(true);
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            DisplaySettings();
        }
    }
}