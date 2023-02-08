using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace UpgradeBuildings
{
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public class Frame_Patch_CompleteConstruction
    {
        static bool Prefix(Frame __instance, Pawn worker)
        {
            UpgradeBuildings.LogMessage(LogLevel.Debug, "CompleteConstruction Prefix");
            if (__instance is Frame_ChangeBuilding frame)
            {
                UpgradeBuildings.LogMessage(LogLevel.Debug, "Found custom Frame");
                frame.CustomCompleteConstruction(worker);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Frame), nameof(Frame.MaterialsNeeded))]
    public class Frame_Patch_MaterialsNeeded
    {
        static bool Prefix(Frame __instance, ref List<ThingDefCountClass> __result)
        {
            if (__instance is Frame_ChangeBuilding frame)
            {
                __result = new List<ThingDefCountClass>();
                foreach (var thingDefCountClass in frame.CustomCostListAdjusted())
                {
                    int countInContainer = __instance.resourceContainer.TotalStackCountOfDef(thingDefCountClass.thingDef);
                    int countNeeded = thingDefCountClass.count - countInContainer;
                    if (countNeeded > 0)
                    {
                        __result.Add(new ThingDefCountClass(thingDefCountClass.thingDef, countNeeded));
                    }
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
    internal class ReplaceFrameNoBlock
    {
        public static void Postfix(Thing constructible, Thing t, ref bool __result)
        {
            if (!__result)
            {
                return;
            }
            if (FrameUtility.IsChangeBuildingFrame(constructible))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
    internal static class ReplaceFrameNoWipe
    {
        public static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
        {
            var newThing = newEntDef as ThingDef;
            if (newThing != null && FrameUtility.IsChangeBuildingFrame(newThing))
            {
                __result = false;
                return;
            }
        }
    }
}
