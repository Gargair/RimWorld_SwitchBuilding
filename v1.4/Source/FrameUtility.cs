using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UpgradeBuildings
{
    internal class FrameUtility
    {
        private static readonly Dictionary<ThingDef, ThingDef> frameCache = new Dictionary<ThingDef, ThingDef>();

        public static ThingDef GetFrameDefForThingDef(ThingDef def)
        {
            if (frameCache.ContainsKey(def)) return frameCache[def];
            var frameDef = NewReplaceFrameDef_Thing(def);
            frameCache.Add(def, frameDef);
            return frameDef;
        }

        public static bool IsChangeBuildingFrame(ThingDef def)
        {
            return def != null && frameCache.ContainsValue(def);
        }

        public static bool IsChangeBuildingFrame(Thing thing)
        {
            return thing != null && IsChangeBuildingFrame(thing.def);
        }

        public static void AddCustomFrames()
        {
            Type typeFromHandle = typeof(ThingDef);
            HashSet<ushort> h = ((Dictionary<Type, HashSet<ushort>>)AccessTools.Field(typeof(ShortHashGiver), "takenHashesPerDeftype").GetValue(null))[typeFromHandle];
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>().Where(td => td.HasComp(typeof(Comp_ChangeBuilding))))
            {
                ThingDef thingDef2 = NewReplaceFrameDef_Thing(thingDef);
                frameCache[thingDef] = thingDef2;
                GiveShortHash(thingDef2, typeFromHandle, h);
                thingDef2.PostLoad();
                DefDatabase<ThingDef>.Add(thingDef2);
            }
        }

        private static ThingDef NewReplaceFrameDef_Thing(ThingDef def)
        {
            ThingDef thingDef = BaseFrameDef();
            thingDef.defName = def.defName + "_ChangeBuilding";
            thingDef.label = def.label + "UpgBldg.Labels.ChangingBuilding".Translate();
            thingDef.size = def.size;
            thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)def.BaseMaxHitPoints * 0.25f);
            thingDef.SetStatBaseValue(StatDefOf.Beauty, -8f);
            thingDef.fillPercent = 0.2f;
            thingDef.pathCost = 10;
            thingDef.description = def.description;
            thingDef.passability = def.passability;
            thingDef.selectable = def.selectable;
            thingDef.constructEffect = def.constructEffect;
            thingDef.building.isEdifice = false;
            thingDef.constructionSkillPrerequisite = def.constructionSkillPrerequisite;
            thingDef.clearBuildingArea = false;
            thingDef.drawPlaceWorkersWhileSelected = def.drawPlaceWorkersWhileSelected;
            thingDef.stuffCategories = def.stuffCategories;
            thingDef.entityDefToBuild = def;
            thingDef.modContentPack = LoadedModManager.GetMod<Mod>().Content;
            return thingDef;
        }

        private static ThingDef BaseFrameDef()
        {
            return new ThingDef
            {
                isFrameInt = true,
                category = ThingCategory.Building,
                label = "Unspecified building change frame",
                thingClass = typeof(Frame_ChangeBuilding),
                altitudeLayer = AltitudeLayer.BuildingOnTop,
                useHitPoints = true,
                selectable = true,
                building = new BuildingProperties(),
                comps =
                {
                    new CompProperties_Forbiddable()
                },
                scatterableOnMapGen = false,
                leaveResourcesWhenKilled = true
            };
        }

        private static GiveShortHashDel GiveShortHash = AccessTools.MethodDelegate<GiveShortHashDel>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash", null, null), null, true);
        private delegate void GiveShortHashDel(Def d, Type t, HashSet<ushort> h);
    }
}
