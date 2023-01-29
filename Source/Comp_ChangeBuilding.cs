using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace UpgradeBuildings
{
    public class Comp_ChangeBuilding : ThingComp
    {
        public ThingDef changeTo;
        public List<ThingDefCountClass> neededResources;
        public List<ThingDefCountClass> payBackResources;
        public CompProperties_ChangeBuilding Props => (CompProperties_ChangeBuilding)props;
        public Frame_ChangeBuilding placedFrame;

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

        public void SetChangeTo(ThingDef thingDef)
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
                UpgradeBuildings.LogMessage(LogLevel.Debug, "Creating Frame");
                Frame_ChangeBuilding frame = new Frame_ChangeBuilding();
                frame.def = FrameUtility.GetFrameDefForThingDef(thingDef);
                frame.SetStuffDirect(parent.Stuff);
                frame.PostMake();
                frame.PostPostMake();
                frame.StyleSourcePrecept = parent.StyleSourcePrecept;
                frame.StyleDef = parent.StyleDef;
                UpgradeBuildings.LogMessage(LogLevel.Debug, "Placing Frame");
                GenSpawn.WipeExistingThings(parent.Position, parent.Rotation, thingDef, parent.Map, DestroyMode.Deconstruct);
                placedFrame = (Frame_ChangeBuilding)GenSpawn.Spawn(frame, parent.Position, parent.Map, parent.Rotation, WipeMode.FullRefund);
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
            if (placedFrame != null)
            {
                if (placedFrame.Spawned)
                {
                    placedFrame.Destroy(DestroyMode.Cancel);
                }
                placedFrame = null;
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
            Scribe_References.Look<Frame_ChangeBuilding>(ref placedFrame, "UpgBldg.placedFrame");
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
            if (changeTo != null)
            {
                var resourceDiff = UpgradeBuildings.GetResourceDifferenceForChange(parent, changeTo);
                neededResources = resourceDiff.Where(c => c.count > 0).ToList();
                payBackResources = resourceDiff.Where(c => c.count < 0).Select(c => new ThingDefCountClass(c.thingDef, -c.count)).ToList();
                UpgradeBuildings.LogMessage(LogLevel.Debug, "Initialized resources");
                UpgradeBuildings.LogMessage(LogLevel.Debug, "needed resources");
                foreach (var c in neededResources)
                {
                    UpgradeBuildings.LogMessage(LogLevel.Debug, c.thingDef.defName, ":", c.count.ToString());
                }
                UpgradeBuildings.LogMessage(LogLevel.Debug, "payBack resources");
                foreach (var c in payBackResources)
                {
                    UpgradeBuildings.LogMessage(LogLevel.Debug, c.thingDef.defName, ":", c.count.ToString());
                }
            }
            else if (changeTo == null)
            {
                neededResources = null;
                payBackResources = null;
            }
        }

        

    }
}
