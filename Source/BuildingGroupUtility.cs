using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UpgradeBuildings
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

        private BuildingGroupUtility() { }

        private void BuildCache()
        {
            groupCache = new Dictionary<string, List<ThingDef>>();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef => thingDef.category == ThingCategory.Building))
            {
                var modExt = thingDef.GetModExtension<BuildingGroup>();
                if (modExt != null)
                {
                    if (!groupCache.ContainsKey(modExt.buildingGroup))
                    {
                        groupCache.Add(modExt.buildingGroup, new List<ThingDef>());
                    }
                    var groupList = groupCache[modExt.buildingGroup];
                    var firstInGroup = groupList.FirstOrDefault();
                    if (firstInGroup != null)
                    {
                        if (firstInGroup.Size == thingDef.Size)
                        {
                            groupList.Add(thingDef);
                        }
                        else
                        {
                            UpgradeBuildings.LogMessage(LogLevel.Error, "ThingDef", thingDef.defName, "does not match size of other thingDefs in group", modExt.buildingGroup);
                        }
                    }
                    else
                    {
                        groupList.Add(thingDef);
                    }
                }
            }
        }
        #endregion

        public bool HasBuildingGroup(ThingDef thingDef)
        {
            return thingDef.GetModExtension<BuildingGroup>() != null;
        }

        public IEnumerable<ThingDef> GetOthersInBuildingGroup(ThingDef thingDef)
        {
            var modExt = thingDef.GetModExtension<BuildingGroup>();
            if (modExt != null)
            {
                var groupList = groupCache[modExt.buildingGroup];
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
                    UpgradeBuildings.LogMessage(LogLevel.Warning, "Encountered building group not in cache:", modExt.buildingGroup);
                }
            }
            yield break;
        }

        public bool AreInSameBuildingGroup(ThingDef thingDef1, ThingDef thingDef2)
        {
            var modExt1 = thingDef1.GetModExtension<BuildingGroup>();
            var modExt2 = thingDef2.GetModExtension<BuildingGroup>();
            if (modExt1 != null && modExt2 != null)
            {
                return modExt1.buildingGroup == modExt2.buildingGroup && thingDef1.Size == thingDef2.Size;
            }
            return false;
        }
    }
}
