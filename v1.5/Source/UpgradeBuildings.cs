using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UpgradeBuildings
{
    [StaticConstructorOnStartup]
    public static class UpgradeBuildings
    {
        static UpgradeBuildings()
        {
            LogMessage(LogLevel.Information, "Welcome to pointless log spam");
            var changeComp = new CompProperties_ChangeBuilding();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs
                        .Where(thingDef => thingDef.HasModExtension<BuildingGroup>())
                        .Where(thingDef => !thingDef.HasComp(typeof(CompProperties_ChangeBuilding))))
            {
                thingDef.comps.Add(changeComp);
            }
            LogMessage(LogLevel.Debug, "Finished adding comps to thingDefs");
            var harmony = new Harmony("rakros.rimworld.upgradebuildings");
            harmony.PatchAll();
            FrameUtility.AddCustomFrames();
        }

        public static LogLevel logLevel = LogLevel.Information;

        public static void LogMessage(LogLevel logLevel, params string[] messages)
        {
            var actualMessage = messages.Aggregate("[UpgradeBuildings]", (logMessage, message) => logMessage + " " + message);
            if (logLevel > UpgradeBuildings.logLevel)
            {
                return;
            }
            switch (logLevel)
            {
                case LogLevel.Error:
                    Log.Error(actualMessage);
                    break;
                case LogLevel.Warning:
                    Log.Warning(actualMessage);
                    break;
                default:
                    Log.Message(actualMessage);
                    break;
            }
        }

        public static void GetResourceDifferenceForChange(Thing source, ThingDef target, out List<ThingDefCountClass> neededResources, out List<ThingDefCountClass> refundedResources)
        {
            LogMessage(LogLevel.Debug, "GetNeededResourcesForChange", source.def.defName, "=>", target.defName);
            neededResources = new List<ThingDefCountClass>();
            refundedResources = new List<ThingDefCountClass>();
            var sourceCostList = source.CostListAdjusted();
            List<ThingDefCountClass> targetCostList;
            if (source.def.MadeFromStuff && target.MadeFromStuff)
            {
                LogMessage(LogLevel.Debug, "Both made from stuff");
                if (GenStuff.AllowedStuffsFor(target).Contains(source.Stuff))
                {
                    LogMessage(LogLevel.Debug, "Stuff can be taken over");
                    targetCostList = target.CostListAdjusted(source.Stuff);
                }
                else
                {
                    LogMessage(LogLevel.Debug, "Stuff can not be taken over");
                    targetCostList = target.CostListAdjusted(target.defaultStuff);
                }
            }
            else if (target.MadeFromStuff)
            {
                LogMessage(LogLevel.Debug, "Only target made from stuff");
                targetCostList = target.CostListAdjusted(target.defaultStuff);
            }
            else
            {
                targetCostList = target.CostListAdjusted(null);
            }

            var join = sourceCostList.FullOuterJoin(targetCostList, s => s.thingDef.defName, t => t.thingDef.defName, (sc, tc, defName) =>
            {
                int sourceCount = 0;
                int targetCount = 0;
                ThingDef thingDef = null;
                if (sc != null)
                {
                    sourceCount = sc.count;
                    thingDef = sc.thingDef;
                }
                if (tc != null)
                {
                    targetCount = tc.count;
                    thingDef = tc.thingDef;
                }
                return new
                {
                    thingDef = thingDef,
                    count = targetCount - sourceCount
                };
            });

            foreach (var i in join)
            {
                if (i.count > 0)
                {
                    neededResources.Add(new ThingDefCountClass(i.thingDef, i.count));
                }
                else if (i.count < 0)
                {
                    refundedResources.Add(new ThingDefCountClass(i.thingDef, -i.count));

                }
            }
        }

        internal static IEnumerable<TResult> FullOuterJoin<TA, TB, TKey, TResult>(
        this IEnumerable<TA> a,
        IEnumerable<TB> b,
        Func<TA, TKey> selectKeyA,
        Func<TB, TKey> selectKeyB,
        Func<TA, TB, TKey, TResult> projection,
        TA defaultA = default(TA),
        TB defaultB = default(TB),
        IEqualityComparer<TKey> cmp = null)
        {
            cmp = cmp ?? EqualityComparer<TKey>.Default;
            var alookup = a.ToLookup(selectKeyA, cmp);
            var blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TKey>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                       from xa in alookup[key].DefaultIfEmpty(defaultA)
                       from xb in blookup[key].DefaultIfEmpty(defaultB)
                       select projection(xa, xb, key);

            return join;
        }
    }

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
        Debug = 4,
    }
}
