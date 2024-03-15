using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace SwitchBuilding
{
    public class Comp_ChangeBuilding : ThingComp
    {
        public ThingDef changeTo;
        public List<ThingDefCountClass> neededResources;
        public List<ThingDefCountClass> payBackResources;
        public CompProperties_ChangeBuilding Props => (CompProperties_ChangeBuilding)props;
        public Frame_ChangeBuilding placedFrame;

        private DesignationManager DesignationManager => parent?.Map?.designationManager;
        private bool HasChangeDesignation => DesignationManager?.DesignationOn(parent, SwitchBuildingDefOf.Designations.ChangeBuilding) != null;
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
            if (BuildingGroupUtility.Instance.AreInSameBuildingGroup(parent.def, thingDef) && parent.def != thingDef)
            {
                CancelChange();
                changeTo = thingDef;
                InitializeResources();
                if (DesignationManager == null)
                {
                    needDesignationAfterSpawn = true;
                    return;
                }
                var designation = DesignationManager.DesignationOn(parent, SwitchBuildingDefOf.Designations.ChangeBuilding);
                if (designation == null)
                {
                    DesignationManager.AddDesignation(new Designation(parent, SwitchBuildingDefOf.Designations.ChangeBuilding));
                }
                SwitchBuilding.LogMessage(LogLevel.Debug, "Creating Frame");
                Frame_ChangeBuilding frame = new Frame_ChangeBuilding();
                frame.def = FrameUtility.GetFrameDefForThingDef(thingDef);
                frame.SetStuffDirect(parent.Stuff);
                frame.PostMake();
                frame.PostPostMake();
                frame.StyleSourcePrecept = parent.StyleSourcePrecept;
                frame.StyleDef = parent.StyleDef;
                frame.thingToChange = parent;
                frame.SetFactionDirect(parent.Faction);
                SwitchBuilding.LogMessage(LogLevel.Debug, "Placing Frame");
                placedFrame = (Frame_ChangeBuilding)GenSpawn.Spawn(frame, parent.Position, parent.Map, parent.Rotation, WipeMode.Vanish);
            }
        }

        public void CancelChange()
        {
            changeTo = null;
            neededResources = null;
            payBackResources = null;
            if (DesignationManager != null)
            {
                var des = DesignationManager.DesignationOn(parent, SwitchBuildingDefOf.Designations.ChangeBuilding);
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
                    DesignationManager.AddDesignation(new Designation(parent, SwitchBuildingDefOf.Designations.ChangeBuilding));
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
                SwitchBuilding.GetResourceDifferenceForChange(parent, changeTo, out neededResources, out payBackResources);
                SwitchBuilding.LogMessage(LogLevel.Debug, "Initialized resources");
                SwitchBuilding.LogMessage(LogLevel.Debug, "needed resources");
                foreach (var c in neededResources)
                {
                    SwitchBuilding.LogMessage(LogLevel.Debug, c.thingDef.defName, ":", c.count.ToString());
                }
                SwitchBuilding.LogMessage(LogLevel.Debug, "payBack resources");
                foreach (var c in payBackResources)
                {
                    SwitchBuilding.LogMessage(LogLevel.Debug, c.thingDef.defName, ":", c.count.ToString());
                }
            }
            else if (changeTo == null)
            {
                neededResources = null;
                payBackResources = null;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (DesignationManager != null && !HasChangeDesignation)
            {
                if (changeTo != null ||
                neededResources != null ||
                payBackResources != null ||
                placedFrame != null)
                {
                    CancelChange();
                }
            }
        }
    }
}
