using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch]
    public static class JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(JobDriver_Lovin), "GenerateRandomMinTicksToNextLovin");
            //Relationship module's copy (used by its own lovin drivers).
            if (Rimpsyche.RelationshipModuleLoaded)
                yield return AccessTools.Method("Maux36.RimPsyche.Relationship.RPR_LovinUtility:GenerateRandomMinTicksToNextLovin");
        }
        //Lovin interval x 1/sexdrive.
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var codes = new List<CodeInstruction>(instructions);

            MethodInfo getMultiplier = AccessTools.Method(typeof(JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch), nameof(JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch.GetSexdriveMultiplier));

            bool patched = false;
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 2500f)
                {
                    //Vanilla: instance (pawn = arg1). Relationship: static (pawn = arg0).
                    yield return new CodeInstruction(original.IsStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, getMultiplier);
                    yield return new CodeInstruction(OpCodes.Mul);
                    patched = true;
                }
            }
            if (!patched)
                Log.Error("[Rimpsyche - Sexuality] Lovin interval transpiler failed to patch");
        }
        public static float GetSexdriveMultiplier(Pawn initiator)
        {
            var initPsyche = initiator.compPsyche();
            if (initPsyche?.Enabled == true)
            {
                //cf rjw sex_drive clamped low at 0.05f
                var sexDrive = Mathf.Max(initPsyche.Sexuality.GetAdjustedSexdrive(), 0.05f);
                var nextTickFactor = 1f / sexDrive;
                return nextTickFactor;
            }
            return 1f;
        }
    }
}