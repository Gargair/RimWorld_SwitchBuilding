using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UpgradeBuildings
{
    public static class UpgradeBuildingDefOf
    {
        [DefOf]
        public static class JobDefs
        {
            public static JobDef ChangeBuilding;
        }

        [DefOf]
        public static class WorkGivers
        {
            public static WorkGiverDef ChangeBuilding;
        }

        [DefOf]
        public static class Designations
        {
            public static DesignationDef ChangeBuilding;
        }
    }
}
