using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace SwitchBuilding
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
                return Comp?.neededResources;
            }
        }
        public List<ThingDefCountClass> RefundedResources
        {
            get
            {
                return Comp?.payBackResources;
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
            SwitchBuilding.LogMessage(LogLevel.Debug, "CustomCompleteConstruction");
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
            var stuff = thingToChange.Stuff;

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
                CopyComps(thingToChange, thing, worker);

                thing.HitPoints = (int)Math.Floor(((float)thingToChange.HitPoints / (float)thingToChange.MaxHitPoints) * (float)thing.MaxHitPoints);

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

                        Map.mapDrawer.MapMeshDirty(position, MapMeshFlagDefOf.PowerGrid);
                        Map.mapDrawer.MapMeshDirty(position, MapMeshFlagDefOf.Things);
                    }
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
                Color? ideoColorForBuilding = ChangeTo != null ? IdeoUtility.GetIdeoColorForBuilding(ChangeTo, base.Faction) : null;
                var resourcesToRefund = RefundedResources;
                Map.designationManager.RemoveAllDesignationsOn(thingToChange);
                thingToChange.Destroy(DestroyMode.WillReplace);
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
                this.SetStorageGroup(null);
                if (ideoColorForBuilding != null)
                {
                    thing.SetColor(ideoColorForBuilding.Value, true);
                }

                if (resourcesToRefund != null)
                {
                    foreach (var item2 in resourcesToRefund)
                    {
                        var thing3 = ThingMaker.MakeThing(item2.thingDef);
                        thing3.stackCount = item2.count;
                        GenPlace.TryPlaceThing(thing3, position, map, ThingPlaceMode.Near);
                    }
                }

            }
            else
            {
                SwitchBuilding.LogMessage(LogLevel.Debug, "ChangeTo is empty");
            }
            if (!this.Destroyed)
            {
                this.Destroy(DestroyMode.Vanish);
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

        private static void CopyComps(Thing oldThing, Thing newThing, Pawn worker)
        {
            // Quality and Art
            var hasQuality = oldThing.TryGetQuality(out var targetqc);
            var compQuality = newThing.TryGetComp<CompQuality>();
            if (hasQuality && compQuality != null)
            {
                compQuality.SetQuality(targetqc, ArtGenerationContext.Colony);
            }
            else if (compQuality != null)
            {
                compQuality.SetQuality(QualityUtility.GenerateQuality(QualityGenerator.BaseGen), ArtGenerationContext.Colony);
            }
            CompArt compArt = newThing.TryGetComp<CompArt>();
            if (compArt != null)
            {
                if (compQuality == null)
                {
                    compArt.InitializeArt(ArtGenerationContext.Colony);
                }
                compArt.JustCreatedBy(worker);
            }

            // Check Refuelable
            var compRefuelable = oldThing.TryGetComp<CompRefuelable>();
            if (compRefuelable != null)
            {
                var num = Mathf.CeilToInt(compRefuelable.Fuel);
                var fuelDef = compRefuelable.Props.fuelFilter.AllowedThingDefs.First();
                if (fuelDef != null && num > 0)
                {
                    var fuel = ThingMaker.MakeThing(fuelDef);
                    fuel.stackCount = num;
                    GenPlace.TryPlaceThing(fuel, oldThing.Position, oldThing.Map, ThingPlaceMode.Near);
                }
            }

            // Copy BillStack
            if (oldThing is Building_WorkTable oldWorkTable && newThing is Building_WorkTable newWorkTable)
            {
                foreach (var item in oldWorkTable.BillStack)
                {
                    newWorkTable.BillStack.AddBill(item);
                }
                newWorkTable.BillStack.RemoveIncompletableBills();
            }

            // Copy Storage Settings
            if (oldThing is IStorageGroupMember oldMember && newThing is IStorageGroupMember newMember)
            {
                newMember.SetStorageGroup(oldMember.Group);
            }
            if (oldThing is Building_Storage oldStorage && oldStorage.settings != null && newThing is Building_Storage newStorage && newStorage.settings != null)
            {
                newStorage.settings.CopyFrom(oldStorage.settings);
            }
        }
    }
}
