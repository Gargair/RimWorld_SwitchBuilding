using RimWorld;
using Verse;

namespace SwitchBuilding
{
    public static class SwitchBuildingDefOf
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
