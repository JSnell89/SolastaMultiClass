using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static SolastaMultiClass.Models.SharedSpellsRules;

namespace SolastaMultiClass.Patches
{
    // patches the panel to display higher level spell slots from shared slots table but hide the spell panels if class level not there yet
    [HarmonyPatch(typeof(SpellRepertoirePanel), "Bind")]
    internal static class SpellRepertoirePanel_Bind_Patch
    {
        internal static void Postfix(SpellRepertoirePanel __instance)
        {
            // hides the sorcery points if required
            if (__instance.SpellRepertoire.SpellCastingFeature.SpellReadyness == RuleDefinitions.SpellReadyness.Prepared)
            {
                var sorceryPointsBox = (RectTransform)AccessTools.Field(__instance.GetType(), "sorceryPointsBox").GetValue(__instance);
                var sorceryPointsLabel = (GuiLabel)AccessTools.Field(__instance.GetType(), "sorceryPointsLabel").GetValue(__instance);
                sorceryPointsBox.gameObject.SetActive(false);
                sorceryPointsLabel.gameObject.SetActive(false);
            }

            // this may not work for subclasses that have 'Prepared' spells, but I don't think any do
            var characterClassDefinition = __instance.SpellRepertoire.SpellCastingClass;
            var rulesetCharacterHero = __instance.GuiCharacter.RulesetCharacterHero;
            var maxLevelOfSpellCastingForClass = (int)Math.Ceiling(GetHeroSharedCasterLevel(rulesetCharacterHero, characterClassDefinition) / 2.0);

            var spellRepertoirePanelType = typeof(SpellRepertoirePanel);
            var spellsByLevelTableFieldInfo = spellRepertoirePanelType.GetField("spellsByLevelTable", BindingFlags.NonPublic | BindingFlags.Instance);

            UnityEngine.RectTransform spellsByLevelRect = (UnityEngine.RectTransform)spellsByLevelTableFieldInfo.GetValue(__instance);

            int childCount = spellsByLevelRect.childCount;
            int accountForCantrips = __instance.SpellRepertoire.SpellCastingFeature.SpellListDefinition.HasCantrips ? 1 : 0;

            for (int i = 0; i < childCount; i++)
            {
                Transform transforms = spellsByLevelRect.GetChild(i);
                for (int k = 0; k < transforms.childCount; k++)
                {
                    var child = transforms.GetChild(k);

                    // don't hide the spell slot status so people can see how many slots they have even if they don't have spells of that level
                    if (child.TryGetComponent(typeof(SlotStatusTable), out Component _))
                        continue;

                    if (i > (maxLevelOfSpellCastingForClass + accountForCantrips) - 1)
                    {
                        child.gameObject.SetActive(false);
                    }
                    else
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(spellsByLevelRect);
        }
    }
}