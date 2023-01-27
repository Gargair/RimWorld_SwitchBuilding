using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UpgradeBuildings
{
    public class Comp_ChangeBuilding : ThingComp
    {
        public ThingDef changeTo;
        public CompProperties_ChangeBuilding Props => (CompProperties_ChangeBuilding)props;

        private DesignationManager DesignationManager => parent?.Map?.designationManager;
        private bool HasChangeDesignation => DesignationManager?.DesignationOn(parent, UpgradeBuildingDefOf.Designations.ChangeBuilding) != null;
        private bool needDesignationAfterSpawn = false;

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
            Scribe_Values.Look<ThingDef>(ref changeTo, "UpgBldg.changeTo");
        }

        public override string CompInspectStringExtra()
        {
            if (HasChangeDesignation)
            {
                return "UpgBldg.Labels.ChangingTo".Translate(changeTo.LabelCap);
            }
            return base.CompInspectStringExtra();
        }
    }
}
