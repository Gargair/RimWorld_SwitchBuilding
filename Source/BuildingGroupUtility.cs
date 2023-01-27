using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private Dictionary<string, List<ThingDef>> groupCache;

        private BuildingGroupUtility() { }

        private void BuildCache()
        {
            groupCache = new Dictionary<string, List<ThingDef>>();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef => thingDef.category == ThingCategory.Building))
            {
                var modExt = thingDef.GetModExtension<BuildingGroupModExtension>();
                if (modExt != null)
                {
                    if (!groupCache.ContainsKey(modExt.buildingGroup))
                    {
                        groupCache.Add(modExt.buildingGroup, new List<ThingDef>());
                        groupCache[modExt.buildingGroup].Add(thingDef);
                    }
                    else
                    {
                        
                    }
                }
            }
        }

        public bool HasBuildingGroup(ThingDef thingDef)
        {
            return false;
        }

        public IEnumerable<ThingDef> GetOthersInBuildingGroup(ThingDef thingDef)
        {
            yield break;
        }
    }
}
