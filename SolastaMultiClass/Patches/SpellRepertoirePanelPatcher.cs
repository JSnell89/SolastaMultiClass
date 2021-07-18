﻿using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;

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
            RectTransform ___levelButtonsTable,
            RulesetCharacter caster,
            RulesetSpellRepertoire spellRepertoire,
            SpellRepertoirePanel.SpellRepertoireChangedHandler spellRepertoireChangedForParent,
            SpellBox.BindMode bindMode,
            ActionDefinitions.InventoryManagementMode inventoryMode)
        {
            // determine the display context
            var rulesetCharacterHero = __instance.GuiCharacter.RulesetCharacterHero;
            var characterClassDefinition = __instance.SpellRepertoire.SpellCastingClass;
            var characterSubclassDefinition = __instance.SpellRepertoire.SpellCastingSubclass;
            var maxLevelOfSpellCastingForClass = (int)Math.Floor((Models.SharedSpellsRules.GetClassCasterLevel(rulesetCharacterHero, characterClassDefinition, characterSubclassDefinition) + 1) / 2.0);
            int accountForCantrips = __instance.SpellRepertoire.SpellCastingFeature.SpellListDefinition.HasCantrips ? 1 : 0;

            // patches the spell level buttons to be hidden if no spells available at that level
            for (int i = 0; i < ___levelButtonsTable.childCount; i++)
            {
                Transform child = ___levelButtonsTable.GetChild(i);

                child.gameObject.SetActive(i < maxLevelOfSpellCastingForClass + accountForCantrips);
            }

            // patches the panel to display higher level spell slots from shared slots table but hide the spell panels if class level not there yet
            for (int i = 0; i < ___spellsByLevelTable.childCount; i++)
            {
                Transform child = ___spellsByLevelTable.GetChild(i);

                for (int k = 0; k < child.childCount; k++)
                {
                    Transform grandChild = child.GetChild(k);
                    
                    if (!grandChild.TryGetComponent(typeof(SlotStatusTable), out Component _))  // don't hide the spell slot status so people can see how many slots they have
                    {
                        grandChild.gameObject.SetActive(i < maxLevelOfSpellCastingForClass + accountForCantrips);
                    }
                    //SpellsByLevelGroup component2 = child.GetComponent<SpellsByLevelGroup>();
                    //component2.BindInspectionOrPreparation(rulesetCharacterHero, spellRepertoire, spellRepertoire.SpellCastingFeature, i, bindMode, null);
                }

            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(___spellsByLevelTable);

            // hides the sorcery points UI if not a sorcerer caster
            var active = __instance.SpellRepertoire?.SpellCastingClass == Sorcerer && rulesetCharacterHero.ClassesAndLevels.ContainsKey(Sorcerer) && rulesetCharacterHero.ClassesAndLevels[Sorcerer] >= 2;

            ___sorceryPointsBox.gameObject.SetActive(active);
            ___sorceryPointsLabel.gameObject.SetActive(active);
        }
    }
}