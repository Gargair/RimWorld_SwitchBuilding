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
    public class Command_ChangeBuilding : Command_Action
    {
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => getFloatingOptions();

        public Command_ChangeBuilding()
        {
            icon = ContentFinder<Texture2D>.Get("UI/Change");
            defaultLabel = "UpgBldg.Labels.ChangeBuilding".Translate();
            defaultDesc = "UpgBldg.Tooltips.ChangeBuilding".Translate();
            hotKey = KeyBindingDefOf.Misc12;
        }

        private static void ChangeTo(ThingDef thingDef)
        {
            List<object> list = Find.Selector.SelectedObjects.FindAll((object o) => typeof(ThingWithComps).IsAssignableFrom(o.GetType()));
            foreach (object item in list)
            {
                if (item is ThingWithComps thing)
                {
                    Comp_ChangeBuilding changeBuildingComp = thing.TryGetComp<Comp_ChangeBuilding>();
                    if (changeBuildingComp != null)
                    {
                        changeBuildingComp.ChangeTo(thingDef);
                    }
                }
            }
        }

        private static IEnumerable<FloatMenuOption> getFloatingOptions()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            var allSelectedThings = Find.Selector.SelectedObjects.FindAll((object o) => typeof(ThingWithComps).IsAssignableFrom(o.GetType())).Cast<ThingWithComps>();
            var distinctChangeTos = allSelectedThings.Where((thing) => BuildingGroupUtility.Instance.HasBuildingGroup(thing.def)).SelectMany(thing => BuildingGroupUtility.Instance.GetOthersInBuildingGroup(thing.def)).Distinct();
            foreach(var thingDef in distinctChangeTos)
            {
                yield return new FloatMenuOption("UpgBldg.Labels.ChangeTo".Translate(thingDef.LabelCap), () => ChangeTo(thingDef));
            }
        }
    }
}
