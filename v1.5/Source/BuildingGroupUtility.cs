using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SwitchBuilding
{
    internal class BuildingGroupUtility
    {
        #region Singleton
        private static BuildingGroupUtility _instance;

        public static BuildingGroupUtility Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuildingGroupUtility();
                    _instance.BuildCache();
                }
                return _instance;
            }
        }
        #endregion

        #region Private Parts
        private Dictionary<string, List<ThingDef>> groupCache;
        public Dictionary<string, List<ThingDef>> GroupCacheForReading => groupCache;

        private BuildingGroupUtility() { }

        private void BuildCache()
        {
            groupCache = new Dictionary<string, List<ThingDef>>();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef => thingDef.category == ThingCategory.Building && thingDef.BuildableByPlayer))
            {
                if (thingDef.HasModExtension<BuildingGroup>())
                {
                    foreach (var modExt in thingDef.modExtensions.Where(m => m is BuildingGroup).Select(m => m as BuildingGroup))
                    {
                        var buildingGroup = GetBuildGroup(modExt, thingDef);
                        SwitchBuilding.LogMessage(LogLevel.Debug, thingDef.defName, buildingGroup);
                        if (!groupCache.ContainsKey(buildingGroup))
                        {
                            groupCache.Add(buildingGroup, new List<ThingDef>());
                        }
                        var groupList = groupCache[buildingGroup];
                        var firstInGroup = groupList.FirstOrDefault();
                        if (firstInGroup != null)
                        {
                            if (firstInGroup.Size == thingDef.Size)
                            {
                                groupList.Add(thingDef);
                            }
                            else
                            {
                                SwitchBuilding.LogMessage(LogLevel.Error, "ThingDef", thingDef.defName, "does not match size of other thingDefs in group", buildingGroup);
                            }
                        }
                        else
                        {
                            groupList.Add(thingDef);
                        }
                    }
                }
            }
        }
        #endregion

        public static bool HasBuildingGroup(ThingDef thingDef)
        {
            return thingDef.HasModExtension<BuildingGroup>() && thingDef.BuildableByPlayer;
        }

        public static IEnumerable<ThingDef> GetOthersInBuildingGroup(ThingDef thingDef)
        {
            if (!thingDef.BuildableByPlayer)
            {
                yield break;
            }
            foreach (var modExt in thingDef.modExtensions.Where(m => m is BuildingGroup).Select(m => m as BuildingGroup))
            {
                var buildingGroup = GetBuildGroup(modExt, thingDef);
                var groupList = Instance.groupCache[buildingGroup];
                if (groupList != null)
                {
                    foreach (var otherThingDef in groupList)
                    {
                        if (otherThingDef.defName != thingDef.defName)
                        {
                            yield return otherThingDef;
                        }
                    }
                }
                else
                {
                    SwitchBuilding.LogMessage(LogLevel.Warning, "Encountered building group not in cache:", buildingGroup);
                }
            }
            yield break;
        }

        public static bool AreInSameBuildingGroup(ThingDef thingDef1, ThingDef thingDef2)
        {
            return GetOthersInBuildingGroup(thingDef1).Contains(thingDef2);
        }

        public static string GetBuildGroup(BuildingGroup bGroup, ThingDef thingDef)
        {
            return $"{bGroup.buildingGroup}_{thingDef.Size.x}_{thingDef.Size.z}";
        }
    }
}
