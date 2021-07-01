//using UnityModManagerNet;
//using ModKit;

//namespace SolastaMultiClass.Viewers
//{
//    public class SettingsViewer : IMenuSelectablePage
//    {
//        public string Name => "Settings";

//        public int Priority => 2;

//        private static void DisplaySettings()
//        {
//            UI.Label("Multi Class (BETA VERSION):".yellow().bold());
//            UI.Div();

//            var maxAllowedClasses = Main.Settings.maxAllowedClasses;
//            if (UI.Slider("Max Allowed Classes", ref maxAllowedClasses, 1, 3, 2, "", UI.AutoWidth()))
//            {
//                Main.Settings.maxAllowedClasses = maxAllowedClasses;
//            }
//        }

//        public void OnGUI(UnityModManager.ModEntry modEntry)
//        {
//            if (Main.Mod == null) return;

//            DisplaySettings();
//        }
//    }
//}