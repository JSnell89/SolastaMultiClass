using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using SolastaMultiClass.Models;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterInformationPanelPatcher
    {
        [HarmonyPatch(typeof(CharacterInformationPanel), "EnumerateClassBadges")]
        internal static class CharacterInformationPanel_EnumerateClassBadges_Patch
        {
            internal static bool Prefix(CharacterInformationPanel __instance, List<BaseDefinition> ___badgeDefinitions, RectTransform ___classBadgesTable, GameObject ___classBadgePrefab)
            {
                ___badgeDefinitions.Clear();
                var rulesetCharacterHero = __instance.InspectedCharacter.RulesetCharacterHero;
                foreach (KeyValuePair<CharacterClassDefinition, CharacterSubclassDefinition> classesAndSubclass in rulesetCharacterHero.ClassesAndSubclasses)
                    if (classesAndSubclass.Key == InspectionPanelContext.GetSelectedClass())
                        ___badgeDefinitions.Add(classesAndSubclass.Value);
                if (rulesetCharacterHero.DeityDefinition != null && (InspectionPanelContext.GetSelectedClass() == Paladin || InspectionPanelContext.GetSelectedClass() == Cleric))
                    ___badgeDefinitions.Add(rulesetCharacterHero.DeityDefinition);
                foreach (var trainedFightingStyle in InspectionPanelContext.GetTrainedFightingStyles(rulesetCharacterHero))
                    ___badgeDefinitions.Add(trainedFightingStyle);
                while (___classBadgesTable.childCount < ___badgeDefinitions.Count)
                    Gui.GetPrefabFromPool(___classBadgePrefab, (Transform)___classBadgesTable);
                int index = 0;
                foreach (var badgeDefinition in ___badgeDefinitions)
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
        }

        [HarmonyPatch(typeof(CharacterInformationPanel), "Refresh")]
        internal static class CharacterInformationPanel_Refresh_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var containsMethod = typeof(string).GetMethod("Contains");
                var getSelectedClassSearchTermMethod = typeof(InspectionPanelContext).GetMethod("GetSelectedClassSearchTerm");
                var found = 0;

                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(containsMethod))
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