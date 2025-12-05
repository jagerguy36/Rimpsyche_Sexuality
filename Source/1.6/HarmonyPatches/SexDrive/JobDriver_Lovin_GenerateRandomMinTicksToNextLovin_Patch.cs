using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(JobDriver_Lovin), "GenerateRandomMinTicksToNextLovin")]
    public static class JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
    {
        //Remove vanilla orientation block.
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            MethodInfo getMultiplier = AccessTools.Method(typeof(JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch), nameof(JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch.GetSexdriveMultiplier));

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 2500f)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, getMultiplier);
                    yield return new CodeInstruction(OpCodes.Mul);
                }
            }
        }
        public static float GetSexdriveMultiplier(Pawn initiator)
        {
            var initPsyche = initiator.compPsyche();
            if (initPsyche?.Enabled == true)
            {
                //cf rjw sex_drive clamped low at 0.05f
                var sexDrive = initPsyche.Sexuality.GetAdjustedSexdrive();
                var nextTickFactor = 1f / sexDrive;
                return nextTickFactor;
            }
            return 1f;
        }
    }
}