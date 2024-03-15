using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace SwitchBuilding
{
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public class Frame_Patch_CompleteConstruction
    {
        static bool Prefix(Frame __instance, Pawn worker)
        {
            SwitchBuilding.LogMessage(LogLevel.Debug, "CompleteConstruction Prefix");
            if (__instance is Frame_ChangeBuilding frame)
            {
                SwitchBuilding.LogMessage(LogLevel.Debug, "Found custom Frame");
                frame.CustomCompleteConstruction(worker);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Frame), nameof(Frame.TotalMaterialCost))]
    public class Frame_Patch_TotalMaterialCost
    {
        static bool Prefix(Frame __instance, ref List<ThingDefCountClass> __result)
        {
            if (__instance is Frame_ChangeBuilding frame)
            {
                __result = new List<ThingDefCountClass>();
                var costList = frame.CustomCostListAdjusted();
                if (costList != null)
                {
                    __result = costList;
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
