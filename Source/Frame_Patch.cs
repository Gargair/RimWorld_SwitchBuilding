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
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            UpgradeBuildings.LogMessage(LogLevel.Debug, "Patching Frame::CompleteConstruction");
            var jumpLabel = generator.DefineLabel();
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Isinst, typeof(Frame_ChangeBuilding));
            yield return new CodeInstruction(OpCodes.Brfalse, jumpLabel);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return CodeInstruction.Call(typeof(Frame_ChangeBuilding), nameof(Frame_ChangeBuilding.CustomCompleteConstruction), new Type[] { typeof(Pawn) });
            yield return new CodeInstruction(OpCodes.Ret);
            var finishIt = new CodeInstruction(OpCodes.Nop);
            finishIt.labels.Add(jumpLabel);
            yield return finishIt;
            foreach (var instruction in instructions)
            {
                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(Frame), nameof(Frame.MaterialsNeeded))]
    public class Frame_Patch_MaterialsNeeded
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            UpgradeBuildings.LogMessage(LogLevel.Debug, "Patching Frame::MaterialsNeeded");
            var jumpToAfterAdjustedCall = generator.DefineLabel();
            var jumpConditionLabel = generator.DefineLabel();
            var defField = AccessTools.Field(typeof(Thing), nameof(Thing.def));
            var adjustedCall = AccessTools.Method(typeof(CostListCalculator), nameof(CostListCalculator.CostListAdjusted), new Type[]
            {
                typeof(BuildableDef),
                typeof(ThingDef),
                typeof(bool)
            });
            foreach (var instruction in instructions)
            {
#pragma warning disable CS0252 // Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'FieldInfo'
                if (instruction.opcode == OpCodes.Ldfld && instruction.operand == defField)
                {
                    UpgradeBuildings.LogMessage(LogLevel.Debug, "Found def accessor. Patching in custom check.");
                    // Need to inject custom code.
                    // "this" object on stack at this time
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(Frame_ChangeBuilding));
                    yield return new CodeInstruction(OpCodes.Brfalse, jumpConditionLabel); // Brfalse pops the stack
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(Frame_ChangeBuilding), nameof(Frame_ChangeBuilding.CustomCostListAdjusted));

                    yield return new CodeInstruction(OpCodes.Br, jumpToAfterAdjustedCall);
                    yield return new CodeInstruction(OpCodes.Nop)
                    {
                        labels =
                        {
                            jumpConditionLabel
                        }
                    };
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    UpgradeBuildings.LogMessage(LogLevel.Debug, "Custom check patched.");
                }
#pragma warning restore CS0252 // Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'FieldInfo'
                yield return instruction;
#pragma warning disable CS0252 // Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'MethodInfo'
                if (instruction.opcode == OpCodes.Call && instruction.operand == adjustedCall)
                {
                    UpgradeBuildings.LogMessage(LogLevel.Debug, "Found adjustedcostlist call. Patching in jump label.");
                    // Add Finish jump target after call to adjustedcostlist
                    var finishIt = new CodeInstruction(OpCodes.Nop);
                    finishIt.labels.Add(jumpToAfterAdjustedCall);
                    yield return finishIt;
                }
#pragma warning restore CS0252 // Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'MethodInfo'
            }
        }
    }

    [HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
    internal class ReplaceFrameNoBlock
    {
        // Token: 0x0600004A RID: 74 RVA: 0x000036AA File Offset: 0x000018AA
        public static void Postfix(Thing constructible, Thing t, ref bool __result)
        {
            if (!__result)
            {
                return;
            }
            if (constructible is Frame_ChangeBuilding)
            {
                __result = false;
            }
        }
    }
}
