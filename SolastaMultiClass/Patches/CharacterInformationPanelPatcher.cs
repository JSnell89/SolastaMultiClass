using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static SolastaMultiClass.Models.GameUi;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterInformationPanelPatcher
    {
        internal static List<FightingStyleDefinition> GetClassBadges(RulesetCharacterHero rulesetCharacterHero)
        {
            var idx = 0;
            var classNames = new List<string>() { };
            var fightingStylePerClass = new Dictionary<string, List<FightingStyleDefinition>>()                    {
                        {"Paladin", new List<FightingStyleDefinition>() { } },
                        {"Ranger", new List<FightingStyleDefinition>() { } },
                        {"Fighter", new List<FightingStyleDefinition>() { } },
                    };

            foreach (var activeFeature in rulesetCharacterHero.ActiveFeatures)
            {
                if (activeFeature.Key.Contains("03Class"))
                {
                    foreach (FeatureDefinition featureDefinition in activeFeature.Value)
                    {
                        if (featureDefinition is FeatureDefinitionFightingStyleChoice featureDefinitionFightingStyleChoice)
                        {
                            var temp = activeFeature.Key.Substring(7, 7);
                            classNames.Add(temp == "Ranger1" ? "Ranger" : temp);
                        }
                    }
                }
            }

            // need to get the classes using this collection order
            // this ensures I'll try my best to find an existing class to take the feature
            foreach (var className in fightingStylePerClass.Keys)
            {
                if (!classNames.Contains(className))
                {
                    continue;
                }

                LABEL_RETRY_NEXT_FEATURE_IN_CASE_PREVIOUS_DIDNT_FIT:

                var trainedFightingStyle = rulesetCharacterHero.TrainedFightingStyles[idx++];

                switch (trainedFightingStyle.Name)
                {
                    case "Archery":
                        if (className != "Paladin")
                        {
                            fightingStylePerClass[className].Add(trainedFightingStyle);
                        }
                        break;

                    case "TwoWeapon":
                        if (className != "Paladin")
                        {
                            fightingStylePerClass[className].Add(trainedFightingStyle);
                        }
                        break;


                    case "GreatWeapon":
                        if (className != "Ranger")
                        {
                            fightingStylePerClass[className].Add(trainedFightingStyle);
                        }
                        break;

                    case "Protection":
                        if (className != "Ranger")
                        {
                            fightingStylePerClass[className].Add(trainedFightingStyle);
                        }
                        break;

                    case "Defense":
                    case "Dueling":
                        fightingStylePerClass[className].Add(trainedFightingStyle);
                        break;

                    // this is the rare case when a fighting style is assigned by a feat
                    // in this case the fighting style list is bigger than the number of features
                    default:
                        idx++;
                        goto LABEL_RETRY_NEXT_FEATURE_IN_CASE_PREVIOUS_DIDNT_FIT;
                }
            }
            return fightingStylePerClass[GetSelectedClassSearchTerm("")];
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

                    ___badgeDefinitions.AddRange(GetClassBadges(rulesetCharacterHero));

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