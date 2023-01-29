using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace UpgradeBuildings
{
    public class Frame_ChangeBuilding : Frame
    {
        public ThingWithComps thingToChange;
        public ThingDef ChangeTo
        {
            get
            {
                return Comp?.changeTo;
            }
        }
        public List<ThingDefCountClass> NeededResources
        {
            get
            {
                return Comp.neededResources;
            }
        }
        public List<ThingDefCountClass> RefundedResources
        {
            get
            {
                return Comp.payBackResources;
            }
        }

        private Comp_ChangeBuilding Comp => thingToChange?.GetComp<Comp_ChangeBuilding>();

        public override void Notify_KilledLeavingsLeft(List<Thing> leavings)
        {
            base.Notify_KilledLeavingsLeft(leavings);
            if (thingToChange != null)
            {
                var comp = thingToChange.GetComp<Comp_ChangeBuilding>();
                if (comp != null)
                {
                    comp.CancelChange();
                }
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref thingToChange, "UpgBldg.thingToChange");
        }

        public void CustomCompleteConstruction(Pawn worker)
        {
            List<CompHasSources> list = new List<CompHasSources>();
            for (int i = 0; i < this.resourceContainer.Count; i++)
            {
                CompHasSources compHasSources = this.resourceContainer[i].TryGetComp<CompHasSources>();
                if (compHasSources != null)
                {
                    list.Add(compHasSources);
                }
            }
            this.resourceContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            Map map = base.Map;
            bool isSelected = Find.Selector.IsSelected(this) || Find.Selector.IsSelected(thingToChange);

            var position = thingToChange.Position;
            var rotation = thingToChange.Rotation;
            BillStack billStack = null;
            var stuff = thingToChange.Stuff;
            var hasQuality = thingToChange.TryGetQuality(out var targetqc);
            if (thingToChange is Building_WorkTable Building)
            {
                billStack = Building.BillStack;
            }

            var compRefuelable = thingToChange.TryGetComp<CompRefuelable>();
            if (compRefuelable != null)
            {
                var num = Mathf.CeilToInt(compRefuelable.Fuel);
                var fuelDef = compRefuelable.Props.fuelFilter.AllowedThingDefs.First();
                if (fuelDef != null && num > 0)
                {
                    var fuel = ThingMaker.MakeThing(fuelDef);
                    fuel.stackCount = num;
                    GenPlace.TryPlaceThing(fuel, position, Map, ThingPlaceMode.Near);
                }
            }

            Map.designationManager.RemoveAllDesignationsOn(thingToChange);
            thingToChange.Destroy(DestroyMode.WillReplace);
            this.Destroy(DestroyMode.Vanish);
            if (this.GetStatValue(StatDefOf.WorkToBuild, true, -1) > 150f && this.def.entityDefToBuild is ThingDef && ((ThingDef)this.def.entityDefToBuild).category == ThingCategory.Building)
            {
                SoundDefOf.Building_Complete.PlayOneShot(new TargetInfo(base.Position, map, false));
            }
            Thing thing = null;
            if (ChangeTo != null)
            {
                if (thingToChange.def.MadeFromStuff && ChangeTo.MadeFromStuff)
                {
                    thing = ThingMaker.MakeThing(ChangeTo, thingToChange.Stuff);
                }
                else if (ChangeTo.MadeFromStuff)
                {
                    thing = ThingMaker.MakeThing(ChangeTo, ChangeTo.defaultStuff);
                }
                else
                {
                    thing = ThingMaker.MakeThing(ChangeTo, null);
                }
                thing.SetFactionDirect(base.Faction);
                var compQuality = thing.TryGetComp<CompQuality>();
                if (hasQuality && compQuality != null)
                {
                    compQuality.SetQuality(targetqc, ArtGenerationContext.Colony);
                }
                thing.HitPoints = (int)Math.Floor(((float)thingToChange.HitPoints / (float)thingToChange.MaxHitPoints) * (float)thing.MaxHitPoints);
                if (billStack != null && thing is Building_WorkTable workTable)
                {
                    foreach (var item in billStack)
                    {
                        workTable.BillStack.AddBill(item);
                    }
                    workTable.BillStack.RemoveIncompletableBills();
                }
                var compPower = thing.TryGetComp<CompPower>();
                if (compPower != null)
                {
                    var compPower2 = PowerConnectionMaker.BestTransmitterForConnector(position, Map);
                    if (compPower2 != null)
                    {
                        compPower.ConnectToTransmitter(compPower2);
                        for (var i = 0; i < 5; i++)
                        {
                            FleckMaker.ThrowMetaPuff(position.ToVector3Shifted(), Map);
                        }

                        Map.mapDrawer.MapMeshDirty(position, MapMeshFlag.PowerGrid);
                        Map.mapDrawer.MapMeshDirty(position, MapMeshFlag.Things);
                    }
                }

                CompArt compArt = thing.TryGetComp<CompArt>();
                if (compArt != null)
                {
                    if (compQuality == null)
                    {
                        compArt.InitializeArt(ArtGenerationContext.Colony);
                    }
                    compArt.JustCreatedBy(worker);
                }
                CompHasSources compHasSources2 = thing.TryGetComp<CompHasSources>();
                if (compHasSources2 != null && !list.NullOrEmpty<CompHasSources>())
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        list[j].TransferSourcesTo(compHasSources2, -1);
                    }
                }
                if (ChangeTo.CanBeStyled() && thingToChange.StyleDef != null)
                {
                    thing.SetStyleDef(thingToChange.StyleDef);
                }
                else if (ChangeTo.CanBeStyled() && this.GetIdeoForStyle(worker) != null)
                {
                    thing.StyleDef = base.StyleDef;
                }
                thing.HitPoints = Mathf.CeilToInt((float)this.HitPoints / (float)base.MaxHitPoints * (float)thing.MaxHitPoints);
                GenSpawn.Spawn(thing, position, map, rotation, WipeMode.FullRefund, false);
                Building building;
                if ((building = (thing as Building)) != null)
                {
                    Lord lord = worker.GetLord();
                    if (lord != null)
                    {
                        lord.AddBuilding(building);
                    }
                    building.StyleSourcePrecept = base.StyleSourcePrecept;
                }
                IStorageGroupMember member;
                if ((member = (thing as IStorageGroupMember)) != null)
                {
                    member.SetStorageGroup(this.storageGroup);
                }
                Building_Storage building_Storage;
                if ((building_Storage = (thing as Building_Storage)) != null && this.storageSettings != null)
                {
                    building_Storage.settings.CopyFrom(this.storageSettings);
                }
                this.SetStorageGroup(null);
                if (ChangeTo != null)
                {
                    Color? ideoColorForBuilding = IdeoUtility.GetIdeoColorForBuilding(ChangeTo, base.Faction);
                    if (ideoColorForBuilding != null)
                    {
                        thing.SetColor(ideoColorForBuilding.Value, true);
                    }
                }

                if (RefundedResources == null)
                {
                    return;
                }

                foreach (var item2 in RefundedResources)
                {
                    var thing3 = ThingMaker.MakeThing(item2.thingDef);
                    thing3.stackCount = item2.count;
                    GenPlace.TryPlaceThing(thing3, position, Map, ThingPlaceMode.Near);
                }
            }

            worker.records.Increment(RecordDefOf.ThingsConstructed);
            if (thing != null && thing.GetStatValue(StatDefOf.WorkToBuild, true, -1) >= 9500f)
            {
                TaleRecorder.RecordTale(TaleDefOf.CompletedLongConstructionProject, new object[]
                {
                    worker,
                    thing.def
                });
            }
            if (thing != null && isSelected)
            {
                Find.Selector.Select(thing, false, false);
            }
            CompGlower compGlower;
            if (this.glowerColorOverride != null && (compGlower = ((thing != null) ? thing.TryGetComp<CompGlower>() : null)) != null)
            {
                compGlower.GlowColor = this.glowerColorOverride.Value;
            }
        }

        private Ideo GetIdeoForStyle(Pawn worker)
        {
            if (worker.Ideo != null)
            {
                return worker.Ideo;
            }
            if (ModsConfig.BiotechActive && worker.IsColonyMech)
            {
                Pawn overseer = worker.GetOverseer();
                if (((overseer != null) ? overseer.Ideo : null) != null)
                {
                    return overseer.Ideo;
                }
            }
            return null;
        }

        public List<ThingDefCountClass> CustomCostListAdjusted()
        {
            return NeededResources;
        }
    }
}
