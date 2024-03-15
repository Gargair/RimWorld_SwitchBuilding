using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UpgradeBuildings
{
    public class WorkGiver_ChangeBuilding: WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn?.Map?.designationManager?.SpawnedDesignationsOfDef(UpgradeBuildingDefOf.Designations.ChangeBuilding).Select(des => des.target.Thing);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return true;
        }
    }
}
