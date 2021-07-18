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
                if (Models.LevelUpContext.LevelingUp)
                {
                    List<SpellDefinition> spellDefinitionList = new List<SpellDefinition>();

                    ___matchingFeatures.Clear();
                    foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                    {
                        // PATCH: short circuit here to only consider the actual class leveling up
                        if (!Models.LevelUpContext.IsRepertoireFromSelectedClass(spellRepertoire))
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
                }
                return !Models.LevelUpContext.LevelingUp;
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
                if (Models.LevelUpContext.LevelingUp)
                {
                    foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                    {
                        string poolName = string.Empty;
                        int maxPoints = 0;

                        if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class)
                        {
                            // PATCH: short circuit if the feature is for another class (change from native code)
                            if (spellRepertoire.SpellCastingClass != Models.LevelUpContext.SelectedClass)
                                continue;

                            poolName = AttributeDefinitions.GetClassTag(Models.LevelUpContext.SelectedClass, Models.LevelUpContext.SelectedHeroLevel); // SelectedClassLevel ???
                        }
                        else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                        {
                            // PATCH: short circuit if class doesn't contain a subclass yet
                            if (!__instance.HeroCharacter.ClassesAndSubclasses.ContainsKey(Models.LevelUpContext.SelectedClass))
                                continue;

                            // PATCH: short circuit if the feature is for another subclass (change from native code)
                            if (spellRepertoire.SpellCastingSubclass != Models.LevelUpContext.SelectedSubclass)
                                continue;

                            poolName = AttributeDefinitions.GetSubclassTag(Models.LevelUpContext.SelectedClass, Models.LevelUpContext.SelectedHeroLevel, Models.LevelUpContext.SelectedSubclass); // SelectedClassLevel ???
                        }
                        else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Race)
                        {
                            poolName = "02Race";
                        }

                        var characterBuildingManagerType = typeof(CharacterBuildingManager);
                        var applyFeatureCastSpellMethod = characterBuildingManagerType.GetMethod("ApplyFeatureCastSpell", BindingFlags.NonPublic | BindingFlags.Instance);
                        var setPointPoolMethod = characterBuildingManagerType.GetMethod("SetPointPool", BindingFlags.NonPublic | BindingFlags.Instance);

                        ___tempAcquiredCantripsNumber = 0;
                        ___tempAcquiredSpellsNumber = 0;
                        ___tempUnlearnedSpellsNumber = 0;

                        applyFeatureCastSpellMethod.Invoke(__instance, new object[] { spellRepertoire.SpellCastingFeature });

                        // PATCH: don't set pool if selected class doesn't have cantrips
                        if (!Models.LevelUpContext.HasCantrips())
                        {
                            ___tempAcquiredCantripsNumber = 0;
                        } 
                        else if (__instance.HasAnyActivePoolOfType(HeroDefinitions.PointsPoolType.Cantrip) && __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools.ContainsKey(poolName))
                        {
                            maxPoints = __instance.PointPoolStacks[HeroDefinitions.PointsPoolType.Cantrip].ActivePools[poolName].MaxPoints;
                        }

                        setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Cantrip, poolName, ___tempAcquiredCantripsNumber + maxPoints });
                        setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Spell, poolName, ___tempAcquiredSpellsNumber });
                        setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.SpellUnlearn, poolName, ___tempUnlearnedSpellsNumber });
                    }
                }
                return !Models.LevelUpContext.LevelingUp;
            }
        }

        // removes any levels from the tag otherwise it'll have a hard time finding it if multiclassed
        [HarmonyPatch(typeof(CharacterBuildingManager), "GetSpellFeature")]
        internal static class CharacterBuildingManager_GetSpellFeature_Patch
        {
            internal static bool Prefix(string tag, ref FeatureDefinitionCastSpell __result)
            {
                if (Models.LevelUpContext.LevelingUp)
                {
                    if (tag.StartsWith("03Class"))
                    {
                        tag = "03Class" + Models.LevelUpContext.SelectedClass.Name;
                    }
                    if (tag.StartsWith("06Subclass"))
                    {
                        tag = "06Subclass" + Models.LevelUpContext.SelectedClass.Name;
                    }
                    __result = null;
                    foreach (KeyValuePair<string, List<FeatureDefinition>> activeFeature in Models.LevelUpContext.SelectedHero.ActiveFeatures)
                    {
                        if (activeFeature.Key.StartsWith(tag))
                        {
                            foreach (FeatureDefinition featureDefinition in activeFeature.Value)
                            {
                                if (featureDefinition is FeatureDefinitionCastSpell)
                                {
                                    __result = featureDefinition as FeatureDefinitionCastSpell;
                                }
                            }
                        }
                    }
                }
                return !Models.LevelUpContext.LevelingUp;
            }
        }

        // captures the desired class and ensures this doesn't get executed in the class panel level up screen
        [HarmonyPatch(typeof(CharacterBuildingManager), "AssignClassLevel")]
        internal static class CharacterBuildingManager_AssignClassLevel_Patch
        {
            internal static bool Prefix(CharacterClassDefinition classDefinition)
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