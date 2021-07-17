using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterBuildingManagerPatcher
    {
        // ensures the level up process only considers the leveling up class when enumerating known spells
        [HarmonyPatch(typeof(CharacterBuildingManager), "EnumerateKnownAndAcquiredSpells")]
        internal static class CharacterBuildingManager_EnumerateKnownAndAcquiredSpells_Patch
        {
            internal static bool Prefix(
                string tagToIgnore,
                CharacterBuildingManager __instance,
                List<FeatureDefinition> ___matchingFeatures,
                Dictionary<string, List<SpellDefinition>> ___bonusCantrips,
                Dictionary<string, List<SpellDefinition>> ___acquiredCantrips,
                Dictionary<string, List<SpellDefinition>> ___acquiredSpells,
                ref List<SpellDefinition> __result)
            {
                ___matchingFeatures.Clear();
                List<SpellDefinition> spellDefinitionList = new List<SpellDefinition>();
                foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                {
                    // short circuit here to only consider the actual class leveling up
                    if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class &&
                        spellRepertoire.SpellCastingClass != Models.LevelUpContext.SelectedClass ||
                        spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass &&
                        spellRepertoire.SpellCastingSubclass != Models.LevelUpContext.SelectedSubclass)
                        continue;

                    foreach (SpellDefinition knownCantrip in spellRepertoire.KnownCantrips)
                    {
                        if (!spellDefinitionList.Contains(knownCantrip))
                            spellDefinitionList.Add(knownCantrip);
                    }
                    foreach (SpellDefinition knownSpell in spellRepertoire.KnownSpells)
                    {
                        if (!spellDefinitionList.Contains(knownSpell))
                            spellDefinitionList.Add(knownSpell);
                    }
                    spellDefinitionList.AddRange((IEnumerable<SpellDefinition>)spellRepertoire.EnumerateAvailableScribedSpells());
                }
                foreach (KeyValuePair<string, List<SpellDefinition>> bonusCantrip in ___bonusCantrips)
                {
                    if (bonusCantrip.Key != tagToIgnore)
                    {
                        foreach (SpellDefinition spellDefinition in bonusCantrip.Value)
                        {
                            if (!spellDefinitionList.Contains(spellDefinition))
                                spellDefinitionList.Add(spellDefinition);
                        }
                    }
                }
                foreach (KeyValuePair<string, List<SpellDefinition>> acquiredCantrip in ___acquiredCantrips)
                {
                    if (acquiredCantrip.Key != tagToIgnore)
                    {
                        foreach (SpellDefinition spellDefinition in acquiredCantrip.Value)
                        {
                            if (!spellDefinitionList.Contains(spellDefinition))
                                spellDefinitionList.Add(spellDefinition);
                        }
                    }
                }
                foreach (KeyValuePair<string, List<SpellDefinition>> acquiredSpell in ___acquiredSpells)
                {
                    if (acquiredSpell.Key != tagToIgnore)
                    {
                        foreach (SpellDefinition spellDefinition in acquiredSpell.Value)
                        {
                            if (!spellDefinitionList.Contains(spellDefinition))
                                spellDefinitionList.Add(spellDefinition);
                        }
                    }
                }
                __result = spellDefinitionList;
                return false;
            }
        }

        // ensures the level up process only offers spells from the leveling up class
        [HarmonyPatch(typeof(CharacterBuildingManager), "UpgradeSpellPointPools")]
        internal static class CharacterBuildingManager_UpgradeSpellPointPools_Patch
        {
            internal static bool Prefix(
                CharacterBuildingManager __instance,
                ref int ___tempAcquiredCantripsNumber,
                ref int ___tempAcquiredSpellsNumber,
                ref int ___tempUnlearnedSpellsNumber)
            {
                foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                {
                    string poolName = string.Empty;
                    CharacterClassDefinition characterClassDefinition;
                    int classLevel;

                    if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class)
                    {
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out classLevel);

                        // short circuit if the feature is for another class (change from native code)
                        if (spellRepertoire.SpellCastingClass != characterClassDefinition)
                            continue;

                        poolName = AttributeDefinitions.GetClassTag(characterClassDefinition, classLevel);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                    {
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out classLevel);

                        if (!__instance.HeroCharacter.ClassesAndSubclasses.ContainsKey(characterClassDefinition))
                            continue;

                        CharacterSubclassDefinition characterSubclassDefinition = __instance.HeroCharacter.ClassesAndSubclasses[characterClassDefinition];
                        
                        // short circuit if the feature is for another subclass (change from native code)
                        if (spellRepertoire.SpellCastingSubclass != characterSubclassDefinition)
                            continue;

                        poolName = AttributeDefinitions.GetSubclassTag(characterClassDefinition, classLevel, characterSubclassDefinition);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Race)
                    {
                        poolName = "02Race";
                    }

                    int maxPoints = 0;

                    if (__instance.HasAnyActivePoolOfType(HeroDefinitions.PointsPoolType.Cantrip) && __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools.ContainsKey(poolName))
                    {
                        maxPoints = __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools[poolName].MaxPoints;
                    }

                    var characterBuildingManagerType = typeof(CharacterBuildingManager);
                    var applyFeatureCastSpellMethod = characterBuildingManagerType.GetMethod("ApplyFeatureCastSpell", BindingFlags.NonPublic | BindingFlags.Instance);
                    var setPointPoolMethod = characterBuildingManagerType.GetMethod("SetPointPool", BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    ___tempAcquiredCantripsNumber = 0;
                    ___tempAcquiredSpellsNumber = 0;
                    ___tempUnlearnedSpellsNumber = 0;

                    applyFeatureCastSpellMethod.Invoke(__instance, new object[] { spellRepertoire.SpellCastingFeature });
                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Cantrip, poolName, ___tempAcquiredCantripsNumber + maxPoints });
                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Spell, poolName, ___tempAcquiredSpellsNumber });
                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.SpellUnlearn, poolName, ___tempUnlearnedSpellsNumber });
                }
                return false;
            }
        }

        // removes any levels from the tag otherwise it leads to getting the first spell feature from any class
        [HarmonyPatch(typeof(CharacterBuildingManager), "GetSpellFeature")]
        internal static class CharacterBuildingManager_GetSpellFeature_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance, string tag, ref FeatureDefinitionCastSpell __result)
            {
                if (Models.LevelUpContext.SelectedSubclass == null)
                {
                    if (tag.StartsWith("03Class") || tag.StartsWith("06Subclass"))
                    {
                        tag = tag.TrimEnd(new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                    }
                    __result = null;
                    foreach (KeyValuePair<string, List<FeatureDefinition>> activeFeature in __instance.HeroCharacter.ActiveFeatures)
                    {
                        if (activeFeature.Key.StartsWith(tag))
                        {
                            foreach (FeatureDefinition featureDefinition in activeFeature.Value)
                            {
                                if (featureDefinition is FeatureDefinitionCastSpell)
                                    __result = featureDefinition as FeatureDefinitionCastSpell;
                            }
                        }
                    }
                    return false;
                }
                return true;
            }
        }

        // captures the desired class and ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "AssignClassLevel")]
        internal static class CharacterBuildingManager_AssignClassLevel_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance, CharacterClassDefinition classDefinition)
            {
                if (Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel)
                {
                    Models.LevelUpContext.SelectedClass = classDefinition;
                }
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "ClearWieldedConfigurations")]
        internal static class CharacterBuildingManager_ClearWieldedConfigurations_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantBaseEquipment")]
        internal static class CharacterBuildingManager_GrantBaseEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "GrantFeatures")]
        internal static class CharacterBuildingManager_GrantFeatures_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "RemoveBaseEquipment")]
        internal static class CharacterBuildingManager_RemoveBaseEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignEquipment")]
        internal static class CharacterBuildingManager_UnassignEquipment_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }

        // ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "UnassignLastClassLevel")]
        internal static class CharacterBuildingManager_UnassignLastClassLevel_Patch
        {
            internal static bool Prefix()
            {
                return !(Models.LevelUpContext.LevelingUp && Models.LevelUpContext.DisplayingClassPanel);
            }
        }
    }
}