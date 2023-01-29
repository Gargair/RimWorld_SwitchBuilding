using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace UpgradeBuildings
{
    public class Command_ChangeBuilding : Command_Action
    {
        public Command_ChangeBuilding()
        {
            icon = ContentFinder<Texture2D>.Get("UI/Change");
            defaultLabel = "UpgBldg.Labels.ChangeBuilding".Translate();
            defaultDesc = "UpgBldg.Tooltips.ChangeBuilding".Translate();
            action = () => Find.WindowStack.Add(new FloatMenu(GetFloatingOptions().ToList())
            {
                vanishIfMouseDistant = true
            });
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
                        changeBuildingComp.SetChangeTo(thingDef);
                    }
                }
            }
        }

        private static IEnumerable<FloatMenuOption> GetFloatingOptions()
        {
            var allSelectedThings = Find.Selector.SelectedObjects.FindAll((object o) => typeof(ThingWithComps).IsAssignableFrom(o.GetType())).Cast<ThingWithComps>();
            var distinctChangeTos = allSelectedThings.Where((thing) => BuildingGroupUtility.Instance.HasBuildingGroup(thing.def)).SelectMany(thing => BuildingGroupUtility.Instance.GetOthersInBuildingGroup(thing.def)).Distinct();
            foreach (var thingDef in distinctChangeTos)
            {
                var option = new FloatMenuOption("UpgBldg.Labels.ChangeTo".Translate(thingDef.LabelCap), () => ChangeTo(thingDef), thingDef);
                if (thingDef.researchPrerequisites != null)
                {
                    var unFinishedResearch = thingDef.researchPrerequisites.Where(res => !res.IsFinished);
                    if (unFinishedResearch != null && unFinishedResearch.Count() > 0)
                    {
                        option.Disabled = true;
                        option.tooltip = "UpgBldg.Tooltips.ResearchMissing".Translate(string.Join(", ", unFinishedResearch.Select(resDef => resDef.LabelCap)));
                    }
                }
                yield return option;
            }
        }
    }
}
