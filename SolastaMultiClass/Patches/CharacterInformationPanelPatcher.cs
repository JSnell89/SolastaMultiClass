using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static SolastaMultiClass.Models.GameUi;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterInformationPanelPatcher
    {
        internal static List<FightingStyleDefinition> GetClassBadges(RulesetCharacterHero rulesetCharacterHero, string className)
        {
            var lenTagClass = AttributeDefinitions.TagClass.Length;
            var classBadges = new List<FightingStyleDefinition>() { };
            var classLevelFightingStyle = new Dictionary<string, FightingStyleDefinition>() { };
            var fightingStyleidx = 0;

            // uses the collections ordering to determine which class got the style
            // this algorithm won't give the proper answer if CJD's features are installed
            // will fix that on next version
            foreach (var activeFeature in rulesetCharacterHero.ActiveFeatures)
            {
                if (activeFeature.Key.Contains(AttributeDefinitions.TagClass))
                {
                    foreach (FeatureDefinition featureDefinition in activeFeature.Value)
                    {
                        if (featureDefinition is FeatureDefinitionFightingStyleChoice featureDefinitionFightingStyleChoice)
                        {
                            classLevelFightingStyle.Add(activeFeature.Key.Substring(lenTagClass), rulesetCharacterHero.TrainedFightingStyles[fightingStyleidx++]);
                        }
                    }
                }
            }

            foreach (var tuple in classLevelFightingStyle)
            {
                if (className == "Fighter" && (tuple.Key == "Figther1" || tuple.Key == "Fighter10"))
                {
                    classBadges.Add(tuple.Value);
                }
                else if (className == "Paladin" && tuple.Key == "Paladin2")
                {
                    classBadges.Add(tuple.Value);
                }
                else if (className == "Ranger" && tuple.Key == "Ranger2")
                {
                    classBadges.Add(tuple.Value);
                }
            }
            return classBadges;
        }

        [HarmonyPatch(typeof(CharacterInformationPanel), "EnumerateClassBadges")]
        internal static class CharacterInformationPanel_EnumerateClassBadges_Patch
        {
            internal static bool Prefix(CharacterInformationPanel __instance,
                                       RectTransform ___classBadgesTable,
                                       GameObject ___classBadgePrefab,
                                       List<BaseDefinition> ___badgeDefinitions)
            {
                if (__instance.InspectedCharacter.RulesetCharacterHero.ClassesHistory.Count > 1)
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

                    ___badgeDefinitions.AddRange(GetClassBadges(rulesetCharacterHero, GetSelectedClassSearchTerm("")));

                    while (___classBadgesTable.childCount < ___badgeDefinitions.Count)
                    {
                        Gui.GetPrefabFromPool(___classBadgePrefab, ___classBadgesTable);
                    }

                    int index = 0;
                    foreach (BaseDefinition badgeDefinition in ___badgeDefinitions)
                    {
                        Transform child = ___classBadgesTable.GetChild(index);
                        child.gameObject.SetActive(true);
                        child.GetComponent<CharacterInformationBadge>().Bind(badgeDefinition, ___classBadgesTable);
                        ++index;
                    }
                    for (; index < ___classBadgesTable.childCount; ++index)
                    {
                        Transform child = ___classBadgesTable.GetChild(index);
                        child.GetComponent<CharacterInformationBadge>().Unbind();
                        child.gameObject.SetActive(false);
                    }

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterInformationPanel), "Refresh")]
        internal static class CharacterInformationPanel_Refresh_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var containsMethod = typeof(string).GetMethod("Contains");
                var getSelectedClassSearchTermMethod = typeof(SolastaMultiClass.Models.GameUi).GetMethod("GetSelectedClassSearchTerm");
                var found = 0;
                var instructionsToBypass = 0;

                foreach (var instruction in instructions)
                {
                    if (instructionsToBypass > 0)
                    {
                        instructionsToBypass -= 1;
                    }
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