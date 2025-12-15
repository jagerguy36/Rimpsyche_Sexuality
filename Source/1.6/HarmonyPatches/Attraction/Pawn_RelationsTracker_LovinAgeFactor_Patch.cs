using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.LovinAgeFactor))]
    public static class Pawn_RelationsTracker_LovinAgeFactor_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            MethodInfo multiplierMethod = AccessTools.Method(typeof(Pawn_RelationsTracker_LovinAgeFactor_Patch), nameof(Pawn_RelationsTracker_LovinAgeFactor_Patch.LovinAgeFactorCalculator));
            bool lovinAgePatched = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!lovinAgePatched && codes[i].opcode == OpCodes.Mul)
                {
                    lovinAgePatched = true;
                    yield return codes[i + 1];
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, multiplierMethod);
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return new CodeInstruction(OpCodes.Ret);
                    break;
                }
                yield return codes[i];
            }
            if (!lovinAgePatched)
            {
                Log.Error("[Rimpsyche - Sexuality] Could not patch LovinAgeFactor. Pawn sexuality will not work as intended");
            }
        }

        public static float LovinAgeFactorCalculator(float num2, float num3, float pawnAge, float otherPawnAge)
        {
            float originalMult = num2 * num3;
            float diff = Mathf.Abs(pawnAge - otherPawnAge);
            if (diff <= 2) return 1f;
            float diffFactor = Mathf.Max(5f - 2f * diff,0);
            return Mathf.Max(originalMult, diffFactor);
        }
    }
}
