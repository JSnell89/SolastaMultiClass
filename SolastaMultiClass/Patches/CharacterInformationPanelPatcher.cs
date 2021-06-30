using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static SolastaMultiClass.Models.ClassPicker;

namespace SolastaMultiClass.Patches
{
    class CharacterInformationPanelPatcher
    {
        [HarmonyPatch(typeof(CharacterInformationPanel), "EnumerateClassBadges")]
        internal static class CharacterInformationPanel_EnumerateClassBadges_Patch
        {
            private static bool Prefix(CharacterInformationPanel __instance,
                                       RectTransform ___classBadgesTable,
                                       GameObject ___classBadgePrefab,
                                       List<BaseDefinition> ___badgeDefinitions)
            {
                var selectedClass = GetSelectedClass();
                var rulesetCharacterHero = __instance.InspectedCharacter.RulesetCharacterHero;

                ___badgeDefinitions.Clear();

                foreach (KeyValuePair<CharacterClassDefinition, CharacterSubclassDefinition> classesAndSubclass in rulesetCharacterHero.ClassesAndSubclasses)
                {
                    if (classesAndSubclass.Key == selectedClass)
                    {
                        ___badgeDefinitions.Add(classesAndSubclass.Value);
                    }
                }
                
                if (rulesetCharacterHero.DeityDefinition != null && selectedClass.RequiresDeity)
                {
                    ___badgeDefinitions.Add(rulesetCharacterHero.DeityDefinition);
                }

                // TODO: Is there any simple way to determine which class gave the Fighting Style?

                foreach (BaseDefinition trainedFightingStyle in rulesetCharacterHero.TrainedFightingStyles)
                {
                    ___badgeDefinitions.Add(trainedFightingStyle);
                }

                while (___classBadgesTable.childCount < ___badgeDefinitions.Count)
                {
                    Gui.GetPrefabFromPool(___classBadgePrefab, ___classBadgesTable);
                }

                int index1 = 0;
                foreach (BaseDefinition badgeDefinition in ___badgeDefinitions)
                {
                    Transform child = ___classBadgesTable.GetChild(index1);
                    child.gameObject.SetActive(true);
                    child.GetComponent<CharacterInformationBadge>().Bind(badgeDefinition, ___classBadgesTable);
                    ++index1;
                }
                for (int index2 = index1; index2 < ___classBadgesTable.childCount; ++index2)
                {
                    Transform child = ___classBadgesTable.GetChild(index2);
                    child.GetComponent<CharacterInformationBadge>().Unbind();
                    child.gameObject.SetActive(false);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterInformationPanel), "Refresh")]
        internal static class CharacterInformationPanel_Refresh_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var containsMethod = typeof(string).GetMethod("Contains");
                var getSelectedClassSearchTermMethod = typeof(SolastaMultiClass.Models.ClassPicker).GetMethod("GetSelectedClassSearchTerm");
                //var getSingleClassLabel = typeof(SolastaMultiClass.Models.ClassPicker).GetMethod("GetSingleClassLabel");
                //var getSingleClassDescription = typeof(SolastaMultiClass.Models.ClassPicker).GetMethod("GetSingleClassDescription");
                var found = 0;
                var instructionsToBypass = 0;

                foreach (var instruction in instructions)
                {
                    if (instructionsToBypass > 0)
                    {
                        instructionsToBypass -= 1;
                    }
                    //else if (instruction.LoadsField(AccessTools.Field(AccessTools.TypeByName("CharacterInformationPanel"), "classLabel")))
                    //{
                    //    yield return instruction;
                    //    yield return new CodeInstruction(OpCodes.Call, getSingleClassLabel);
                    //    instructionsToBypass = 3;
                    //}
                    //else if (instruction.LoadsField(AccessTools.Field(AccessTools.TypeByName("CharacterInformationPanel"), "classDescription")))
                    //{
                    //    yield return instruction;
                    //    yield return new CodeInstruction(OpCodes.Call, getSingleClassDescription);
                    //    instructionsToBypass = 3;
                    //}
                    else if (instruction.Calls(containsMethod))
                    {
                        found++;
                        if (found == 2 || found == 3)
                        {
                            yield return new CodeInstruction(OpCodes.Call, getSelectedClassSearchTermMethod);
                        }
                        yield return instruction;
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }
    }
}

//private static void Prefix(CharacterInformationPanel __instance,
//                   GuiLabel ___raceLabel,
//                   Image ___raceImage,
//                   ScrollRect ___raceFeaturesScrollRect,
//                   GuiLabel ___raceDescription,
//                   RectTransform ___raceFeaturesTable,
//                   GuiLabel ___classLabel,
//                   Image ___classImage,
//                   RectTransform ___classBadgesTable,
//                   GameObject ___classBadgePrefab,
//                   GuiLabel ___classDescription,
//                   ScrollRect ___classFeaturesScrollRect,
//                   RectTransform ___classFeaturesTable,
//                   GuiLabel ___backgroundLabel,
//                   Image ___backgroundImage,
//                   GuiLabel ___backgroundDescription,
//                   ScrollRect ___backgroundFeaturesScrollRect,
//                   RectTransform ___backgroundFeaturesTable,
//                   GameObject ___featurePrefab,
//                   List<FeatureUnlockByLevel> ___filteredFeatures,
//                   List<FeatureUnlockByLevel> ___raceFeatures,
//                   List<FeatureUnlockByLevel> ___classFeatures,
//                   List<FeatureUnlockByLevel> ___backgroundFeatures,
//                   List<BaseDefinition> ___badgeDefinitions)