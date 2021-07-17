using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;
using static SolastaMultiClass.Models.SharedSpellsRules;

namespace SolastaMultiClass.Patches
{
    [HarmonyPatch(typeof(SpellRepertoirePanel), "Bind")]
    internal static class SpellRepertoirePanel_Bind_Patch
    {
        internal static void Postfix(
            SpellRepertoirePanel __instance, 
            RectTransform ___sorceryPointsBox, 
            GuiLabel ___sorceryPointsLabel, 
            RectTransform ___spellsByLevelTable, 
            RectTransform ___levelButtonsTable)
        {
            // hides the sorcery points UI if not a sorcerer caster
            var active = __instance.SpellRepertoire.SpellCastingClass?.Name == "Sorcerer";

            ___sorceryPointsBox.gameObject.SetActive(active);
            ___sorceryPointsLabel.gameObject.SetActive(active);

            // determine the display context
            var rulesetCharacterHero = __instance.GuiCharacter.RulesetCharacterHero;
            var characterClassDefinition = __instance.SpellRepertoire.SpellCastingClass;
            var characterSubclassDefinition = __instance.SpellRepertoire.SpellCastingSubclass;
            var maxLevelOfSpellCastingForClass = (int)Math.Ceiling(GetHeroSharedCasterLevel(rulesetCharacterHero, characterClassDefinition, characterSubclassDefinition) / 2.0);
            int accountForCantrips = __instance.SpellRepertoire.SpellCastingFeature.SpellListDefinition.HasCantrips ? 1 : 0;

            // patches the spell level buttons to be hidden if no spells available at that level
            for (int i = 0; i < ___levelButtonsTable.childCount; i++)
            {
                Transform child = ___levelButtonsTable.GetChild(i);

                if (i > (maxLevelOfSpellCastingForClass + accountForCantrips) - 1)
                {
                    child.gameObject.SetActive(false);
                }
                else
                {
                    child.gameObject.SetActive(true);
                }
            }

            // patches the panel to display higher level spell slots from shared slots table but hide the spell panels if class level not there yet
            for (int i = 0; i < ___spellsByLevelTable.childCount; i++)
            {
                Transform transforms = ___spellsByLevelTable.GetChild(i);
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
        }
    }
}