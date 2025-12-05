using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche.Sexuality.RPS_VRE_Compat
{
    [HarmonyPatch(typeof(Rimpsyche_Utility), nameof(Rimpsyche_Utility.GetPawnAge))]
    public static class Rimpsyche_Utility_GetPawnAge_Patch
    {
        public static bool Prefix(Pawn pawn, ref float __result)
        {
            if (HarmonyInit.androidPawnkindShorthash.Contains(pawn.kindDef.shortHash)) __result = 18f;
            return false;
        }
    }
}
