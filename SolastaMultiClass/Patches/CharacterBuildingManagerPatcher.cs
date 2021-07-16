using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SolastaMultiClass.Patches
{
    internal static class CharacterBuildingManagerPatcher
    {        
        // ensure the level up process doesn't offer spells from a class not leveling up
        [HarmonyPatch(typeof(CharacterBuildingManager), "UpgradeSpellPointPools")]
        internal static class CharacterBuildingManager_UpgradeSpellPointPools_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance)
            {
                foreach (RulesetSpellRepertoire spellRepertoire in __instance.HeroCharacter.SpellRepertoires)
                {
                    string poolName = string.Empty;
                    CharacterClassDefinition characterClassDefinition;
                    int num;

                    if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class)
                    {
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out num);

                        // short circuit if the feature is for another class otherwise (change from native code)
                        if (spellRepertoire.SpellCastingClass != characterClassDefinition)
                            continue;

                        poolName = AttributeDefinitions.GetClassTag(characterClassDefinition, num);
                    }
                    else if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                    {
                        __instance.GetLastAssignedClassAndLevel(out characterClassDefinition, out num);
                        CharacterSubclassDefinition item = __instance.HeroCharacter.ClassesAndSubclasses[characterClassDefinition];

                        // short circuit if the feature is for another subclass (change from native code)
                        if (spellRepertoire.SpellCastingSubclass != characterClassDefinition)
                            continue;

                        poolName = AttributeDefinitions.GetSubclassTag(characterClassDefinition, num, item);
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
                    var tempAcquiredCantripsNumberFieldInfo = characterBuildingManagerType.GetField("tempAcquiredCantripsNumber", BindingFlags.NonPublic | BindingFlags.Instance);
                    var tempAcquiredSpellsNumberFieldInfo = characterBuildingManagerType.GetField("tempAcquiredSpellsNumber", BindingFlags.NonPublic | BindingFlags.Instance);

                    tempAcquiredCantripsNumberFieldInfo.SetValue(__instance, 0);
                    tempAcquiredSpellsNumberFieldInfo.SetValue(__instance, 0);

                    //Make sure not to recurse indefinitely!  The call here is needed 
                    applyFeatureCastSpellMethod.Invoke(__instance, new object[] { spellRepertoire.SpellCastingFeature });

                    int tempCantrips = (int)tempAcquiredCantripsNumberFieldInfo.GetValue(__instance);
                    int tempSpells = (int)tempAcquiredSpellsNumberFieldInfo.GetValue(__instance);

                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Cantrip, poolName, tempCantrips + maxPoints });
                    setPointPoolMethod.Invoke(__instance, new object[] { HeroDefinitions.PointsPoolType.Spell, poolName, tempSpells });
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterBuildingManager), "GetSpellFeature")]
        internal static class CharacterBuildingManager_GetSpellFeature_Patch
        {
            internal static bool Prefix(CharacterBuildingManager __instance, string tag, ref FeatureDefinitionCastSpell __result)
            {
                FeatureDefinitionCastSpell featureDefinitionCastSpell = null;
                string str = tag;

                if (str.StartsWith("03Class"))
                {
                    str = tag.Substring(0, tag.Length - 2); // removes any levels from the tag examples are 03ClassRanger2, 03ClassRanger20. This is a bit lazy but no class will have a tag where the class name is only 1 character.  
                                                            // old Solasta code was str = "03Class"; which lead to getting the first spell feature from any class
                }
                else if (str.StartsWith("06Subclass"))
                {
                    str = tag.Substring(0, tag.Length - 2); // similar to above just with subclasses
                                                            // old Solasta code was str = "06Subclass"; which lead to getting the first spell feature from any subclass
                }

                Dictionary<string, List<FeatureDefinition>>.Enumerator enumerator = __instance.HeroCharacter.ActiveFeatures.GetEnumerator();

                try
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, List<FeatureDefinition>> current = enumerator.Current;
                        if (!current.Key.StartsWith(str))
                        {
                            continue;
                        }
                        List<FeatureDefinition>.Enumerator enumerator1 = current.Value.GetEnumerator();
                        try
                        {
                            while (enumerator1.MoveNext())
                            {
                                FeatureDefinition featureDefinition = enumerator1.Current;
                                if (!(featureDefinition is FeatureDefinitionCastSpell))
                                {
                                    continue;
                                }
                                featureDefinitionCastSpell = featureDefinition as FeatureDefinitionCastSpell;
                                __result = featureDefinitionCastSpell;
                                return false;
                            }
                        }
                        finally
                        {
                            ((IDisposable)enumerator1).Dispose();
                        }
                    }
                }
                finally
                {
                    ((IDisposable)enumerator).Dispose();
                }
                __result = featureDefinitionCastSpell;
                return false;
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