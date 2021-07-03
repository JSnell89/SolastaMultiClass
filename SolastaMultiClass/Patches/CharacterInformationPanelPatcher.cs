using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static SolastaMultiClass.Models.GameUi;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterInformationPanelPatcher
    {
        //
        // code below isn't pythonic ;-) wish there were an easier way but the game data structures don't proper track who gave what on fighting styles
        //
        internal static List<FightingStyleDefinition> GetClassBadges(RulesetCharacterHero rulesetCharacterHero)
        {

            var classNames = new List<string>() { };

            // collects all #CLASS#LEVEL tags where a fighting style got granted
            foreach (var activeFeature in rulesetCharacterHero.ActiveFeatures)
            {
                if (activeFeature.Key.Contains("03Class"))
                {
                    foreach (FeatureDefinition featureDefinition in activeFeature.Value)
                    {
                        if (featureDefinition is FeatureDefinitionFightingStyleChoice featureDefinitionFightingStyleChoice)
                        {
                            classNames.Add(activeFeature.Key.Substring(7));
                        }
                    }
                }
            }

            var idx = 0;
            var fightingStylePerClass = new Dictionary<string, List<FightingStyleDefinition>>()                    {
                        {"Paladin2", new List<FightingStyleDefinition>() { } },
                        {"Ranger2", new List<FightingStyleDefinition>() { } },
                        {"Fighter1", new List<FightingStyleDefinition>() { } },
                        {"Fighter10", new List<FightingStyleDefinition>() { } },
                    };

            // now traverse above buckets in order and try to find the best class who could own the fighting style
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
                        if (className != "Paladin2")
                        {
                            fightingStylePerClass[className].Add(trainedFightingStyle);
                        }
                        break;

                    case "TwoWeapon":
                        if (className != "Paladin2")
                        {
                            fightingStylePerClass[className].Add(trainedFightingStyle);
                        }
                        break;


                    case "GreatWeapon":
                        if (className != "Ranger2")
                        {
                            fightingStylePerClass[className].Add(trainedFightingStyle);
                        }
                        break;

                    case "Protection":
                        if (className != "Ranger2")
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
                    // i.e: a Paladin who takes a GreatWeapon and later on takes Archery on a feat... 
                    default:
                        idx++;
                        goto LABEL_RETRY_NEXT_FEATURE_IN_CASE_PREVIOUS_DIDNT_FIT;
                }
            }

            fightingStylePerClass["Fighter1"].AddRange(fightingStylePerClass["Fighter10"]);

            switch (GetSelectedClassSearchTerm(""))
            {
                case "Fighter":
                    return fightingStylePerClass["Fighter1"];

                case "Paladin":
                    return fightingStylePerClass["Paladin2"];

                case "Ranger":
                    return fightingStylePerClass["Ranger2"];

                default:
                    return new List<FightingStyleDefinition>() { };
            }
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