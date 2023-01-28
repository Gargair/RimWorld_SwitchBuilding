using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UpgradeBuildings
{
    public class Comp_ChangeBuilding : ThingComp
    {
        public ThingDef changeTo;
        public List<ThingDefCountClass> neededResources;
        public List<ThingDefCountClass> payBackResources;
        public CompProperties_ChangeBuilding Props => (CompProperties_ChangeBuilding)props;

        private DesignationManager DesignationManager => parent?.Map?.designationManager;
        private bool HasChangeDesignation => DesignationManager?.DesignationOn(parent, UpgradeBuildingDefOf.Designations.ChangeBuilding) != null;
        private bool needDesignationAfterSpawn = false;

        public Comp_ChangeBuilding() { }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!HasChangeDesignation)
            {
                if (parent.Faction == Faction.OfPlayer && (parent.GetInnerIfMinified() is Building))
                {
                    yield return CreateChangeBuildingGizmo();
                }
            }
            yield break;
        }

        private Command CreateChangeBuildingGizmo()
        {
            return new Command_ChangeBuilding();
        }

        public void ChangeTo(ThingDef thingDef)
        {
            if (BuildingGroupUtility.Instance.AreInSameBuildingGroup(parent.def, thingDef))
            {
                changeTo = thingDef;
                InitializeResources();
                if (DesignationManager == null)
                {
                    needDesignationAfterSpawn = true;
                    return;
                }
                var designation = DesignationManager.DesignationOn(parent, UpgradeBuildingDefOf.Designations.ChangeBuilding);
                if (designation == null)
                {
                    DesignationManager.AddDesignation(new Designation(parent, UpgradeBuildingDefOf.Designations.ChangeBuilding));
                }
            }
        }

        public void CancelChange()
        {
            changeTo = null;
            neededResources = null;
            payBackResources = null;
            if (DesignationManager != null)
            {
                var des = DesignationManager.DesignationOn(parent, UpgradeBuildingDefOf.Designations.ChangeBuilding);
                if (des != null)
                {
                    DesignationManager.RemoveDesignation(des);
                };
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitializeResources();
            if (needDesignationAfterSpawn)
            {
                if (DesignationManager != null)
                {
                    needDesignationAfterSpawn = false;
                    DesignationManager.AddDesignation(new Designation(parent, UpgradeBuildingDefOf.Designations.ChangeBuilding));
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look<ThingDef>(ref changeTo, "UpgBldg.changeTo");
        }

        public override string CompInspectStringExtra()
        {
            if (HasChangeDesignation)
            {
                return "UpgBldg.Labels.ChangingTo".Translate(changeTo.LabelCap);
            }
            return base.CompInspectStringExtra();
        }

        private void InitializeResources()
        {
            if (changeTo != null && (neededResources == null || payBackResources == null))
            {
                var resourceDiff = UpgradeBuildings.GetResourceDifferenceForChange(parent, changeTo);
                neededResources = resourceDiff.Where(c => c.count > 0).ToList();
                payBackResources = resourceDiff.Where(c => c.count < 0).Select(c => new ThingDefCountClass(c.thingDef, -c.count)).ToList();
            }
            else if (changeTo == null && (neededResources != null || payBackResources != null))
            {
                neededResources = null;
                payBackResources = null;
            }
        }
    }
}
