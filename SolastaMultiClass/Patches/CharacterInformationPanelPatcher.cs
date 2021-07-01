using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static SolastaMultiClass.Models.MultiClass;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterInformationPanelPatcher
    {
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

                    // TODO: Is there any simple way to determine which class gave the Fighting Style?

                    foreach (BaseDefinition trainedFightingStyle in rulesetCharacterHero.TrainedFightingStyles)
                    {
                        ___badgeDefinitions.Add(trainedFightingStyle);
                    }

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
                var getSelectedClassSearchTermMethod = typeof(SolastaMultiClass.Models.MultiClass).GetMethod("GetSelectedClassSearchTerm");
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