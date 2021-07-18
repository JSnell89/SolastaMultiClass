using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    [HarmonyPatch(typeof(SpellsByLevelGroup), "BindLearning")]
    internal static class SpellsByLevelGroup_BindLearning_Patch
    {
        internal static void Postfix(SpellsByLevelGroup __instance, ICharacterBuildingService characterBuildingService, SpellListDefinition spellListDefinition, List<string> restrictedSchools, int spellLevel, SpellBox.SpellBoxChangedHandler spellBoxChanged, List<SpellDefinition> knownSpells, List<SpellDefinition> unlearnedSpells, string spellTag, bool canAcquireSpells, bool unlearn)
        {
            if (Models.LevelUpContext.LevelingUp)
            {
                __instance.SpellLevel = spellLevel;
                List<FeatureDefinition> features = (List<FeatureDefinition>)AccessTools.Field(__instance.GetType(), "features").GetValue(__instance);
                List<SpellDefinition> autoPreparedSpells = (List<SpellDefinition>)AccessTools.Field(__instance.GetType(), "autoPreparedSpells").GetValue(__instance);
                SlotStatusTable slotStatusTable = (SlotStatusTable)AccessTools.Field(__instance.GetType(), "slotStatusTable").GetValue(__instance);

                // solasta engine code
                List<SpellDefinition> spellDefinitions = new List<SpellDefinition>();
                foreach (SpellDefinition spell in spellListDefinition.SpellsByLevel[(spellListDefinition.HasCantrips ? spellLevel : spellLevel - 1)].Spells)
                {
                    if (restrictedSchools.Count != 0 && !restrictedSchools.Contains(spell.SchoolOfMagic))
                    {
                        continue;
                    }
                    spellDefinitions.Add(spell);
                }
                foreach (SpellDefinition spellDefinition in characterBuildingService.EnumerateKnownAndAcquiredSpells(string.Empty))
                {
                    if (spellDefinition.SpellLevel != spellLevel || spellDefinitions.Contains(spellDefinition))
                    {
                        continue;
                    }
                    spellDefinitions.Add(spellDefinition);
                }

                // ACTUAL PATCH: remove any features that aren't part of the class/subclass combo that just leveled up
                Models.LevelUpContext.SelectedHero.EnumerateFeaturesToBrowse<FeatureDefinitionMagicAffinity>(features, null);
                List<FeatureDefinition> characterClassAndSubclassFeatures = Models.LevelUpContext.SelectedClass.FeatureUnlocks.FindAll(fubl => fubl.Level <= Models.LevelUpContext.SelectedHeroLevel).Select(fubl => fubl.FeatureDefinition).ToList();
                
                Models.LevelUpContext.SelectedHero.ClassesAndSubclasses.TryGetValue(Models.LevelUpContext.SelectedClass, out CharacterSubclassDefinition characterSubclassDefinition);
                if (characterSubclassDefinition != null)
                {
                    characterClassAndSubclassFeatures.AddRange(characterSubclassDefinition.FeatureUnlocks.FindAll(fubl => fubl.Level <= Models.LevelUpContext.SelectedHeroLevel).Select(fubl => fubl.FeatureDefinition));
                }
                features.RemoveAll(f => !characterClassAndSubclassFeatures.Contains(f));

                //
                // solasta engine code from here
                //

                foreach (FeatureDefinitionMagicAffinity feature in features)
                {
                    if (feature.ExtendedSpellList == null)
                    {
                        continue;
                    }
                    foreach (SpellDefinition spell1 in feature.ExtendedSpellList.SpellsByLevel[(spellListDefinition.HasCantrips ? spellLevel : spellLevel - 1)].Spells)
                    {
                        if (spellDefinitions.Contains(spell1) || restrictedSchools.Count != 0 && !restrictedSchools.Contains(spell1.SchoolOfMagic))
                        {
                            continue;
                        }
                        spellDefinitions.Add(spell1);
                    }
                }
                autoPreparedSpells.Clear();
                string empty = string.Empty;

                //
                // NEED TO CLEAN UP BELOW ENUMERATOR CODE...
                //

                if (__instance.SpellLevel > 0)
                {
                    characterBuildingService.HeroCharacter.EnumerateFeaturesToBrowse<FeatureDefinitionAutoPreparedSpells>(features, null);
                    List<FeatureDefinition>.Enumerator enumerator = features.GetEnumerator();
                    try
                    {
                        if (enumerator.MoveNext())
                        {
                            FeatureDefinitionAutoPreparedSpells current = (FeatureDefinitionAutoPreparedSpells)enumerator.Current;
                            empty = current.AutoPreparedTag;
                            foreach (FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup autoPreparedSpellsGroup in current.AutoPreparedSpellsGroups)
                            {
                                foreach (SpellDefinition spellsList in autoPreparedSpellsGroup.SpellsList)
                                {
                                    if (spellsList.SpellLevel != __instance.SpellLevel)
                                    {
                                        continue;
                                    }
                                    autoPreparedSpells.Add(spellsList);
                                }
                            }
                            foreach (SpellDefinition autoPreparedSpell in autoPreparedSpells)
                            {
                                if (spellDefinitions.Contains(autoPreparedSpell))
                                {
                                    continue;
                                }
                                spellDefinitions.Add(autoPreparedSpell);
                            }
                        }
                    }
                    finally
                    {
                        ((IDisposable)enumerator).Dispose();
                    }
                }
                IGamingPlatformService service = ServiceRepository.GetService<IGamingPlatformService>();
                for (int i = spellDefinitions.Count - 1; i >= 0; i--)
                {
                    if (!service.IsContentPackAvailable(spellDefinitions[i].ContentPack))
                    {
                        spellDefinitions.RemoveAt(i);
                    }
                }
                __instance.CommonBind(null, (unlearn ? SpellBox.BindMode.Unlearn : SpellBox.BindMode.Learning), spellBoxChanged, spellDefinitions, null, autoPreparedSpells, unlearnedSpells, empty);
                if (!unlearn)
                {
                    __instance.RefreshLearning(characterBuildingService, knownSpells, unlearnedSpells, spellTag, canAcquireSpells);
                }
                else
                {
                    __instance.RefreshUnlearning(characterBuildingService, knownSpells, unlearnedSpells, spellTag, (!canAcquireSpells ? false : spellLevel > 0));
                }
                slotStatusTable.Bind(null, spellLevel, null, false);
            }
        }
    }
}